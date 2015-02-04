// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Defines a set of settings which can be used for response caching.
    /// </summary>
    public class CacheProfile
    {
        /// <summary>
        /// Gets or sets the duration in seconds for which the response is cached.
        /// If this property is set to a non null value,
        /// the "max-age" in "Cache-control" header is set in the <see cref="HttpContext.Response" />.
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// Gets or sets the location where the data from a particular URL must be cached.
        /// If this property is set to a non null value,
        /// the "Cache-control" header is set in the <see cref="HttpContext.Response" />.
        /// </summary>
        public ResponseCacheLocation? Location { get; set; }

        /// <summary>
        /// Gets or sets the value which determines whether the data should be stored or not.
        /// When set to <see langword="true"/>, it sets "Cache-control" header in
        /// <see cref="HttpContext.Response" /> to "no-store".
        /// Ignores the "Location" parameter for values other than "None".
        /// Ignores the "Duration" parameter.
        /// </summary>
        public bool? NoStore { get; set; }

        /// <summary>
        /// Gets or sets the value for the Vary header in <see cref="HttpContext.Response" />.
        /// </summary>
        public string VaryByHeader { get; set; }
    }
}