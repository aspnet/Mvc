// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Routing;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Core
{
    /// <summary>
    /// Represents the routing information for a given <see cref="ActionDescriptor"/>.
    /// </summary>
    public class RouteInfo
    {
        /// <summary>
        /// Creates a new instance of <see cref="RouteInfo"/>.
        /// </summary>
        /// <param name="name">The name of the route.</param>
        /// <param name="order">The order of the route.</param>
        /// <param name="constraints">The <see cref="IDictionary{TKey, TValue}"/> of constraints applicable to the route.</param>
        /// <param name="dataTokens">The <see cref="IDictionary{TKey, TValue}"/> of data tokens associated with the route.</param>
        /// <param name="defaults">The <see cref="IDictionary{TKey, TValue}"/> of default values associated with the parameters
        /// of the route.</param>
        public RouteInfo(
            string template,
            string name,
            int? order,
            IDictionary<string,IRouteConstraint> constraints,
            IDictionary<string, object> dataTokens,
            IDictionary<string, object> defaults)
        {
            Template = template;
            Name = name;
            Order = order;
            Constraints = constraints;
            DataTokens = dataTokens;
            Defaults = defaults;
        }

        /// <summary>
        /// The route template.
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// Gets the name of the route associated with this <see cref="ActionDescriptor"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the order of the route associated with this <see cref="ActionDescriptor"/>. The order determines the order
        /// of route execution. Routes with a lower order value are tried first. When a route doesn't specify a value, it
        /// gets a default value of 0. The default value is 0. A route with a lower Order value has more priority than a
        /// route with a higher Order value.
        /// </summary>
        public int? Order { get; set; }

        /// <summary>
        /// Additional constraints applied to the route.
        /// </summary>
        public IDictionary<string, IRouteConstraint> Constraints { get; set; }

        /// <summary>
        /// Extra data associated with the route.
        /// </summary>
        public IDictionary<string, object> DataTokens { get; set; }

        /// <summary>
        /// Default values associated with the route.
        /// </summary>
        public IDictionary<string, object> Defaults { get; set; }
    }
}