﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Routing
{
    /// <summary>
    /// Represents the routing information for an actions that is attribute routed.
    /// </summary>
    public class AttributeRouteInfo
    {
        /// <summary>
        /// The route template May be null if the action has no attribute routes.
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// Gets the order of the route associated with this <see cref="ActionDescriptor"/>. This property determines
        /// the order in which routes get executed. Routes with a lower order value are tried first. In case a route 
        /// doesn't specify a value, it gets a default order of 0.
        /// </summary>
        public int Order { get; set; }
    }
}