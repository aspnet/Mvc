// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNet.FileSystems;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class RazorPreCompiler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileSystem _fileSystem;
        private readonly IMvcRazorHost _host;
        private readonly string _basePath;

        protected virtual string FileExtension
        {
            get
            {
                return ".cshtml";
            }
        }

        public RazorPreCompiler([NotNull] IServiceProvider designTimeServiceProvider) :
            this(designTimeServiceProvider, designTimeServiceProvider.GetService<IMvcRazorHost>())
        {
        }

        public RazorPreCompiler([NotNull] IServiceProvider designTimeServiceProvider,
                                [NotNull] IMvcRazorHost host)
        {
            _serviceProvider = designTimeServiceProvider;
            _host = host;
            var appEnv = _serviceProvider.GetService<IApplicationEnvironment>();
            _basePath = RelativePath.EnsureTrailingSlash(appEnv.ApplicationBasePath);
            _fileSystem = new PhysicalFileSystem(appEnv.ApplicationBasePath);
        }

        public virtual void CompileViews([NotNull] IBeforeCompileContext context)
        {
            var descriptors = CreateCompilationDescriptors(context);
            var collectionGenerator = new ViewDescriptorCollectionGenerator(
                                            descriptors,
                                            ParseOptions.GetParseOptions(context.CSharpCompilation));

            var tree = collectionGenerator.GenerateCollection();
            context.CSharpCompilation = context.CSharpCompilation.AddSyntaxTrees(tree);
        }

        protected virtual IReadOnlyList<RazorFileInfo>
            CreateCompilationDescriptors([NotNull] IBeforeCompileContext context)
        {
            var state = new CompilationContext(context);

            CompileViews(state, string.Empty);

            return state.CompiledFiles;
        }

        private void CompileViews(CompilationContext compilationContext, string currentPath)
        {
            IEnumerable<IFileInfo> fileInfos;
            string path = currentPath;

            if (!_fileSystem.TryGetDirectoryContents(path, out fileInfos))
            {
                return;
            }

            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.IsDirectory)
                {
                    var subPath = Path.Combine(path, fileInfo.Name);

                    CompileViews(compilationContext, subPath);
                }
                else if (Path.GetExtension(fileInfo.Name).Equals(FileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    var descriptor = ParseView(fileInfo, compilationContext.Context, compilationContext.Options);

                    if (descriptor != null)
                    {
                        compilationContext.CompiledFiles.Add(descriptor);
                    }
                }
            }
        }

        protected virtual RazorFileInfo ParseView([NotNull] IFileInfo fileInfo,
                                                  [NotNull] IBeforeCompileContext context,
                                                  [NotNull] CSharpParseOptions options)
        {
            using (var stream = fileInfo.CreateReadStream())
            {
                var relativePath = RelativePath.GetRelativePath(_basePath, fileInfo);
                var results = _host.GenerateCode(relativePath, stream);
                if (results.Success)
                {
                    var syntaxTree = SyntaxTreeGenerator.Generate(results.GeneratedCode, fileInfo, options);

                    // when we start generating more than one class per view, 
                    // we could figure out a better way to deal with this.
                    var classes = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
                    var mainClass = classes.FirstOrDefault(c =>
                        c.Identifier.ValueText.StartsWith(_host.MainClassNamePrefix, StringComparison.Ordinal));

                    if (mainClass != null)
                    {
                        context.CSharpCompilation = context.CSharpCompilation.AddSyntaxTrees(syntaxTree);

                        var typeName = mainClass.Identifier.ValueText;

                        if (!string.IsNullOrEmpty(_host.DefaultNamespace))
                        {
                            typeName = _host.DefaultNamespace + "." + typeName;
                        }

                        var hash = RazorFileHash.GetHash(fileInfo);

                        return new RazorFileInfo()
                        {
                            FullTypeName = typeName,
                            RelativePath = relativePath,
                            LastModified = fileInfo.LastModified,
                            Length = fileInfo.Length,
                            Hash = hash,
                        };
                    }
                }
            }

            // TODO: Add diagnostics when view parsing/code generation failed.
            return null;
        }

        private class CompilationContext
        {
            public CompilationContext(IBeforeCompileContext context)
            {
                Context = context;
                Options = ParseOptions.GetParseOptions(context.CSharpCompilation);
            }

            public List<RazorFileInfo> CompiledFiles { get; } = new List<RazorFileInfo>();
            public IBeforeCompileContext Context { get; private set; }
            public CSharpParseOptions Options { get; private set; }
        }
    }
}

namespace Microsoft.Framework.Runtime
{
    [AssemblyNeutral]
    public interface IBeforeCompileContext
    {
        CSharpCompilation CSharpCompilation { get; set; }

        IList<ResourceDescription> Resources { get; }

        IList<Diagnostic> Diagnostics { get; }
    }
}
