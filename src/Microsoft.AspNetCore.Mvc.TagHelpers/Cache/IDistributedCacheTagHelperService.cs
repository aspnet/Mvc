﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Distributed;

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Cache
{
    /// <summary>
    /// An implementation of this interface provides a service to process
    /// the content or fetches it from cache for distributed cache tag helpers.
    /// </summary>
    public interface IDistributedCacheTagHelperService
    {
        /// <summary>
        /// Processes the html content of a distributed cache tag helper.
        /// </summary>
        /// <param name="output">The <see cref="TagHelperOutput" />.</param>
        /// <param name="key">The key in the storage.</param>
        /// <param name="options">The <see cref="DistributedCacheEntryOptions"/>.</param>
        /// <returns>A cached or new content for the cache tag helper.</returns>
        Task<IHtmlContent> ProcessContentAsync(TagHelperOutput output, CacheTagKey key, DistributedCacheEntryOptions options);
    }
}
