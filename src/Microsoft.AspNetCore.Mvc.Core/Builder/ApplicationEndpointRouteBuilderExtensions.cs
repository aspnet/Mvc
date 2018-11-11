// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationEndpointRouteBuilderExtensions
    {
        public static ApplicationEndpointConventionBuilder MapApplication(this IEndpointRouteBuilder routes)
        {
            if (routes == null)
            {
                throw new ArgumentNullException(nameof(routes));
            }

            var assemblyProvider = routes.ServiceProvider.GetRequiredService<ApplicationAssemblyProvider>();

            var dataSources = new List<EndpointDataSource>();
            var assemblies = assemblyProvider.GetApplicationAssemblies();
            var factories = routes.ServiceProvider.GetServices<IApplicationAssemblyDataSourceFactory>();
            foreach (var factory in factories)
            {
                for (var i = 0; i < assemblies.Count; i++)
                {
                    var dataSource = factory.GetOrCreateDataSource(routes.DataSources, assemblies[i]);
                    if (dataSource != null)
                    {
                        dataSources.Add(dataSource);
                    }
                }
            }

            return new ApplicationEndpointConventionBuilder(dataSources);
        }
    }
}
