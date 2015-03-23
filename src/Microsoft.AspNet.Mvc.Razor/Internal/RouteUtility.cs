// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.Mvc.Razor.Internal
{
    /// <summary>
    /// Utility for normalizing route values.
    /// </summary>
    public class RouteUtility
    {
        /// <summary>
        /// In a case sensitive file system, Views with a case different from the URL will not be found
        /// if we are looking only in the route data. Hence looking at the ActionDescriptor as well.
        /// </summary>
        /// <param name="key">The key to lookup.</param>
        /// <param name="context">The key is searched in this <see cref="ActionContext"/>.</param>
        /// <returns>The value corresponding to the key.</returns>
        public static string GetNormalizedRouteValue(string key, ActionContext context)
        {
            if (context.ActionDescriptor.AttributeRouteInfo == null)
            {
                var match = context.ActionDescriptor.RouteConstraints.FirstOrDefault(
                    r => string.Equals(r.RouteKey, key, StringComparison.OrdinalIgnoreCase));
                if (match != null && match.KeyHandling != RouteKeyHandling.CatchAll)
                {
                    if (match.KeyHandling == RouteKeyHandling.DenyKey)
                    {
                        return null;
                    }

                    return match.RouteValue;
                }
            }
            else
            {
                object match;
                context.ActionDescriptor.RouteValueDefaults.TryGetValue(key, out match);
                if (match != null)
                {
                    return match.ToString();
                }
            }

            return context.RouteData.Values.GetValueOrDefault<string>(key);
        }
    }
}