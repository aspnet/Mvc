// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.Routing;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Specifies an attribute route on a controller. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RouteAttribute : Attribute, IRouteTemplateProvider
    {
        /// <summary>
        /// Creates a new <see cref="RouteAttribute"/> with the given route template.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        public RouteAttribute([NotNull] string template)
        {
            Template = template;
        }

        /// <inheritdoc />
        public string Template { get; private set; }

        /// <inheritdoc />
        public virtual string Name { get; set; }

        /// <inheritdoc />
        public virtual int? Order { get; set; }

        /// <inheritdoc />
        public virtual IDictionary<string, IRouteConstraint> Constraints
        {
            get { return null; }
        }

        /// <inheritdoc />
        public virtual IDictionary<string, object> Defaults
        {
            get { return null; }
        }

        /// <inheritdoc />
        public virtual IDictionary<string, object> DataTokens
        {
            get { return null; }
        }
    }
}