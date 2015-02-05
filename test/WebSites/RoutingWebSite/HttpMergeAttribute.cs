﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Routing;

namespace RoutingWebSite
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpMergeAttribute : Attribute, IActionHttpMethodProvider, IRouteTemplateProvider
    {
        private static readonly IEnumerable<string> _supportedMethods = new[] { "MERGE" };

        public HttpMergeAttribute(string template)
        {
            Template = template;
        }

        public IEnumerable<string> HttpMethods
        {
            get { return _supportedMethods; }
        }

        /// <inheritdoc />
        public string Template { get; private set; }

        /// <inheritdoc />
        public int? Order { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }
    }
}