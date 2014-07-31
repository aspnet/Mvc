// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Routing;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Identifies an action that only supports the HTTP POST method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpPostAttribute : Attribute, IActionHttpMethodProvider, IRouteTemplateProvider
    {
        private static readonly IEnumerable<string> _supportedMethods = new string[] { "POST" };

        private int? _order;

        /// <summary>
        /// Creates a new <see cref="HttpPostAttribute"/>.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        public HttpPostAttribute()
        {
        }

        /// <summary>
        /// Creates a new <see cref="HttpPostAttribute"/> with the given route template.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        public HttpPostAttribute([NotNull] string template)
        {
            Template = template;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods
        {
            get { return _supportedMethods; }
        }

        /// <summary>
        /// The route template. May be null.
        /// </summary>
        public string Template { get; private set; }

        /// <summary>
        /// Gets the route order. The order determines the order of route execution. Routes with a lower
        /// order value are tried first. When a route doesn't specify a value, it gets the value of the
        /// <see cref="RouteAttribute.Order"/> or a default value of 0 if the <see cref="RouteAttribute"/>
        /// doesn't define a value.
        /// </summary>
        public int Order
        {
            get { return _order ?? 0; }
            set { _order = value; }
        }

        /// <inheritdoc />
        string IRouteTemplateProvider.Template
        {
            get
            {
                return Template;
            }
        }

        /// <inheritdoc />
        int? IRouteTemplateProvider.Order
        {
            get
            {
                return _order;
            }
        }
    }
}