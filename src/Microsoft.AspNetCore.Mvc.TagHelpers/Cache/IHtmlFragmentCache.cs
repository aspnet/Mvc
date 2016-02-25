// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Cache
{
    /// <summary>
    /// An implementation of this interface provides a service to cache html fragments.
    /// One client of this service is <see cref="CacheTagHelper"/>.
    /// </summary>
    public interface IHtmlFragmentCache
    {
        /// <summary>
        /// Evaluates the <see cref="IHtmlContent"/> to render and caches it.
        /// </summary>
        /// <param name="key">The unique key reprsenting the fragment to cache.</param>
        /// <param name="renderContent">A delegate rendering the content to cache.</param>
        /// <param name="context">The context of the operation.</param>
        /// <returns>The actual value that is cache.</returns>
        Task<IHtmlContent> SetAsync(string key, Func<Task<IHtmlContent>> renderContent, HtmlFragmentCacheContext context);

        /// <summary>
        /// Retrieves the value with the specified key from the cache.
        /// </summary>
        /// <param name="key">The unique key reprsenting the fragment to cache.</param>
        /// <param name="context">The context of the operation.</param>
        /// <returns>The value that is cached or <value>null</value> if it couldn't be found.</returns>
        Task<IHtmlContent> GetValueAsync(string key, HtmlFragmentCacheContext context);
    }
}
