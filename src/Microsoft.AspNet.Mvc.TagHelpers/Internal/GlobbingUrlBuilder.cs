// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    /// <summary>
    /// Utility methods for <see cref="AspNet.Razor.TagHelpers.ITagHelper"/>'s that support
    /// attributes containing file globbing patterns.
    /// </summary>
    public class GlobbingUrlBuilder
    {
        private static readonly IList<string> EmptyList =
#if NET451
            new string[0];
#else
            Array.Empty<string>();
#endif

        private static readonly char[] PatternSeparator = new[] { ',' };

        // Valid whitespace characters defined by the HTML5 spec.
        private static readonly char[] ValidAttributeWhitespaceChars =
            new[] { '\t', '\n', '\u000C', '\r', ' ' };

        private static readonly PathComparer DefaultPathComparer = new PathComparer();

        private readonly FileProviderGlobbingDirectory _baseGlobbingDirectory;

        public GlobbingUrlBuilder() { }

        /// <summary>
        /// Creates a new <see cref="GlobbingUrlBuilder"/>.
        /// </summary>
        /// <param name="fileProvider">The file provider.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="requestPathBase">The request path base.</param>
        public GlobbingUrlBuilder(IFileProvider fileProvider, IMemoryCache cache, PathString requestPathBase)
        {
            if (fileProvider == null)
            {
                throw new ArgumentNullException(nameof(fileProvider));
            }

            FileProvider = fileProvider;
            Cache = cache;
            RequestPathBase = requestPathBase;
            _baseGlobbingDirectory = new FileProviderGlobbingDirectory(fileProvider, fileInfo: null, parent: null);
        }

        /// <summary>
        /// The <see cref="IMemoryCache"/> to cache globbing results in.
        /// </summary>
        public IMemoryCache Cache { get; }

        /// <summary>
        /// The <see cref="IFileProvider"/> used to watch for changes to file globbing results.
        /// </summary>
        public IFileProvider FileProvider { get; }

        /// <summary>
        /// The base path of the current request (i.e. <see cref="HttpRequest.PathBase"/>).
        /// </summary>
        public PathString RequestPathBase { get; }

        // Internal for testing.
        internal Func<Matcher> MatcherBuilder { get; set; }

        /// <summary>
        /// Builds a list of URLs.
        /// </summary>
        /// <param name="staticUrl">The statically declared URL. This will always be added to the result.</param>
        /// <param name="includePattern">The file globbing include pattern.</param>
        /// <param name="excludePattern">The file globbing exclude pattern.</param>
        /// <returns>The list of URLs</returns>
        public virtual ICollection<string> BuildUrlList(string staticUrl, string includePattern, string excludePattern)
        {

            var urls = new List<string>();
            if (staticUrl != null)
            {
                urls.Add(staticUrl);
            }

            // Add urls that match the globbing patterns specified
            var globbedUrls = ExpandGlobbedUrl(includePattern, excludePattern);

            // ExpandGlobbedUrl returns a sorted list. We can take advantage of this to perform an order preserving distinct.
            string lastAddedEntry = null;
            for (var i = 0; i < globbedUrls.Count; i++)
            {
                if (!string.Equals(staticUrl, globbedUrls[i], StringComparison.Ordinal) &&
                    !string.Equals(lastAddedEntry, globbedUrls[i], StringComparison.Ordinal))
                {
                    lastAddedEntry = globbedUrls[i];
                    urls.Add(lastAddedEntry);
                }
            }

            return urls;
        }

        private IList<string> ExpandGlobbedUrl(string include, string exclude)
        {
            if (string.IsNullOrEmpty(include))
            {
                return EmptyList;
            }
            else if (Cache == null)
            {
                var includePatterns = include.Split(PatternSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (includePatterns.Length == 0)
                {
                    return EmptyList;
                }

                return FindFiles(includePatterns, exclude);
            }
            else
            {
                var cacheKey = new GlobbingUrlKey(include, exclude);
                List<string> files;
                if (!Cache.TryGetValue(cacheKey, out files))
                {
                    var includePatterns = include.Split(PatternSeparator, StringSplitOptions.RemoveEmptyEntries);
                    if (includePatterns.Length == 0)
                    {
                        return new List<string>();
                    }

                    var options = new MemoryCacheEntryOptions();
                    for (var i = 0; i < includePatterns.Length; i++)
                    {
                        var changeToken = FileProvider.Watch(includePatterns[i]);
                        options.AddExpirationToken(changeToken);
                    }

                    files = FindFiles(includePatterns, exclude);

                    Cache.Set(cacheKey, files, options);
                }

                return files;
            }
        }

        private List<string> FindFiles(string[] includePatterns, string exclude)
        {
            var matcher = MatcherBuilder != null ? MatcherBuilder() : new Matcher();
            var trimmedIncludePatterns = new List<string>();
            for (var i = 0; i < includePatterns.Length; i++)
            {
                trimmedIncludePatterns.Add(TrimLeadingTildeSlash(includePatterns[i]));
            }
            matcher.AddIncludePatterns(trimmedIncludePatterns);

            if (!string.IsNullOrWhiteSpace(exclude))
            {
                var excludePatterns = exclude.Split(PatternSeparator, StringSplitOptions.RemoveEmptyEntries);
                var trimmedExcludePatterns = new List<string>(excludePatterns.Length);
                for (var i = 0; i < excludePatterns.Length; i++)
                {
                    trimmedExcludePatterns.Add(TrimLeadingTildeSlash(excludePatterns[i]));
                }
                matcher.AddExcludePatterns(trimmedExcludePatterns);
            }

            var matches = matcher.Execute(_baseGlobbingDirectory);
            var matchedUrls = new List<string>();
            foreach (var matchedPath in matches.Files)
            {
                // Resolve the path to site root
                var relativePath = new PathString("/" + matchedPath.Path);
                matchedUrls.Add(RequestPathBase.Add(relativePath).ToString());
            }

            matchedUrls.Sort(DefaultPathComparer);
            return matchedUrls;
        }

        private class PathComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                // < 0 = x < y
                // > 0 = x > y

                if (string.Equals(x, y, StringComparison.Ordinal))
                {
                    return 0;
                }

                if (string.IsNullOrEmpty(x) || string.IsNullOrEmpty(y))
                {
                    return string.Compare(x, y, StringComparison.Ordinal);
                }

                var xExtIndex = x.LastIndexOf('.');
                var yExtIndex = y.LastIndexOf('.');

                // Ensure extension index is in the last segment, i.e. in the file name
                var xSlashIndex = x.LastIndexOf('/');
                var ySlashIndex = y.LastIndexOf('/');
                xExtIndex = xExtIndex > xSlashIndex ? xExtIndex : -1;
                yExtIndex = yExtIndex > ySlashIndex ? yExtIndex : -1;

                // Get paths without their extensions, if they have one
                var xLength = xExtIndex >= 0 ? xExtIndex : x.Length;
                var yLength = yExtIndex >= 0 ? yExtIndex : y.Length;
                var compareLength = Math.Max(xLength, yLength);

                if (string.Compare(x, 0, y, 0, compareLength, StringComparison.Ordinal) == 0)
                {
                    // Only extension differs so just compare the extension
                    if (xExtIndex >= 0 && yExtIndex >= 0)
                    {
                        var length = x.Length - xExtIndex;
                        return string.Compare(x, xExtIndex, y, yExtIndex, length, StringComparison.Ordinal);
                    }

                    return xExtIndex - yExtIndex;
                }

                var xNoExt = xExtIndex >= 0 ? x.Substring(0, xExtIndex) : x;
                var yNoExt = yExtIndex >= 0 ? y.Substring(0, yExtIndex) : y;

                var result = 0;
                var xTokenizer = new StringTokenizer(xNoExt, '/').GetEnumerator();
                var yTokenizer = new StringTokenizer(yNoExt, '/').GetEnumerator();
                StringSegment xSegment;
                StringSegment ySegment;
                while (TryGetNextSegment(ref xTokenizer, out xSegment))
                {
                    if (!TryGetNextSegment(ref yTokenizer, out ySegment))
                    {
                        // Different path depths (right is shorter), so shallower path wins.
                        return 1;
                    }

                    if (result != 0)
                    {
                        // Once we've determined that a segment differs, we need to ensure that the two paths
                        // are of equal depth.
                        continue;
                    }

                    var length = Math.Max(xSegment.Length, ySegment.Length);
                    result = string.Compare(
                        xSegment.Buffer,
                        xSegment.Offset,
                        ySegment.Buffer,
                        ySegment.Offset,
                        length,
                        StringComparison.Ordinal);
                }

                if (TryGetNextSegment(ref yTokenizer, out ySegment))
                {
                    // Different path depths (left is shorter). Shallower path wins.
                    return -1;
                }
                else
                {
                    // Segments are of equal length
                    return result;
                }
            }

            private static bool TryGetNextSegment(ref StringTokenizer.Enumerator enumerator, out StringSegment segment)
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.HasValue && enumerator.Current.Length > 0)
                    {
                        segment = enumerator.Current;
                        return true;
                    }
                }

                segment = default(StringSegment);
                return false;
            }
        }

        private static string TrimLeadingTildeSlash(string value)
        {
            var result = value.Trim(ValidAttributeWhitespaceChars);

            if (result.StartsWith("~/", StringComparison.Ordinal))
            {
                result = result.Substring(2);
            }
            else if (result.StartsWith("/", StringComparison.Ordinal) ||
                result.StartsWith("\\", StringComparison.Ordinal))
            {
                // Trim the leading slash as the matcher runs from the provided root only anyway
                result = result.Substring(1);
            }

            return result;
        }

        private struct GlobbingUrlKey : IEquatable<GlobbingUrlKey>
        {
            public GlobbingUrlKey(string include, string exclude)
            {
                Include = include;
                Exclude = exclude;
            }

            public string Include { get; }

            public string Exclude { get; }

            public bool Equals(GlobbingUrlKey other)
            {
                return string.Equals(Include, other.Include, StringComparison.Ordinal) &&
                    string.Equals(Exclude, other.Exclude, StringComparison.Ordinal);

            }
        }
    }
}