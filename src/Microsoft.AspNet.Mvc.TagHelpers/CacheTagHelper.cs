// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Cache.Memory;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    /// <summary>
    /// <see cref="TagHelper"/> implementation targeting &lt;cache&gt; elements.
    /// </summary>
    public class CacheTagHelper : TagHelper
    {
        private const string VaryByAttributeName = "asp-vary-by";
        private const string VaryByHeaderAttributeName = "asp-vary-by-header";
        private const string VaryByQueryAttributeName = "asp-vary-by-query";
        private const string VaryByRouteAttributeName = "asp-vary-by-route";
        private const string VaryByCookieAttributeName = "asp-vary-by-route";
        private const string VaryByUserAttributeName = "asp-vary-by-user";
        private const string ExpiresOnAttributeName = "asp-expires-on";
        private const string ExpiresAfterAttributeName = "asp-expires-after";
        private const string ExpiresSlidingAttributeName = "asp-expires-sliding";
        private const string CachePriorityAttributeName = "asp-priority";
        private const string CacheKeyTokenSeparator = "||";
        private static readonly char[] AttributeSeparator = new[] { ',' };

        [Activate]
        public IMemoryCache MemoryCache { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ViewContext"/> for the current executing View.
        /// </summary>
        [Activate]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="string" /> to vary the cached result by.
        /// </summary>
        [HtmlAttributeName(VaryByAttributeName)]
        public string VaryBy { get; set; }

        /// <summary>
        /// Gets or sets the name of a HTTP request header to vary the cached result by.
        /// </summary>
        [HtmlAttributeName(VaryByHeaderAttributeName)]
        public string VaryByHeader { get; set; }

        /// <summary>
        /// Gets or sets a comma-delimited set of query parameters to vary the cached result by.
        /// </summary>
        [HtmlAttributeName(VaryByQueryAttributeName)]
        public string VaryByQuery { get; set; }

        /// <summary>
        /// Gets or sets a comma-delimited set of route data parameters to vary the cached result by.
        /// </summary>
        [HtmlAttributeName(VaryByRouteAttributeName)]
        public string VaryByRoute { get; set; }

        /// <summary>
        /// Gets or sets a comma-delimited set of cookie names to vary the cached result by.
        /// </summary>
        [HtmlAttributeName(VaryByRouteAttributeName)]
        public string VaryByCookie { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if the content is to be varied by the Identity for the logged in 
        /// <see cref="HttpContext.User"/>.
        /// </summary>
        [HtmlAttributeName(VaryByUserAttributeName)]
        public bool VaryByUser { get; set; }

        /// <summary>
        /// Gets or sets the exact <see cref="DateTimeOffset"/> the cache entry should be evicted.
        /// </summary>
        [HtmlAttributeName(ExpiresOnAttributeName)]
        public DateTimeOffset? ExpiresOn { get; set; }

        /// <summary>
        /// Gets or sets the duration from the time the cache entry when it should be evicted.
        /// </summary>
        [HtmlAttributeName(ExpiresAfterAttributeName)]
        public TimeSpan? ExpiresAfter { get; set; }

        /// <summary>
        /// Gets or sets the duration from last access that the cache entry should be evicted.
        /// </summary>
        [HtmlAttributeName(ExpiresSlidingAttributeName)]
        public TimeSpan? ExpiresSliding { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CachePreservationPriority"/> policy for the cache entry.
        /// </summary>
        [HtmlAttributeName(CachePriorityAttributeName)]
        public CachePreservationPriority? Priority { get; set; }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var key = GenerateKey(context);
            string result;
            if (!MemoryCache.TryGetValue(key, out result))
            {
                result = await context.GetChildContentAsync();
                MemoryCache.Set(key, cacheSetContext =>
                {
                    UpdateCacheContext(cacheSetContext);
                    return result;
                });
            }

            // Clear the contents of the "cache" element since we don't want to render it.
            output.SupressOutput();

            output.Content = result;
        }

        // Internal for unit testing
        internal string GenerateKey(TagHelperContext context)
        {
            var builder = new StringBuilder(nameof(CacheTagHelper));
            builder.Append(CacheKeyTokenSeparator)
                   .Append(context.UniqueId);

            var request = ViewContext.HttpContext.Request;

            if (!string.IsNullOrEmpty(VaryBy))
            {
                builder.Append(CacheKeyTokenSeparator)
                        .Append(nameof(VaryBy))
                        .Append(CacheKeyTokenSeparator)
                        .Append(VaryBy);
            }

            BuildStringCollectionKey(builder, nameof(VaryByCookie), VaryByCookie, request.Cookies);
            BuildStringCollectionKey(builder, nameof(VaryByHeader), VaryByHeader, request.Headers);
            BuildStringCollectionKey(builder, nameof(VaryByQuery), VaryByQuery, request.Query);
            BuildVaryByRouteKey(builder);

            if (VaryByUser)
            {
                builder.Append(CacheKeyTokenSeparator)
                       .Append(nameof(VaryByUser))
                       .Append(CacheKeyTokenSeparator)
                       .Append(ViewContext.HttpContext.User?.Identity?.Name);
            }

            // The key is typically too long to be useful, so we use a cryptographic hash
            // as the actual key (better randomization and key distribution, so small vary
            // values will generate dramtically different keys).
            using (var sha = SHA256.Create())
            {
                var contentBytes = Encoding.UTF8.GetBytes(builder.ToString());
                var hashedBytes = sha.ComputeHash(contentBytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Internal for unit testing
        internal void UpdateCacheContext(ICacheSetContext cacheSetContext)
        {
            if (ExpiresOn != null)
            {
                cacheSetContext.SetAbsoluteExpiration(ExpiresOn.Value);
            }

            if (ExpiresAfter != null)
            {
                cacheSetContext.SetAbsoluteExpiration(ExpiresAfter.Value);
            }

            if (ExpiresSliding != null)
            {
                cacheSetContext.SetSlidingExpiration(ExpiresSliding.Value);
            }

            if (Priority != null)
            {
                cacheSetContext.SetPriority(Priority.Value);
            }
        }

        private static void BuildStringCollectionKey(StringBuilder builder,
                                                     string keyName,
                                                     string value,
                                                     IReadableStringCollection sourceCollection)
        {
            if (!string.IsNullOrEmpty(value))
            {
                // keyName(param1=value1|param2=value2)
                builder.Append(CacheKeyTokenSeparator)
                        .Append(keyName)
                        .Append("(");

                var tokenFound = false;
                foreach (var item in Tokenize(value))
                {
                    tokenFound = true;

                    builder.Append(item)
                           .Append(CacheKeyTokenSeparator)
                           .Append(sourceCollection[item])
                           .Append(CacheKeyTokenSeparator);
                }

                if (tokenFound)
                {
                    // Remove the trailing separator
                    builder.Length -= CacheKeyTokenSeparator.Length;
                }

                builder.Append(")");
            }
        }

        private void BuildVaryByRouteKey(StringBuilder builder)
        {
            var tokenFound = false;

            if (!string.IsNullOrEmpty(VaryByRoute))
            {
                builder.Append(CacheKeyTokenSeparator)
                       .Append(nameof(VaryByRoute))
                       .Append("(");

                foreach (var route in Tokenize(VaryByRoute))
                {
                    tokenFound = true;

                    builder.Append(route)
                           .Append(CacheKeyTokenSeparator)
                           .Append(ViewContext.RouteData.Values[route])
                           .Append(CacheKeyTokenSeparator);
                }

                if (tokenFound)
                {
                    builder.Length -= CacheKeyTokenSeparator.Length;
                }

                builder.Append(")");
            }
        }

        private static IEnumerable<string> Tokenize(string value)
        {
            var index = 0;
            do
            {
                var nextIndex = value.IndexOf(',', index);
                var length = nextIndex == -1 ? value.Length - index : nextIndex - index;
                if (length > 0)
                {
                    yield return value.Substring(index, length);
                }
                index = nextIndex + 1;
            } while (index != 0);
        }
    }
}