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
    public class RouteDataConstraintsContainsKeyConstraint : IRouteConstraint
    {
        public bool Match([NotNull] HttpContext httpContext,
                          [NotNull] IRouter route,
                          [NotNull] string routeKey,
                          [NotNull] IDictionary<string, object> values,
                          RouteDirection routeDirection)
        {
            object value = null;
            if (values.TryGetValue(routeKey, out value))
            {
                string valueAsString = value as string;

                if (valueAsString != null)
                {
                    var allValues = GetAllMatchingValues(routeKey, httpContext);
                    var match = allValues.Any(existingRouteValue => 
                                                existingRouteValue.Equals(
                                                                    valueAsString,
                                                                    StringComparison.OrdinalIgnoreCase));

                    return match;
                }
            }

            return false;
        }

        private IEnumerable<string> GetAllMatchingValues(string routeKey, HttpContext httpContext)
        {
            var provider = httpContext.ApplicationServices
                                      .GetService<INestedProviderManager<ActionDescriptorProviderContext>>();
            var context = new ActionDescriptorProviderContext();
            provider.Invoke(context);

            var routeValueCollection = context
                                        .Results
                                        .Select(ad => ad.RouteConstraints
                                                        .FirstOrDefault(c => c.RouteKey == routeKey &&
                                                                        c.KeyHandling == RouteKeyHandling.RequireKey))
                                        .Where(rc => rc != null)
                                        .Select(rc => rc.RouteValue)
                                        .Distinct();

            return routeValueCollection;
        }
    }
}
