// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Cache
{
    /// <summary>
    /// Implements <see cref="IDistributedCacheTagHelperService"/> and ensure
    /// multiple concurrent requests are gated.
    /// </summary>
    public class DistributedCacheTagHelperService : IDistributedCacheTagHelperService
    {
        private readonly IDistributedCacheTagHelperStorage _storage;
        private readonly IDistributedCacheTagHelperFormatter _formatter;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly ConcurrentDictionary<CacheTagKey, Task<IHtmlContent>> _workers;

        public DistributedCacheTagHelperService(
            IDistributedCacheTagHelperStorage storage,
            IDistributedCacheTagHelperFormatter formatter,
            HtmlEncoder HtmlEncoder 
        )
        {
            _formatter = formatter;
            _storage = storage;
            _htmlEncoder = HtmlEncoder;

            _workers = new ConcurrentDictionary<CacheTagKey, Task<IHtmlContent>>();
        }

        /// <inheritdoc />
        public async Task<IHtmlContent> ProcessContentAsync(TagHelperOutput output, CacheTagKey key, DistributedCacheEntryOptions options)
        {
            IHtmlContent content = null;

            while (content == null)
            {
                Task<IHtmlContent> result = null;

                // Is there any request already processing the value?
                if (!_workers.TryGetValue(key, out result))
                {
                    var tcs = new TaskCompletionSource<IHtmlContent>();

                    _workers.TryAdd(key, tcs.Task);

                    try
                    {
                        var serializedKey = Encoding.UTF8.GetBytes(key.GenerateKey());
                        var storageKey = key.GenerateHashedKey();
                        var value = await _storage.GetAsync(storageKey);
                                                
                        if (value == null)
                        {
                            var processedContent = await output.GetChildContentAsync();

                            var stringBuilder = new StringBuilder();
                            using (var writer = new StringWriter(stringBuilder))
                            {
                                processedContent.WriteTo(writer, _htmlEncoder);
                            }

                            var formattingContext = new DistributedCacheTagHelperFormattingContext
                            {
                                Html = new HtmlString(stringBuilder.ToString())
                            };

                            value = await _formatter.SerializeAsync(formattingContext);

                            
                            using (var buffer = new MemoryStream())
                            {
                                // The stored content is 
                                // - Length of the serialized cache key in bytes
                                // - Cache Key
                                // - Content

                                var keyLength = BitConverter.GetBytes(serializedKey.Length);

                                await buffer.WriteAsync(keyLength, 0, keyLength.Length);
                                await buffer.WriteAsync(serializedKey, 0, serializedKey.Length);
                                await buffer.WriteAsync(value, 0, value.Length);

                                await _storage.SetAsync(storageKey, buffer.ToArray(), options);
                            }

                            content = formattingContext.Html;
                        }
                        else
                        {
                            // Extract the length of the serialized key
                            byte[] contentBuffer = null;
                            using (var buffer = new MemoryStream(value))
                            {
                                var keyLengthBuffer = new byte[sizeof(int)];
                                await buffer.ReadAsync(keyLengthBuffer, 0, keyLengthBuffer.Length);

                                var keyLength = BitConverter.ToInt32(keyLengthBuffer, 0);
                                var serializedKeyBuffer = new byte[keyLength];
                                await buffer.ReadAsync(serializedKeyBuffer, 0, serializedKeyBuffer.Length);

                                // Ensure we are reading the expected key before continuing
                                if (serializedKeyBuffer == serializedKey)
                                {
                                    contentBuffer = new byte[value.Length - keyLengthBuffer.Length - serializedKeyBuffer.Length];
                                    await buffer.ReadAsync(contentBuffer, 0, contentBuffer.Length);
                                }
                            }

                            try
                            {
                                if (contentBuffer != null)
                                {
                                    content = await _formatter.DeserializeAsync(contentBuffer);
                                }
                            }
                            finally
                            {
                                // If the deserialization fails, it can return null, for instance when the 
                                // value is not in the expected format, or the keys have collisions.
                                if (content == null)
                                {
                                    content = await output.GetChildContentAsync();
                                }
                            }                            
                        }

                        tcs.TrySetResult(content);
                    }
                    catch
                    {
                        tcs.TrySetResult(null);
                        throw;
                    }
                    finally
                    {
                        // Remove the worker task from the in-memory cache
                        Task<IHtmlContent> worker;
                        _workers.TryRemove(key, out worker);
                    }
                }
                else
                {
                    content = await result;
                }
            }

            return content;
        }
    }
}
