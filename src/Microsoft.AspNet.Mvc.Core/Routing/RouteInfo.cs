﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Routing.Template;

namespace Microsoft.AspNet.Mvc.Routing
{
    /// <summary>
    /// Information about the route entry associated with an action.
    /// </summary>
    public class RouteInfo
    {
        /// <summary>
        /// The route precedence.
        /// </summary>
        public decimal Precedence { get; set; }

        /// <summary>
        /// The parsed route template.
        /// </summary>
        public Template Template { get; set; }

        /// <summary>
        /// The plain-text of the route template.
        /// </summary>
        public string TemplateText { get; set; }

        /// <summary>
        /// The route group - a value that is shared between identical routes.
        /// </summary>
        public string RouteGroup { get; set; }
    }
}