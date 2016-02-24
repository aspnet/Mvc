// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TagHelperServicesExtensions
    {
        public static IMvcCoreBuilder AddCacheTagHelper(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            AddTagHelperServices(builder.Services);
            return builder;
        }

        // Internal for testing.
        internal static void AddTagHelperServices(IServiceCollection services)
        {
            // Consumed by the Cache tag helper to cache results across the lifetime of the application.
            services.TryAddSingleton<IMemoryCache, MemoryCache>();
            services.TryAddSingleton<IHtmlFragmentCache, HybridHtmlFragmentCache>();
        }
    }
}