// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class CompilerCache
    {
        public static bool DebugBreak { get; set; }
        private readonly ConcurrentDictionary<string, CompilerCacheEntry> _cache;
        private static readonly Type[] EmptyType = new Type[0];

        internal CompilerCache()
        {
            _cache = new ConcurrentDictionary<string, CompilerCacheEntry>(StringComparer.OrdinalIgnoreCase);
        }

        public CompilerCache([NotNull] IControllerAssemblyProvider _controllerAssemblyProvider)
            : this()
        {
            var assemblies = _controllerAssemblyProvider.CandidateAssemblies;
            var types = assemblies.SelectMany(a => a.ExportedTypes);
            var preCompiledCollections =
                types
                .Where(Match);

            foreach (var collectionType in preCompiledCollections)
            {
                var preCompiledCollection = Activator.CreateInstance(collectionType)
                                                    as ViewDescriptorCollection;

                foreach (var fileInfo in preCompiledCollection.FileInfos)
                {
                    var containingAssembly = collectionType.GetTypeInfo().Assembly;
                    var viewType = containingAssembly.GetType(fileInfo.FullTypeName);
                    var cacheEntry = new CompilerCacheEntry(fileInfo, viewType);

                    _cache.AddOrUpdate(fileInfo.RelativePath, cacheEntry, (a, b) => cacheEntry);
                }
            }
        }

        private static bool Match(Type t)
        {
            var b = t.GetConstructor(EmptyType) != null;

            var inAssemblyType = typeof(ViewDescriptorCollection);
            var c = inAssemblyType.IsAssignableFrom(t);

            return b && c
                && !t.GetTypeInfo().IsAbstract
                && !t.GetTypeInfo().ContainsGenericParameters;
        }

        public CompilationResult GetOrAdd(RelativeFileInfo fileInfo, Func<CompilationResult> compile)
        {
            CompilerCacheEntry cacheEntry;

            if (!_cache.TryGetValue(fileInfo.RelativePath, out cacheEntry))
            {
                return OnCacheMiss(fileInfo, compile);
            }
            else
            {
                if (cacheEntry.Length != fileInfo.FileInfo.Length)
                {
                    // it's not a match, recompile
                    return OnCacheMiss(fileInfo, compile);
                }

                if (cacheEntry.RuntimeTimeStamp == fileInfo.FileInfo.LastModified)
                {
                    // Match, not update needed
                    return CompilationResult.Successful(cacheEntry.ViewType);
                }

                if (cacheEntry.CompiledTimeStamp == fileInfo.FileInfo.LastModified ||
                    // Date doesn't match but it might be because of deployment, compare the hash
                    cacheEntry.Hash == RazorFileHash.GetHash(fileInfo.FileInfo))
                {
                    // Cache hit, but we need to update the entry
                    return OnCacheMiss(fileInfo, () => CompilationResult.Successful(cacheEntry.ViewType));
                }

                // it's not a match, recompile
                return OnCacheMiss(fileInfo, compile);
            }
        }

        private CompilationResult OnCacheMiss(RelativeFileInfo file, Func<CompilationResult> compile)
        {
            var result = compile();

            var cacheEntry = new CompilerCacheEntry(file, result.CompiledType, null);
            _cache.AddOrUpdate(file.RelativePath, cacheEntry, (a, b) => cacheEntry);

            return result;
        }
    }
}
