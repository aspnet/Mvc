// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.AspNet.FileSystems;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <inheritdoc />
    public class CompilerCache : ICompilerCache
    {
        private readonly ConcurrentDictionary<string, CompilerCacheEntry> _cache;

        public CompilerCache()
        {
            _cache = new ConcurrentDictionary<string, CompilerCacheEntry>(StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public void Add([NotNull] RazorFileInfo info, [NotNull] Type type)
        {
            var key = NormalizePath(info.RelativePath);
            var entry = new CompilerCacheEntry(info, type);
            _cache.TryAdd(key, entry);
        }

        /// <inheritdoc />
        public CompilationResult GetOrAdd([NotNull] RelativeFileInfo fileInfo,
                                          [NotNull] IFileSystem fileSystem,
                                          [NotNull] Func<CompilationResult> compile)
        {
            CompilerCacheEntry cacheEntry;
            var normalizedPath = NormalizePath(fileInfo.RelativePath);
            if (!_cache.TryGetValue(normalizedPath, out cacheEntry))
            {
                return OnCacheMiss(fileInfo, compile);
            }
            else
            {
                var cacheStatus = GetCacheValidity(cacheEntry, fileSystem, fileInfo);

                if (cacheStatus == CacheEntryStatus.Valid)
                {
                    return CompilationResult.Successful(cacheEntry.CompiledType);
                }
                else if (cacheStatus == CacheEntryStatus.ValidWithIncorrectTimestamp)
                {
                    // Cache hit, but we need to update the entry
                    return OnCacheMiss(fileInfo, 
                        () => CompilationResult.Successful(cacheEntry.CompiledType));
                }

                // it's not a match, recompile
                return OnCacheMiss(fileInfo, compile);
            }
        }

        public object GetOrAddMetadata([NotNull] RelativeFileInfo fileInfo,
                                       [NotNull] IFileSystem fileSystem,
                                       [NotNull] object key,
                                       Func<object> valueFactory)
        {
            var normalizedPath = NormalizePath(fileInfo.RelativePath);
            CompilerCacheEntry cacheEntry;

            if (_cache.TryGetValue(normalizedPath, out cacheEntry) && 
                GetCacheValidity(cacheEntry, fileSystem, fileInfo) != CacheEntryStatus.Invalid)
            {
                lock (cacheEntry.Metadata)
                {
                    object value;
                    if (!cacheEntry.Metadata.TryGetValue(key, out value))
                    {
                        value = valueFactory();
                        cacheEntry.Metadata[key] = value;
                    }
                }
            }

            // Indicates that a cache entry does not exist or the entry is invalid.
            // At this point, simply invoke the valueFactory and do not bother saving the result.
            return valueFactory();
        }

        private CompilationResult OnCacheMiss(RelativeFileInfo file,
                                              Func<CompilationResult> compile)
        {
            var result = compile();

            var cacheEntry = new CompilerCacheEntry(file, result.CompiledType);
            _cache[NormalizePath(file.RelativePath)] = cacheEntry;

            return result;
        }

        private bool AreAssociatedViewStartsUnchanged(IFileSystem fileSystem, string relativePath)
        {
            var viewStartLocations = ViewStartUtility.GetViewStartLocations(fileSystem, relativePath);

            foreach (var viewStartLocation in viewStartLocations)
            {
                IFileInfo fileInfo;
                CompilerCacheEntry cacheEntry;

                var existsOnDisk = fileSystem.TryGetFileInfo(viewStartLocation, out fileInfo);
                var existsInCache = _cache.TryGetValue(viewStartLocation, out cacheEntry);

                if (existsOnDisk ^ existsInCache)
                {
                    // If a ViewStart entry exists in the cache but doesn't exist on disk it must have been deleted.
                    // If a ViewStart entry does not exist in the cache but exists on disk, we have a new ViewStart entry.
                    return false;
                }
                else if (existsOnDisk)
                {
                    var relativeFileInfo = new RelativeFileInfo(fileInfo, viewStartLocation);
                    var cacheStatus = GetCacheValidity(cacheEntry, fileSystem, relativeFileInfo);
                    if (cacheStatus == CacheEntryStatus.Invalid)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private CacheEntryStatus GetCacheValidity(CompilerCacheEntry cacheEntry,
                                                  IFileSystem fileSystem,
                                                  RelativeFileInfo relativeFileInfo)
        {
            var fileInfo = relativeFileInfo.FileInfo;
            if (cacheEntry.Length != fileInfo.Length)
            {
                // Recompile if the file lengths differ
                return CacheEntryStatus.Invalid;
            }

            if (!AreAssociatedViewStartsUnchanged(fileSystem, relativeFileInfo.RelativePath))
            {
                // Recompile if the view starts have changed since they were last cached.
                return CacheEntryStatus.Invalid;
            }

            if (cacheEntry.LastModified == fileInfo.LastModified)
            {
                return CacheEntryStatus.Valid;
            }

            var hash = RazorFileHash.GetHash(fileInfo);

            // Timestamp doesn't match but it might be because of deployment, compare the hash.
            if (cacheEntry.IsPreCompiled &&
                string.Equals(cacheEntry.Hash, hash, StringComparison.Ordinal))
            {
                return CacheEntryStatus.ValidWithIncorrectTimestamp;
            }

            return CacheEntryStatus.Invalid;
        }

        private static string NormalizePath(string path)
        {
            path = path.Replace('/', '\\');
            path = path.TrimStart('\\');

            return path;
        }

        private enum CacheEntryStatus
        {
            Invalid,
            ValidWithIncorrectTimestamp,
            Valid
        }
    }
}
