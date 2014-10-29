// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.FileSystems;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <inheritdoc />
    public class CompilerCache : ICompilerCache
    {
        private readonly ConcurrentDictionary<string, CompilerCacheEntry> _cache;
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Initializes a new instance of <see cref="CompilerCache"/> that is populated with precompiled views
        /// discovered using <paramref name="provider"/>.
        /// </summary>
        /// <param name="provider">
        /// An <see cref="IAssemblyProvider"/> representing the assemblies
        /// used to search for pre-compiled views.
        /// </param>
        /// <param name="fileSystem">An <see cref="ICachedFileSystem"/> that represents application's file system.
        /// </param>
        public CompilerCache(IAssemblyProvider provider, ICachedFileSystem fileSystem)
            : this (GetFileInfos(provider.CandidateAssemblies), fileSystem)
        {
        }

        // Internal for unit testing
        internal CompilerCache(IEnumerable<RazorFileInfoCollection> viewCollections, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _cache = new ConcurrentDictionary<string, CompilerCacheEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (var viewCollection in viewCollections)
            {
                foreach (var fileInfo in viewCollection.FileInfos)
                {
                    var containingAssembly = viewCollection.GetType().GetTypeInfo().Assembly;
                    var viewType = containingAssembly.GetType(fileInfo.FullTypeName);
                    var cacheEntry = new CompilerCacheEntry(fileInfo, viewType);

                    // There shouldn't be any duplicates and if there are any the first will win.
                    // If the result doesn't match the one on disk its going to recompile anyways.
                    _cache.TryAdd(NormalizePath(fileInfo.RelativePath), cacheEntry);
                }
            }

            // Set up ViewStarts
            foreach (var entry in _cache)
            {
                var viewStartLocations = ViewStartUtility.GetViewStartLocations(fileSystem, entry.Key);
                foreach (var location in viewStartLocations)
                {
                    CompilerCacheEntry viewStartEntry;
                    if (_cache.TryGetValue(location, out viewStartEntry))
                    {
                        // Add the the composite _ViewStart entry as a dependency.
                        entry.Value.AssociatedViewStartEntry = viewStartEntry;
                        break;
                    }
                }
            }
        }

        internal static IEnumerable<RazorFileInfoCollection>
                            GetFileInfos(IEnumerable<Assembly> assemblies)
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

        /// <inheritdoc />
        public CompilationResult GetOrAdd([NotNull] RelativeFileInfo fileInfo,
                                          [NotNull] Func<RelativeFileInfo, CompilationResult> compile)
        {
            CompilationResult result;
            var entry = GetOrAddEntry(fileInfo, compile, out result);
            return result;
        }

        private CompilerCacheEntry GetOrAddEntry(RelativeFileInfo relativeFileInfo,
                                                 Func<RelativeFileInfo, CompilationResult> compile,
                                                 out CompilationResult result)
        {
            CompilerCacheEntry cacheEntry;
            var normalizedPath = NormalizePath(relativeFileInfo.RelativePath);
            if (!_cache.TryGetValue(normalizedPath, out cacheEntry))
            {
                return OnCacheMiss(relativeFileInfo, normalizedPath, compile, out result);
            }
            else
            {
                var fileInfo = relativeFileInfo.FileInfo;
                if (cacheEntry.Length != fileInfo.Length)
                {
                    // Recompile if the file lengths differ
                    return OnCacheMiss(relativeFileInfo, normalizedPath, compile, out result);
                }

                if (AssociatedViewStartsChanged(cacheEntry, compile))
                {
                    // Recompile if the view starts have changed since the entry was created.
                    return OnCacheMiss(relativeFileInfo, normalizedPath, compile, out result);
                }

                if (cacheEntry.LastModified == fileInfo.LastModified)
                {
                    result = CompilationResult.Successful(cacheEntry.CompiledType);
                    return cacheEntry;
                }

                // Timestamp doesn't match but it might be because of deployment, compare the hash.
                if (cacheEntry.IsPreCompiled &&
                    string.Equals(cacheEntry.Hash, RazorFileHash.GetHash(fileInfo), StringComparison.Ordinal))
                {
                    // Cache hit, but we need to update the entry.
                    cacheEntry.LastModified = fileInfo.LastModified;
                    result = CompilationResult.Successful(cacheEntry.CompiledType);

                    return cacheEntry;
                }

                // it's not a match, recompile
                return OnCacheMiss(relativeFileInfo, normalizedPath, compile, out result);
            }
        }

        private CompilerCacheEntry OnCacheMiss(RelativeFileInfo file,
                                               string normalizedPath,
                                               Func<RelativeFileInfo, CompilationResult> compile,
                                               out CompilationResult result)
        {
            result = compile(file);

            var cacheEntry = new CompilerCacheEntry(file, result.CompiledType)
            {
                AssociatedViewStartEntry = GetCompositeViewStartEntry(normalizedPath, compile)
            };

            _cache[normalizedPath] = cacheEntry;
            return cacheEntry;
        }

        private bool AssociatedViewStartsChanged(CompilerCacheEntry entry,
                                                 Func<RelativeFileInfo, CompilationResult> compile)
        {
            var viewStartEntry = GetCompositeViewStartEntry(entry.RelativePath, compile);
            return entry.AssociatedViewStartEntry != viewStartEntry;
        }

        /// <remarks>
        /// Returns the entry for the nearest _ViewStart that the file inherits directives from. Since _ViewStart
        /// entries are affected by other _ViewStart entries that are in the path hierarchy, the returned value
        /// represents the compositie result of performing a cache check on individual _ViewStart entries.
        /// </remarks>
        private CompilerCacheEntry GetCompositeViewStartEntry(string relativePath,
                                                              Func<RelativeFileInfo, CompilationResult> compile)
        {
            var viewStartLocations = ViewStartUtility.GetViewStartLocations(_fileSystem, relativePath);
            foreach (var viewStartLocation in viewStartLocations)
            {
                IFileInfo viewStartFileInfo;
                if (_fileSystem.TryGetFileInfo(viewStartLocation, out viewStartFileInfo))
                {
                    var relativeFileInfo = new RelativeFileInfo(viewStartFileInfo, viewStartLocation);
                    CompilationResult result;
                    return GetOrAddEntry(relativeFileInfo, compile, out result);
                }
            }

            // No _ViewStarts discovered.
            return null;
        }

        private static string NormalizePath(string path)
        {
            path = path.Replace('/', '\\');
            path = path.TrimStart('\\');

            return path;
        }
    }
}
