// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class AssemblyEndpointRouteBuilderExtensions
    {
        public static AssemblyEndpointConventionBuilder MapAssembly(this IEndpointRouteBuilder routes, Assembly assembly)
        {
            if (routes == null)
            {
                throw new ArgumentNullException(nameof(routes));
            }

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return MapAssemblyCore(routes, assembly);
        }

        public static AssemblyEndpointConventionBuilder MapAssemblyContaining<T>(this IEndpointRouteBuilder routes)
        {
            if (routes == null)
            {
                throw new ArgumentNullException(nameof(routes));
            }

            return MapAssemblyCore(routes, typeof(T).Assembly);
        }

        private static AssemblyEndpointConventionBuilder MapAssemblyCore(IEndpointRouteBuilder routes, Assembly assembly)
        {
            var dataSources = new List<EndpointDataSource>();
            var factories = routes.ServiceProvider.GetServices<IApplicationAssemblyDataSourceFactory>();
            foreach (var factory in factories)
            {
                var dataSource = factory.GetOrCreateDataSource(routes.DataSources, assembly);
                if (dataSource != null)
                {
                    dataSources.Add(dataSource);
                }
            }

            return new AssemblyEndpointConventionBuilder(dataSources);
        }
    }
}
