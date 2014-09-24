// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Default implementation of <see cref="IViewLocationCache"/>.
    /// </summary>
    public class DefaultViewLocationCache : IViewLocationCache
    {
        // A mapping of keys generated from ViewLocationExpanderContext to view locations.
        private readonly ConcurrentDictionary<string, string> _cache;

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultViewLocationCache"/>.
        /// </summary>
        public DefaultViewLocationCache()
        {
            _cache = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
        }

        /// <inheritdoc />
        public ViewLocationCacheResult Get([NotNull] ViewLocationExpanderContext context)
        {
            var keyBuilder = new StringBuilder();
            var routeValues = context.ActionContext.RouteData.Values;
            // format is "{viewName}:{controllerName}:{areaName}:"
            keyBuilder.Append(context.ViewName)
                      .Append(':')
                      .Append(GetRouteValue(routeValues, "controller"));

            var area = GetRouteValue(routeValues, "area");
            if (!string.IsNullOrEmpty(area))
            {
                keyBuilder.Append(':')
                          .Append(area);
            }

            if (context.Values != null)
            {
                var valuesDictionary = context.Values;
                foreach (var key in valuesDictionary.Keys.OrderBy(k => k, StringComparer.Ordinal))
                {
                    keyBuilder.Append(':')
                              .Append(key)
                              .Append(':')
                              .Append(valuesDictionary[key]);
                }
            }

            var cacheKey = keyBuilder.ToString();
            _cache.TryGetValue(cacheKey, out var result);
            return new ViewLocationCacheResult(cacheKey, result);
        }

        /// <inheritdoc />
        public void Set([NotNull] object cacheKey,
                        [NotNull] string value)
        {
            var stringKey = cacheKey as string;
            if (stringKey == null)
            {
                var message = Resources.FormatViewLocationCache_KeyMustBeString(nameof(cacheKey),
                                                                                nameof(String),
                                                                                nameof(Get));
                throw new ArgumentException(message, nameof(cacheKey));
            }
            _cache.TryAdd(stringKey, value);
        }

        private static string GetRouteValue(IDictionary<string, object> routeValues, string key)
        {
            routeValues.TryGetValue(key, out var value);
            return value as string;
        }
    }
}