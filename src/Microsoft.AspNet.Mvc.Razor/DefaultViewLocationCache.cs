// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Default implementation of <see cref="IViewLocationCache"/>.
    /// </summary>
    public class DefaultViewLocationCache : IViewLocationCache
    {
        private const char CacheKeySeparator = ':';

        // A mapping of keys generated from ViewLocationExpanderContext to view locations.
        private readonly ConcurrentDictionary<string, ViewLocationCacheResult> _cache;

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultViewLocationCache"/>.
        /// </summary>
        public DefaultViewLocationCache()
        {
            _cache = new ConcurrentDictionary<string, ViewLocationCacheResult>(StringComparer.Ordinal);
        }

        /// <inheritdoc />
        public ViewLocationCacheResult Get([NotNull] ViewLocationExpanderContext context)
        {
            var cacheKey = GenerateKey(context);
            ViewLocationCacheResult result;
            if (_cache.TryGetValue(cacheKey, out result))
            {
                return result;
            }

            return ViewLocationCacheResult.None;
        }

        /// <inheritdoc />
        public void Set(
            [NotNull] ViewLocationExpanderContext context,
            [NotNull] ViewLocationCacheResult value)
        {
            var cacheKey = GenerateKey(context);
            _cache.TryAdd(cacheKey, value);
        }

        internal static string GenerateKey(ViewLocationExpanderContext context)
        {
            var keyBuilder = new StringBuilder();
            var routeValues = context.ActionContext.RouteData.Values;
            var controller = RazorViewEngine.GetNormalizedRouteValue(
                context.ActionContext,
                RazorViewEngine.ControllerKey);

            // format is "{viewName}:{isPartial}:{controllerName}:{areaName}:"
            keyBuilder.Append(context.ViewName)
                      .Append(CacheKeySeparator)
                      .Append(context.IsPartial ? 1 : 0)
                      .Append(CacheKeySeparator)
                      .Append(controller);

            var area = RazorViewEngine.GetNormalizedRouteValue(context.ActionContext, RazorViewEngine.AreaKey);
            if (!string.IsNullOrEmpty(area))
            {
                keyBuilder.Append(CacheKeySeparator)
                          .Append(area);
            }

            if (context.Values != null)
            {
                var valuesDictionary = context.Values;
                foreach (var item in valuesDictionary.OrderBy(k => k.Key, StringComparer.Ordinal))
                {
                    keyBuilder.Append(CacheKeySeparator)
                              .Append(item.Key)
                              .Append(CacheKeySeparator)
                              .Append(item.Value);
                }
            }

            var cacheKey = keyBuilder.ToString();
            return cacheKey;
        }
    }
}