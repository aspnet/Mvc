// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.FileSystems;
using Microsoft.AspNet.Razor;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class RazorCompilationService : IRazorCompilationService
    {
        // This class must be registered as a singleton service for the caching to work.
        private readonly CompilerCache _cache;
        private readonly IApplicationEnvironment _environment;
        private readonly ICompilationService _baseCompilationService;
        private readonly IMvcRazorHost _razorHost;
        private readonly string _appRoot;

        public RazorCompilationService(IApplicationEnvironment environment,
                                       ICompilationService compilationService,
                                       IControllerAssemblyProvider _controllerAssemblyProvider,
                                       IMvcRazorHost razorHost)
        {
            _environment = environment;
            _baseCompilationService = compilationService;
            _razorHost = razorHost;
            _appRoot = RelativePath.EnsureTrailingSlash(environment.ApplicationBasePath);
            _cache = new CompilerCache(_controllerAssemblyProvider.CandidateAssemblies);
        }

        public CompilationResult Compile([NotNull] IFileInfo file)
        {
            var relativePath = RelativePath.GetRelativePath(_appRoot, file);
            var fileInfo = new RelativeFileInfo()
            {
                FileInfo = file,
                RelativePath = relativePath,
            };

            return _cache.GetOrAdd(fileInfo, () => CompileCore(file, relativePath));
        }

        internal CompilationResult CompileCore(IFileInfo file, string relativePath)
        {
            GeneratorResults results;
            using (var inputStream = file.CreateReadStream())
            {
                results = _razorHost.GenerateCode(
                    relativePath, inputStream);
            }

            if (!results.Success)
            {
                var messages = results.ParserErrors.Select(e => new CompilationMessage(e.Message));
                return CompilationResult.Failed(file, results.GeneratedCode, messages);
            }

            return _baseCompilationService.Compile(file, results.GeneratedCode);
        }
    }
}
