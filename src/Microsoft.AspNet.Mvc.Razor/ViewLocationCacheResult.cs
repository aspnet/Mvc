// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// The result of a lookup in <see cref="IViewLocationCache"/>.
    /// </summary>
    public class ViewLocationCacheResult
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ViewLocationCacheResult"/>.
        /// </summary>
        /// <param name="cacheKey">The computed cache key for ths lookup.</param>
        /// <param name="viewLocation">The result of the lookup.</param>
        public ViewLocationCacheResult([NotNull] string cacheKey,
                                       string viewLocation)
        {
            CacheKey = cacheKey;
            ViewLocation = viewLocation;
        }

        /// <summary>
        /// Gets the computed cache key for the lookup.
        /// </summary>
        public object CacheKey { get; private set; }

        /// <summary>
        /// Gets the location of the view if found in cache.
        /// </summary>
        public string ViewLocation { get; private set; }
    }
}