﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.Mvc.Core;

namespace Microsoft.AspNet.Mvc.ReflectedModelBuilder
{
    public class ReflectedActionModel
    {
        public ReflectedActionModel([NotNull] MethodInfo actionMethod)
        {
            ActionMethod = actionMethod;

            // CoreCLR returns IEnumerable<Attribute> from GetCustomAttributes - the OfType<object>
            // is needed to so that the result of ToList() is List<object>
            Attributes = actionMethod.GetCustomAttributes(inherit: true).OfType<object>().ToList();

            Filters = Attributes.OfType<IFilter>().ToList();

            var routeTemplateAttribute = Attributes.OfType<IRouteTemplateProvider>().FirstOrDefault();
            if (routeTemplateAttribute != null)
            {
                RouteInfo = new RouteInfo(
                    routeTemplateAttribute.Template,
                    routeTemplateAttribute.Name,
                    routeTemplateAttribute.Order,
                    routeTemplateAttribute.Constraints,
                    routeTemplateAttribute.DataTokens,
                    routeTemplateAttribute.Defaults);
            }

            HttpMethods = new List<string>();
            Parameters = new List<ReflectedParameterModel>();
        }

        public MethodInfo ActionMethod { get; private set; }

        public string ActionName { get; set; }

        public List<object> Attributes { get; private set; }

        public List<IFilter> Filters { get; private set; }

        public List<string> HttpMethods { get; private set; }

        public bool IsActionNameMatchRequired { get; set; }

        public List<ReflectedParameterModel> Parameters { get; private set; }

        public RouteInfo RouteInfo { get; set; }
    }
}