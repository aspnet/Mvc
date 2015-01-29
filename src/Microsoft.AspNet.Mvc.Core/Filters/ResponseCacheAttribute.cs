// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ResponseCacheAttribute : Attribute, IFilterFactory
    {
        // A nullable-int cannot be used as an Attribute parameter.
        // Hence this nullable-int is present to back the Duration property.
        // The same goes for nullable-ResponseCacheLocation and nullable-bool.
        private int? _duration;
        private ResponseCacheLocation? _location;
        private bool? _noStore;

        /// <summary>
        /// Gets or sets the duration in seconds for which the response is cached.
        /// This is a required parameter.
        /// This sets "max-age" in "Cache-control" header.
        /// </summary>
        public int Duration
        {
            get
            {
                return _duration ?? 0;
            }
            set
            {
                _duration = value;
            }
        }

        /// <summary>
        /// Gets or sets the location where the data from a particular URL must be cached.
        /// </summary>
        public ResponseCacheLocation Location
        {
            get
            {
                return _location ?? ResponseCacheLocation.Any;
            }
            set
            {
                _location = value;
            }
        }

        /// <summary>
        /// Gets or sets the value which determines whether the data should be stored or not.
        /// When set to true, it sets "Cache-control" header to "no-store".
        /// Ignores the "Location" parameter for values other than "None".
        /// Ignores the "duration" parameter.
        /// </summary>
        public bool NoStore
        {
            get
            {
                return _noStore ?? false;
            }
            set
            {
                _noStore = value;
            }
        }

        /// <summary>
        /// Gets or sets the value for the Vary response header.
        /// </summary>
        public string VaryByHeader { get; set; }

        /// <summary>
        /// Gets or sets the value of the Cache Profile.
        /// </summary>
        public string CacheProfileName { get; set; }

        public IFilter CreateInstance([NotNull] IServiceProvider serviceProvider)
        {
            var optionsAccessor = serviceProvider.GetRequiredService<IOptions<MvcOptions>>();
            var selectedProfile =
                optionsAccessor.Options.CacheProfiles.FirstOrDefault(
                    p => string.Equals(p.Name, CacheProfileName, StringComparison.OrdinalIgnoreCase));

            // If the ResponseCacheAttribute parameters are set,
            // then it must override the values from the Cache Profile.
            // The below expression first checks if the duration is set by the attribute's parameter.
            // If absent, it checks the selected cache profile (Note: There can be no cache profile as well)
            // The same is the case for other properties.
            _duration = _duration ?? selectedProfile?.Duration;
            _noStore = _noStore ?? selectedProfile?.NoStore;
            _location = _location ?? selectedProfile?.Location;
            VaryByHeader = VaryByHeader ?? selectedProfile?.VaryByHeader;

            if (NoStore != true)
            {
                // Duration MUST be set if NoStore is false. Either in the cache profile or in the attribute.
                if (_duration == null)
                {
                    throw new InvalidOperationException(
                            Resources.FormatResponseCache_SpecifyDuration(nameof(NoStore), nameof(Duration)));
                }
            }

            // ResponseCacheFilter cannot take any null values. Hence, if there are any null values,
            // the properties convert them to thier defaults and are passed on.
            return new ResponseCacheFilter(Duration, Location, NoStore, VaryByHeader);
        }
    }
}