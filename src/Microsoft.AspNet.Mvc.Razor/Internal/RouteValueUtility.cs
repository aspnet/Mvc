// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.AspNet.Mvc.Razor.Internal
{
    /// <summary>
    /// Utility for reading case-normalized route values.
    /// </summary>
    public class RouteValueUtility
    {
        /// <summary>
        /// Gets the case-normalized route value for the specified route <paramref name="key"/>.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <param name="key">The route key to lookup.</param>
        /// <returns>The value corresponding to the key.</returns>
        /// <remarks>
        /// The casing of a route value in <see cref="ActionContext.RouteData"/> is determined by the client.
        /// This making constructing paths for view locations in a case sensitive file system unreliable. Using the
        /// <see cref="ActionDescriptor.RouteValueDefaults"/> for attribute routes and
        /// <see cref="ActionDescriptor.RouteConstraints"/> for traditional routes to get route values produces
        /// consistently cased results.
        /// </remarks>
        public static string GetNormalizedRouteValue(ActionContext context, string key)
        {
            var actionDescriptor = context.ActionDescriptor;
            if (actionDescriptor.AttributeRouteInfo != null)
            {
                // For attribute routes, use an inline constraint if available.
                object match;
                if (actionDescriptor.RouteValueDefaults.TryGetValue(key, out match))
                {
                    return match.ToString();
                }
            }
            else
            {
                // For traditional routes, lookup the key in RouteConstraints if the key is RequireKey.
                var match = actionDescriptor.RouteConstraints.FirstOrDefault(
                    constraint => string.Equals(constraint.RouteKey, key, StringComparison.OrdinalIgnoreCase));
                if (match != null && match.KeyHandling != RouteKeyHandling.CatchAll)
                {
                    if (match.KeyHandling == RouteKeyHandling.DenyKey)
                    {
                        return null;
                    }

                    return match.RouteValue;
                }
            }

            object routeValue;
            if (context.RouteData.Values.TryGetValue(key, out routeValue))
            {
                return routeValue.ToString();
            }

            return null;
        }
    }
}