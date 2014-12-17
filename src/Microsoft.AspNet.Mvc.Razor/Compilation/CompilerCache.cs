// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.FileSystems;
using Microsoft.Framework.Cache.Memory;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <inheritdoc />
    public class CompilerCache : ICompilerCache
    {
        private static readonly MemoryCacheOptions _memoryCacheOptions = new MemoryCacheOptions
        {
            ListenForMemoryPressure = false,
            ExpirationScanFrequency = TimeSpan.MaxValue
        };

        private readonly IMemoryCache _cache;
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Initializes a new instance of <see cref="CompilerCache"/> populated with precompiled views
        /// discovered using <paramref name="provider"/>.
        /// </summary>
        /// <param name="provider">
        /// An <see cref="IAssemblyProvider"/> representing the assemblies
        /// used to search for pre-compiled views.
        /// </param>
        /// <param name="options">An accessor to the <see cref="RazorViewEngineOptions"/> configured for this
        /// application.</param>
        public CompilerCache(IAssemblyProvider provider,
                             IOptions<RazorViewEngineOptions> optionsAccessor)
            : this(GetFileInfos(provider.CandidateAssemblies),
                   optionsAccessor.Options.FileSystem)
        {
        }

        // Internal for unit testing
        internal CompilerCache(IEnumerable<RazorFileInfoCollection> viewCollections,
                               IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _cache = new MemoryCache(_memoryCacheOptions);

            foreach (var viewCollection in viewCollections)
            {
                foreach (var fileInfo in viewCollection.FileInfos)
                {
                    var containingAssembly = viewCollection.GetType().GetTypeInfo().Assembly;
                    var viewType = containingAssembly.GetType(fileInfo.FullTypeName);

                    // There shouldn't be any duplicates and if there are any the first will win.
                    // If the result doesn't match the one on disk its going to recompile anyways.
                    GetOrAdd(fileInfo.RelativePath, () => CompilationResult.Successful(viewType));
                }
            }
        }

        /// <inheritdoc />
        public CompilationResult GetOrAdd([NotNull] RelativeFileInfo relativeFileInfo,
                                          [NotNull] Func<CompilationResult> compile)
        {
            return GetOrAdd(relativeFileInfo.RelativePath, compile);
        }

        private CompilationResult GetOrAdd(string relativePath,
                                           Func<CompilationResult> compile)
        {
            var normalizedPath = NormalizePath(relativePath);
            var cachedResult = _cache.Get<Type>(normalizedPath);
            if (cachedResult != null)
            {
                return CompilationResult.Successful(cachedResult);
            }

            var compilationResult = compile();
            _cache.Set(normalizedPath,
                       context => OnCacheMiss(context, compilationResult.CompiledType, relativePath));

            return compilationResult;
        }

        private Type OnCacheMiss(ICacheSetContext context,
                                 Type compiledType,
                                 string relativePath)
        {
            context.AddExpirationTrigger(_fileSystem.Watch(relativePath));

            var viewStartLocations = ViewStartUtility.GetViewStartLocations(relativePath);
            foreach (var location in viewStartLocations)
            {
                context.AddExpirationTrigger(_fileSystem.Watch(location));
            }

            return compiledType;
        }

        internal static IEnumerable<RazorFileInfoCollection> GetFileInfos(
            IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(a => a.ExportedTypes)
                    .Where(Match)
                    .Select(c => (RazorFileInfoCollection)Activator.CreateInstance(c));
        }

        private static bool Match(Type t)
        {
            var inAssemblyType = typeof(RazorFileInfoCollection);
            if (inAssemblyType.IsAssignableFrom(t))
            {
                var hasParameterlessConstructor = t.GetConstructor(Type.EmptyTypes) != null;

                return hasParameterlessConstructor
                    && !t.GetTypeInfo().IsAbstract
                    && !t.GetTypeInfo().ContainsGenericParameters;
            }

            return false;
        }

        private static string NormalizePath(string path)
        {
            // We need to allow for scenarios where the application was precompiled on a machine with forward slashes
            // but is being run in one with backslashes (or vice versa). To this effect, we'll normalize paths to
            // use backslashes for lookups and storage in the dictionary.
            path = path.Replace('/', '\\');
            path = path.TrimStart('\\');

            return path;
        }
    }
}
