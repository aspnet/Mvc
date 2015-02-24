﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Cache.Memory;
using Microsoft.Framework.FileSystemGlobbing;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    /// <summary>
    /// Utility methods for <see cref="ITagHelper"/>'s that support attributes containing file globbing patterns.
    /// </summary>
    public class GlobbingUrlBuilder
    {
        private static readonly char[] PatternSeparator = new[] { ',' };

        private readonly FileProviderGlobbingDirectory _baseGlobbingDirectory;

        // Internal for testing
        internal GlobbingUrlBuilder() { }

        /// <summary>
        /// Creates a new <see cref="GlobbingUrlBuilder"/>.
        /// </summary>
        /// <param name="fileProvider">The file provider.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="requestPathBase">The request path base.</param>
        public GlobbingUrlBuilder([NotNull] IFileProvider fileProvider, IMemoryCache cache, PathString requestPathBase)
        {
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
        public virtual IEnumerable<string> BuildUrlList(string staticUrl, string includePattern, string excludePattern)
        {
            var urls = new HashSet<string>(StringComparer.Ordinal);

            // Add the statically declared url if present
            if (staticUrl != null)
            {
                urls.Add(staticUrl);
            }

            // Add urls that match the globbing patterns specified
            var matchedUrls = ExpandGlobbedUrl(includePattern, excludePattern);
            urls.UnionWith(matchedUrls);

            return urls;
        }

        private IEnumerable<string> ExpandGlobbedUrl(string include, string exclude)
        {
            if (string.IsNullOrEmpty(include))
            {
                return Enumerable.Empty<string>();
            }

            var includePatterns = include.Split(PatternSeparator, StringSplitOptions.RemoveEmptyEntries);
            var excludePatterns = exclude?.Split(PatternSeparator, StringSplitOptions.RemoveEmptyEntries);

            if (includePatterns.Length == 0)
            {
                return Enumerable.Empty<string>();
            }

            if (Cache != null)
            {
                var cacheKey = $"{nameof(GlobbingUrlBuilder)}-inc:{include}-exc:{exclude}";
                return Cache.GetOrSet(cacheKey, cacheSetContext =>
                {
                    foreach (var pattern in includePatterns)
                    {
                        var trigger = FileProvider.Watch(pattern);
                        cacheSetContext.AddExpirationTrigger(trigger);
                    }

                    return FindFiles(includePatterns, excludePatterns);
                });
            }

            return FindFiles(includePatterns, excludePatterns);
        }

        private IEnumerable<string> FindFiles(IEnumerable<string> includePatterns, IEnumerable<string> excludePatterns)
        {
            var matcher = MatcherBuilder != null ? MatcherBuilder() : new Matcher();

            matcher.AddIncludePatterns(includePatterns.Select(pattern => TrimLeadingSlash(pattern)));

            if (excludePatterns != null)
            {
                matcher.AddExcludePatterns(excludePatterns.Select(pattern => TrimLeadingSlash(pattern)));
            }

            var matches = matcher.Execute(_baseGlobbingDirectory);

            return matches.Files.Select(ResolveMatchedPath)
                .OrderBy(path => path, new PathComparer());
        }

        private string ResolveMatchedPath(string matchedPath)
        {
            // Resolve the path to site root
            var relativePath = new PathString("/" + matchedPath);
            return RequestPathBase.Add(relativePath).ToString();
        }

        private class PathComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                // < 0 = x < y
                // > 0 = x > y

                if (x.Equals(y))
                {
                    return 0;
                }

                var xExtIndex = x.LastIndexOf('.');
                var yExtIndex = y.LastIndexOf('.');

                var xNoExt = x.Substring(0, xExtIndex);
                var yNoExt = y.Substring(0, yExtIndex);

                if (xNoExt.Equals(yNoExt))
                {
                    // Only extension differs so just compare the extension
                    var xExt = x.Substring(xExtIndex);
                    var yExt = y.Substring(yExtIndex);
                    return xExt.CompareTo(yExt);
                }

                var xSegments = xNoExt.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                var ySegments = yNoExt.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (xSegments.Length != ySegments.Length)
                {
                    // Different path depths so shallower path wins
                    return xSegments.Length.CompareTo(ySegments.Length);
                }

                // Depth is the same so compare each segment
                for (int i = 0; i < xSegments.Length; i++)
                {
                    var xSegment = xSegments[i];
                    var ySegment = ySegments[i];

                    var xToY = xSegment.CompareTo(ySegment);
                    if (xToY != 0)
                    {
                        return xToY;
                    }
                }

                // Should't get here, but if we do, hey, they're the same :)
                return 0;
            }
        }

        private static string TrimLeadingSlash(string value)
        {
            var result = value;

            if (result.StartsWith("/", StringComparison.Ordinal) ||
                result.StartsWith("\\", StringComparison.Ordinal))
            {
                // Trim the leading slash as the matcher runs from the provided root only anyway
                result = result.Substring(1);
            }

            return result;
        }
    }
}