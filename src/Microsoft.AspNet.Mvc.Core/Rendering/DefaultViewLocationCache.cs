// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNet.MemoryCache;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <inheritdoc />
    public class DefaultViewLocationCache : IViewLocationCache, IDisposable
    {
        private const char TokenSeparator = '|';
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultViewLocationCache"/>.
        /// </summary>
        public DefaultViewLocationCache()
        {
            _memoryCache = new MemoryCache.MemoryCache();
        }

        /// <inheritdoc />
        public ViewLocationCacheResult Get([NotNull] SortedDictionary<string, string> values)
        {
            var builder = new StringBuilder();
            foreach (var item in values)
            {
                builder.Append(item.Key)
                       .Append(TokenSeparator)
                       .Append(item.Value)
                       .Append(TokenSeparator);
            }

            var key = builder.ToString();
            if (_memoryCache.TryGetValue<string>(key, out var value))
            {
                return new ViewLocationCacheResult(key, value);
            }

            return new ViewLocationCacheResult(key, value: null);
        }

        /// <inheritdoc />
        public void Set([NotNull] string key, [NotNull] string value)
        {
            _memoryCache.Set(key, value);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _memoryCache.Dispose();
        }
    }
}