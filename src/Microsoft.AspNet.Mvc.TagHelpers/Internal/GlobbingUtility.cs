// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Cache.Memory;
using Microsoft.Framework.FileSystemGlobbing;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    /// <summary>
    /// Utility methods for <see cref="ITagHelper"/>'s that support attributes containing file globbing patterns.
    /// </summary>
    public class GlobbingUtility
    {
        private FileProviderGlobbingDirectory _baseGlobbingDirectory;

        public GlobbingUtility(IMemoryCache cache, IFileProvider fileProvider, PathString requestPathBase)
        {
            Cache = cache;
            FileProvider = fileProvider;
            RequestPathBase = requestPathBase;
            _baseGlobbingDirectory = new FileProviderGlobbingDirectory(fileProvider, fileInfo: null, parent: null);
        }

        /// <summary>
        /// The <see cref="IMemoryCache"/> to cache globbing results in.
        /// </summary>
        public IMemoryCache Cache { get; set; }

        /// <summary>
        /// The <see cref="IFileProvider"/> used to watch for changes to file globbing results.
        /// </summary>
        public IFileProvider FileProvider { get; set; }

        /// <summary>
        /// The base path of the current request (e.g. <see cref="HttpRequest.PathBase"/>).
        /// </summary>
        public PathString RequestPathBase { get; set; }

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
            foreach (var url in matchedUrls)
            {
                urls.Add(url);
            }

            return urls;
        }

        private IEnumerable<string> ExpandGlobbedUrl(string include, string exclude = null)
        {
            if (string.IsNullOrEmpty(include))
            {
                return Enumerable.Empty<string>();
            }

            var includePatterns = include.Split(',');
            var excludePatterns = exclude?.Split(',');

            if (includePatterns.Length == 0)
            {
                return Enumerable.Empty<string>();
            }
            
            if (Cache != null)
            {
                var cacheKey = $"{nameof(GlobbingUtility)}-inc:{include}-exc:{exclude}";
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
            var matcher = new Matcher();
            matcher.AddPatterns(includePatterns, excludePatterns);
            var matches = matcher.Execute(_baseGlobbingDirectory);

            return matches.Files.Select(ResolveMatchedPath);
        }

        private string ResolveMatchedPath(string matchedPath)
        {
            // Resolve the path to site root
            var relativePath = new PathString("/" + matchedPath);
            return RequestPathBase.Add(relativePath).ToString();
        }
    }
}