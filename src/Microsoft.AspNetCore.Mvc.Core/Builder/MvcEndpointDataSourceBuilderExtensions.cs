// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class MvcEndpointDataSourceBuilderExtensions
    {
        public static IEndpointConventionBuilder MapMvcRoute(
            this IEndpointRouteBuilder routeBuilder,
            string name,
            string template)
        {
            return MapMvcRoute<ControllerBase>(routeBuilder, name, template, defaults: null);
        }

        public static IEndpointConventionBuilder MapMvcRoute(
            this IEndpointRouteBuilder routeBuilder,
            string name,
            string template,
            object defaults)
        {
            return MapMvcRoute<ControllerBase>(routeBuilder, name, template, defaults, constraints: null);
        }

        public static IEndpointConventionBuilder MapMvcRoute(
            this IEndpointRouteBuilder routeBuilder,
            string name,
            string template,
            object defaults,
            object constraints)
        {
            return MapMvcRoute<ControllerBase>(routeBuilder, name, template, defaults, constraints, dataTokens: null);
        }

        public static IEndpointConventionBuilder MapMvcRoute(
            this IEndpointRouteBuilder routeBuilder,
            string name,
            string template,
            object defaults,
            object constraints,
            object dataTokens)
        {
            return MapMvcRoute<ControllerBase>(routeBuilder, name, template, defaults, constraints, dataTokens);
        }

        public static IEndpointConventionBuilder MapMvcRoute<TController>(
            this IEndpointRouteBuilder routeBuilder,
            string name,
            string template) where TController : ControllerBase
        {
            return MapMvcRoute<TController>(routeBuilder, name, template, defaults: null);
        }

        public static IEndpointConventionBuilder MapMvcRoute<TController>(
            this IEndpointRouteBuilder routeBuilder,
            string name,
            string template,
            object defaults) where TController : ControllerBase
        {
            return MapMvcRoute<TController>(routeBuilder, name, template, defaults, constraints: null);
        }

        public static IEndpointConventionBuilder MapMvcRoute<TController>(
            this IEndpointRouteBuilder routeBuilder,
            string name,
            string template,
            object defaults,
            object constraints) where TController : ControllerBase
        {
            return MapMvcRoute<TController>(routeBuilder, name, template, defaults, constraints, dataTokens: null);
        }

        public static IEndpointConventionBuilder MapMvcRoute<TController>(
            this IEndpointRouteBuilder routeBuilder,
            string name,
            string template,
            object defaults,
            object constraints,
            object dataTokens) where TController : ControllerBase
        {
            var mvcEndpointDataSource = routeBuilder.EndpointDataSources.OfType<MvcEndpointDataSource>().FirstOrDefault();

            if (mvcEndpointDataSource == null)
            {
                mvcEndpointDataSource = routeBuilder.ServiceProvider.GetRequiredService<MvcEndpointDataSource>();
                routeBuilder.EndpointDataSources.Add(mvcEndpointDataSource);
            }

            var endpointInfo = new MvcEndpointInfo(
                name,
                template,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(constraints),
                new RouteValueDictionary(dataTokens),
                routeBuilder.ServiceProvider.GetRequiredService<ParameterPolicyFactory>());

            endpointInfo.ControllerType = typeof(TController);

            mvcEndpointDataSource.ConventionalEndpointInfos.Add(endpointInfo);

            return endpointInfo;
        }
    }
}
