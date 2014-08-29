// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.Routing;

namespace RoutingWebSite
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MultipleHttpMethodsAttribute : Attribute, IActionHttpMethodProvider, IRouteTemplateProvider
    {
        private readonly IEnumerable<string> _supportedMethods;

        public MultipleHttpMethodsAttribute(params string[] methods)
            : this(null, methods)
        {
        }

        public MultipleHttpMethodsAttribute(string template, params string[] methods)
        {
            _supportedMethods = methods ?? new string[0];
            Template = template;
        }

        public IEnumerable<string> HttpMethods
        {
            get { return _supportedMethods; }
        }

        /// <inheritdoc />
        public string Template { get; private set; }

        /// <inheritdoc />
        int? IRouteTemplateProvider.Order { get { return _order; } }

        private int? _order;

        public int Order
        {
            get { return _order.GetValueOrDefault(); }
            set { _order = value; }
        }

        /// <inheritdoc />
        public string Name { get; set; }
    }
}