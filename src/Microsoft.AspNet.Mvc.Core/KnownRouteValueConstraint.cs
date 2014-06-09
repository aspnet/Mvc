// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class KnownRouteValueConstraint : IRouteConstraint
    {
        private RouteValuesCollection _cachedValuesCollection;

        public bool Match([NotNull] HttpContext httpContext,
                          [NotNull] IRouter route,
                          [NotNull] string routeKey,
                          [NotNull] IDictionary<string, object> values,
                          RouteDirection routeDirection)
        {
            object value;
            if (values.TryGetValue(routeKey, out value))
            {
                string valueAsString = value as string;

                if (valueAsString != null)
                {
                    var allValues = GetAndCacheAllMatchingValues(routeKey, httpContext);
                    var match = allValues.Any(existingRouteValue => 
                                                existingRouteValue.Equals(
                                                                    valueAsString,
                                                                    StringComparison.OrdinalIgnoreCase));

                    return match;
                }
            }

            return false;
        }

        private IEnumerable<string> GetAndCacheAllMatchingValues(string routeKey, HttpContext httpContext)
        {
            var provider = httpContext.ApplicationServices
                                      .GetService<IActionDescriptorsCollectionProvider>();

            var actionDescriptors = provider.ActionDescriptors;
            var version = actionDescriptors.Version;

            if (_cachedValuesCollection == null ||
                version != _cachedValuesCollection.Version)
            {
                var routeValueCollection = actionDescriptors
                                            .Items
                                            .Select(ad => ad.RouteConstraints
                                                            .FirstOrDefault(c => c.RouteKey == routeKey &&
                                                                            c.KeyHandling == RouteKeyHandling.RequireKey))
                                            .Where(rc => rc != null)
                                            .Select(rc => rc.RouteValue)
                                            .Distinct();

                _cachedValuesCollection = new RouteValuesCollection(version, routeValueCollection);
            }

            return _cachedValuesCollection.Items;
        }

        private class RouteValuesCollection
        {
            public RouteValuesCollection(int version, IEnumerable<string> items)
            {
                Version = version;
                Items = items;
            }

            public int Version { get; private set; }

            public IEnumerable<string> Items
            {
                get;

                private set;
            }
        }
    }
}
