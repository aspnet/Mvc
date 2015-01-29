// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc
{
	public class CacheProfile
	{
        /// <summary>
        /// Gets or sets the name of the CacheProfile.
        /// The CacheProfile is referred in the ResponseCache attribute using this name.
        /// </summary>
        public string Name { get; set; }

		/// <summary>
        /// Gets or sets the duration in seconds for which the response is cached.
        /// This is a required parameter.
        /// This sets "max-age" in "Cache-control" header.
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// Gets or sets the location where the data from a particular URL must be cached.
        /// This sets the value for "Cache-control" header.
        /// </summary>
        public ResponseCacheLocation? Location { get; set; }

        /// <summary>
        /// Gets or sets the value which determines whether the data should be stored or not.
        /// When set to true, it sets "Cache-control" header to "no-store".
        /// Ignores the "Location" parameter for values other than "None".
        /// Ignores the "duration" parameter.
        /// </summary>
        public bool? NoStore { get; set; }

        /// <summary>
        /// Gets or sets the value for the Vary response header.
        /// </summary>
        public string VaryByHeader { get; set; }
	}
}