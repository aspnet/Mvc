// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Cache.Memory;
using Microsoft.Framework.FileSystemGlobbing;
using Microsoft.Framework.FileSystemGlobbing.Abstractions;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// Utility methods for <see cref="ITagHelper"/>'s that support attributes containing file globbing patterns.
    /// </summary>
    public class GlobbingUtility
    {
        /// <summary>
        /// Creates a new <see cref="GlobbingUtility"/>.
        /// </summary>
        /// <param name="cache">The <see cref="IMemoryCache"/> to cache globbing results in.</param>
        /// <param name="baseDirectory">The base directory to perform file globbing matches against.</param>
        /// <param name="fileProvider">
        ///     The <see cref="IFileProvider"/> used to watch for changes to file globbing results.
        /// </param>
        /// <param name="requestPathBase">
        ///     The base path of the current request (e.g. <see cref="HttpRequest.PathBase"/>).
        /// </param>
        public GlobbingUtility(
            IMemoryCache cache,
            DirectoryInfoBase baseDirectory,
            IFileProvider fileProvider,
            PathString requestPathBase)
        {
            Cache = cache;
            BaseDirectory = baseDirectory;
            FileProvider = fileProvider;
            RequestPathBase = requestPathBase;
        }

        /// <summary>
        /// The <see cref="IMemoryCache"/> to cache globbing results in.
        /// </summary>
        public virtual IMemoryCache Cache { get; private set; }

        /// <summary>
        /// The base directory to perform file globbing matches against.
        /// </summary>
        public virtual DirectoryInfoBase BaseDirectory { get; private set; }

        /// <summary>
        /// The <see cref="IFileProvider"/> used to watch for changes to file globbing results.
        /// </summary>
        public virtual IFileProvider FileProvider { get; private set; }

        /// <summary>
        /// The base path of the current request (e.g. <see cref="HttpRequest.PathBase"/>).
        /// </summary>
        public virtual PathString RequestPathBase { get; private set; }

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
            if (!string.IsNullOrWhiteSpace(staticUrl))
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
            var cacheKey = $"{nameof(GlobbingUtility)}-inc:{include}-exc:{exclude}";

            return Cache.GetOrSet(cacheKey, cacheSetContext =>
            {
                if (string.IsNullOrEmpty(include))
                {
                    return Enumerable.Empty<string>();
                }

                var includePatterns = include.Split(',');

                if (includePatterns.Length == 0)
                {
                    return Enumerable.Empty<string>();
                }

                foreach (var pattern in includePatterns)
                {
                    var trigger = FileProvider.Watch(pattern);
                    cacheSetContext.AddExpirationTrigger(trigger);
                }

                var excludePatterns = exclude?.Split(',');
                var matcher = new Matcher();
                matcher.AddPatterns(includePatterns, excludePatterns);
                var matches = matcher.Execute(BaseDirectory);

                return matches.Files.Select(ResolveMatchedPath);
            });
        }

        private string ResolveMatchedPath(string matchedPath)
        {
            // Resolve the path to site root
            var relativePath = new PathString("/" + matchedPath);
            return RequestPathBase.Add(relativePath).ToString();
        }
    }
}