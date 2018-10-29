// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapApplication(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var factories = builder.ServiceProvider.GetServices<EndpointDataSourceFactory>();
            var dataSources = factories.Select(f => f.GetOrCreateDataSource(builder)).Where(d => d != null).ToList();

            // Add application assemblies

            return new ApplicationConventionBuilder(dataSources);
        }

        public static AssemblyConventionBuilder MapAssembly(this IEndpointRouteBuilder builder, Assembly assembly)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var factories = builder.ServiceProvider.GetServices<EndpointDataSourceFactory>();
            var dataSources = factories.Select(f => f.GetOrCreateDataSource(builder)).Where(d => d != null).ToList();
            var builder = new ApplicationConventionBuilder(dataSources);
        }

        public static AssemblyConventionBuilder MapAssemblyOfType<T>(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var dataSource = GetOrCreateControllerDataSource(builder);
            return dataSource.Application.GetOrCreateAssembly(typeof(T).Assembly);
        }

        public static ControllerConventionBuilder MapController<TController>(this IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var dataSource = GetOrCreateControllerDataSource(builder);
            return dataSource.Application.GetOrCreateAssembly(typeof(TController).Assembly).GetOrCreateController(typeof(TController));
        }

        public static ControllerConventionBuilder MapController(this IEndpointRouteBuilder builder, Type controllerType)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var dataSource = GetOrCreateControllerDataSource(builder);
            return dataSource.Application.GetOrCreateAssembly(controllerType.Assembly).GetOrCreateController(controllerType);
        }

        private static ControllerDataSource GetOrCreateControllerDataSource(IEndpointRouteBuilder builder)
        {
            var dataSource = builder.DataSources.OfType<ControllerDataSource>().FirstOrDefault();
            if (dataSource == null)
            {
                dataSource = builder.ServiceProvider.GetRequiredService<ControllerDataSource>();
                builder.DataSources.Add(dataSource);
            }

            return dataSource;
        }
    }
}
