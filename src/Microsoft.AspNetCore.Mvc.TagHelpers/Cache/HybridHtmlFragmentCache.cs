// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Cache
{
    /// <summary>
    /// This implementation of <see cref="IHtmlFragmentCache"/> is able to switch from 
    /// <see cref="IMemoryCache"/> to <see cref="IDistributedCache"/>.
    /// </summary>
    /// <remarks>
    /// This service accepts the following options:
    /// - <code>options-distributed</code> with <code>true</code> to store the cache entry
    /// in the <see cref="IDistributedCache"/> service.
    /// - <code>options-priority</code> with a value from <see cref="CacheItemPriority"/> that
    /// can only be used for <see cref="IMemoryCache"/>.
    /// </remarks>
    public class HybridHtmlFragmentCache : IHtmlFragmentCache
    {
        private const string PriorityOptionName = "priority";
        private const string DistributedOptionName = "distributed";

        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;

        /// <summary>
        /// Creates a new <see cref="HybridHtmlFragmentCache"/>.
        /// </summary>
        /// <param name="memoryCache">The <see cref="IMemoryCache"/>.</param>
        public HybridHtmlFragmentCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Creates a new <see cref="HybridHtmlFragmentCache"/>.
        /// </summary>
        /// <param name="memoryCache">The <see cref="IMemoryCache"/>.</param>
        /// <param name="distributedCache">The <see cref="IDistributedCache"/>.</param>
        public HybridHtmlFragmentCache(IMemoryCache memoryCache, IDistributedCache distributedCache)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
        }

        /// <inheritdoc />
        public Task<IHtmlContent> SetAsync(string key, Func<Task<IHtmlContent>> renderContent, HtmlFragmentCacheContext context)
        {
            // is the distributed option set?
            if (EnsureDistributedService(context))
            {
                return SetDistributedAsync(key, renderContent, context, _distributedCache);
            }
            else
            {
                return SetMemoryAsync(key, renderContent, context);
            }
        }

        /// <inheritdoc />
        public Task<IHtmlContent> GetValueAsync(string key, HtmlFragmentCacheContext context)
        {
            // is the distributed option set?
            if (EnsureDistributedService(context))
            {
                return GetDistributedValueAsync(key, _distributedCache);
            }
            else
            {
                IHtmlContent value;
                if (GetMemoryValue(key, out value))
                {
                    return Task.FromResult(value);
                }
                else
                {
                    // no existing cache entry
                    return Task.FromResult<IHtmlContent>(null);
                }
            }
        }

        private async Task<IHtmlContent> SetMemoryAsync(string key, Func<Task<IHtmlContent>> renderContent, HtmlFragmentCacheContext context)
        {
            var options = new MemoryCacheEntryOptions();

            if (context.ExpiresOn != null)
            {
                options.SetAbsoluteExpiration(context.ExpiresOn.Value);
            }

            if (context.ExpiresAfter != null)
            {
                options.SetAbsoluteExpiration(context.ExpiresAfter.Value);
            }

            if (context.ExpiresSliding != null)
            {
                options.SetSlidingExpiration(context.ExpiresSliding.Value);
            }

            // If the priority option is defined, use it
            if (context.HasOptions && context.Options.ContainsKey(PriorityOptionName))
            {
                CacheItemPriority priority;
                if (Enum.TryParse(context.Options[PriorityOptionName], out priority))
                {
                    options.SetPriority(priority);
                }
            }

            // Create an entry link scope and flow it so that any tokens related to the cache entries
            // created within this scope get copied to this scope.
            using (var link = _memoryCache.CreateLinkingScope())
            {
                // The content is lazily evaluated in order to enlist any nested cache token
                // representing a cache dependency.
                var content = await renderContent();

                options.AddEntryLink(link);

                // Serialize the html content as it could be buffered
                var stringBuilder = new StringBuilder();
                using (var writer = new StringWriter(stringBuilder))
                {
                    content.WriteTo(writer, context.HtmlEncoder);
                }

                var result = new StringBuilderHtmlContent(stringBuilder);

                return _memoryCache.Set<IHtmlContent>(key, result, options);
            }
        }

        private async Task<IHtmlContent> SetDistributedAsync(string key, Func<Task<IHtmlContent>> renderContent, HtmlFragmentCacheContext context, IDistributedCache cache)
        {
            var options = new DistributedCacheEntryOptions();

            if (context.ExpiresOn != null)
            {
                options.SetAbsoluteExpiration(context.ExpiresOn.Value);
            }

            if (context.ExpiresAfter != null)
            {
                options.SetAbsoluteExpiration(context.ExpiresAfter.Value);
            }

            if (context.ExpiresSliding != null)
            {
                options.SetSlidingExpiration(context.ExpiresSliding.Value);
            }

            var content = await renderContent();

            var stringBuilder = new StringBuilder();
            using (var writer = new StringWriter(stringBuilder))
            {
                content.WriteTo(writer, context.HtmlEncoder);
            }

            // The content is stored using UTF8 encoding on the distributed cache service.
            var serialized = Encoding.UTF8.GetBytes(stringBuilder.ToString());

            await cache.SetAsync(key, serialized, options);

            return content;
        }
        
        private bool GetMemoryValue(string key, out IHtmlContent value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }

        private async Task<IHtmlContent> GetDistributedValueAsync(string key, IDistributedCache cache)
        {
            var encoded = await cache.GetAsync(key);

            if (encoded == null)
            {
                return null;
            }

            var content = Encoding.UTF8.GetString(encoded);
            return new HtmlEncodedString(content);
        }

        private bool EnsureDistributedService(HtmlFragmentCacheContext context)
        {
            // Ensure the distributed cache service is registered if necessary  
            
            bool distributed;

            if (context.Options.ContainsKey(DistributedOptionName) &&
                bool.TryParse(context.Options[DistributedOptionName], out distributed) &&
                distributed)
            {
                if (_distributedCache == null)
                {
                    throw new NotSupportedException(Resources.FormatCacheTagHelper_NoDistributedCacheService(nameof(IDistributedCache), DistributedOptionName));
                }

                return true;
            }

            return false;
        }

        private class StringBuilderHtmlContent : IHtmlContent
        {
            private readonly StringBuilder _builder;

            public StringBuilderHtmlContent(StringBuilder builder)
            {
                _builder = builder;
            }

            public void WriteTo(TextWriter writer, HtmlEncoder encoder)
            {
                var htmlTextWriter = writer as HtmlTextWriter;
                if (htmlTextWriter != null)
                {
                    htmlTextWriter.Write(this);
                    return;
                }

                for (var i = 0; i < _builder.Length; i++)
                {
                    writer.Write(_builder[i]);
                }
            }
        }
    }
}
