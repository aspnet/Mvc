// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Cache
{
    /// <summary>
    /// Context object for html fragment caching.
    /// </summary>
    public class HtmlFragmentCacheContext
    {
        private IDictionary<string, string> _options;

        /// <summary>
        /// Creates a new <see cref="HtmlFragmentCacheContext"/>.
        /// </summary>
        /// <param name="htmlEncoder">The <see cref="HtmlEncoder"/>.</param>
        public HtmlFragmentCacheContext(HtmlEncoder htmlEncoder)
        {
            HtmlEncoder = htmlEncoder;
        }

        /// <summary>
        /// Gets or sets the exact <see cref="DateTimeOffset"/> the cache entry should be evicted.
        /// </summary>
        public DateTimeOffset? ExpiresOn { get; set; }

        /// <summary>
        /// Gets or sets the duration, from the time the cache entry was added, when it should be evicted.
        /// </summary>
        public TimeSpan? ExpiresAfter { get; set; }

        /// <summary>
        /// Gets or sets the duration from last access that the cache entry should be evicted.
        /// </summary>
        public TimeSpan? ExpiresSliding { get; set; }

        /// <summary>
        /// Gets whether this context has options or not. This should be called before accessing
        /// the <see cref="Options"/> values.
        /// </summary>
        public bool HasOptions => _options?.Count > 0;

        /// <summary>
        /// Gets the options to be used by the <see cref="IHtmlFragmentCache"/> 
        /// specific implementation.
        /// </summary>
        public IDictionary<string, string> Options
        {
            get
            {
                if (_options == null)
                {
                    _options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                return _options;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Text.Encodings.Web.HtmlEncoder"/> which encodes the content to be cached.
        /// </summary>
        public HtmlEncoder HtmlEncoder { get; }
    }
}
