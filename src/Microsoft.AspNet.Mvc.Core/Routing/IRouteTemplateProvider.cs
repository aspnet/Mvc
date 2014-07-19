// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Routing;

namespace Microsoft.AspNet.Mvc.Routing
{
    /// <summary>
    /// Interface for attributes which can supply a route template for attribute routing.
    /// </summary>
    public interface IRouteTemplateProvider
    {
        /// <summary>
        /// The route template. May be null.
        /// </summary>
        string Template { get; }

        /// <summary>
        /// Gets the route order. The order determines the order of route execution. Routes with a lower
        /// order value are tried first. When a route doesn't specify a value, it gets a default value of 0.
        /// </summary>
        int? Order { get; }

        /// <summary>
        /// The route name. May be null. When a value is given, it can be used to reference the route for
        /// link generation purposes.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Additional constraints applied to the route.
        /// </summary>
        IDictionary<string, IRouteConstraint> Constraints { get; }

        /// <summary>
        /// Extra data associated with the route.
        /// </summary>
        IDictionary<string, object> DataTokens { get; }

        /// <summary>
        /// Default values associated with the route.
        /// </summary>
        IDictionary<string, object> Defaults { get; }
    }
}