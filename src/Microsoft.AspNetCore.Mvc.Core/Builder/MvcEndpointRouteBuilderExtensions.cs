// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class MvcEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapApplication(
            this IEndpointRouteBuilder routeBuilder)
        {
            return MapController(routeBuilder);
        }

        public static IEndpointConventionBuilder MapApplication<TAssembly>(
            this IEndpointRouteBuilder routeBuilder)
        {
            return MapController(routeBuilder).ForAssemblyType(typeof(TAssembly));
        }

        public static IControllerEndpointConventionBuilder MapController(
            this IEndpointRouteBuilder routeBuilder)
        {
            var mvcEndpointDataSource = routeBuilder.DataSources.OfType<MvcEndpointDataSource>().FirstOrDefault();

            if (mvcEndpointDataSource == null)
            {
                mvcEndpointDataSource = routeBuilder.ServiceProvider.GetRequiredService<MvcEndpointDataSource>();
                routeBuilder.DataSources.Add(mvcEndpointDataSource);
            }

            var conventionBuilder = new DefaultControllerEndpointConventionBuilder();

            mvcEndpointDataSource.AttributeRoutingConventionResolvers.Add(actionDescriptor =>
            {
                // TODO: Filtering for controllers by TController
                // TODO: Filtering for Razor pages by path
                // - NOTE: Razor types are in another assembly
                // TODO: Filtering for other types of action descriptors
                return conventionBuilder;
            });

            return conventionBuilder;
        }

        public static IControllerEndpointConventionBuilder MapController(
            this IEndpointRouteBuilder routeBuilder,
            Type assemblyType,
            Type controllerType)
        {
            var mvcEndpointDataSource = routeBuilder.DataSources.OfType<MvcEndpointDataSource>().FirstOrDefault();

            if (mvcEndpointDataSource == null)
            {
                mvcEndpointDataSource = routeBuilder.ServiceProvider.GetRequiredService<MvcEndpointDataSource>();
                routeBuilder.DataSources.Add(mvcEndpointDataSource);
            }

            var conventionBuilder = new DefaultControllerEndpointConventionBuilder();

            mvcEndpointDataSource.AttributeRoutingConventionResolvers.Add(actionDescriptor =>
            {
                // TODO: Filtering for controllers by TController
                // TODO: Filtering for Razor pages by path
                // - NOTE: Razor types are in another assembly
                // TODO: Filtering for other types of action descriptors
                return conventionBuilder;
            });

            return conventionBuilder;
        }

        public static IControllerEndpointConventionBuilder MapControllerRoute(
            this IEndpointRouteBuilder routeBuilder,
            string name,
            string template)
        {
            return MapControllerRoute(routeBuilder, name, template, defaults: null);
        }

        public static IControllerEndpointConventionBuilder MapControllerRoute(
            this IEndpointRouteBuilder routeBuilder,
            string name,
            string template,
            object defaults)
        {
            return MapControllerRoute(routeBuilder, name, template, defaults, constraints: null);
        }

        public static IControllerEndpointConventionBuilder MapControllerRoute(
            this IEndpointRouteBuilder routeBuilder,
            string name,
            string template,
            object defaults,
            object constraints)
        {
            return MapControllerRoute(routeBuilder, name, template, defaults, constraints, dataTokens: null);
        }

        public static IControllerEndpointConventionBuilder MapControllerRoute(
            this IEndpointRouteBuilder routeBuilder,
            string name,
            string template,
            object defaults,
            object constraints,
            object dataTokens)
        {
            var mvcEndpointDataSource = routeBuilder.DataSources.OfType<MvcEndpointDataSource>().FirstOrDefault();

            if (mvcEndpointDataSource == null)
            {
                mvcEndpointDataSource = routeBuilder.ServiceProvider.GetRequiredService<MvcEndpointDataSource>();
                routeBuilder.DataSources.Add(mvcEndpointDataSource);
            }

            var endpointInfo = new MvcEndpointInfo(
                name,
                template,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(constraints),
                new RouteValueDictionary(dataTokens),
                routeBuilder.ServiceProvider.GetRequiredService<ParameterPolicyFactory>());

            mvcEndpointDataSource.ConventionalEndpointInfos.Add(endpointInfo);

            return endpointInfo;
        }
    }
}
