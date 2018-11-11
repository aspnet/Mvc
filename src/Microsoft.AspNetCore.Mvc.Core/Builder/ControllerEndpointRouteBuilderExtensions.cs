// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder
{
    public static class ControllerRouteBuilderExtensions
    {
        public static ControllerEndpointConventionBuilder Controllers(this IEndpointRouteBuilder routes)
        {
            if (routes == null)
            {
                throw new ArgumentNullException(nameof(routes));
            }

            var dataSources = new List<ControllerEndpointDataSource>();
            foreach (var dataSource in routes.DataSources)
            {
                if (dataSource is ControllerEndpointDataSource controllerDataSource)
                {
                    dataSources.Add(controllerDataSource);
                }
            }

            return new ControllerEndpointConventionBuilder(dataSources);
        }

        public static ControllerEndpointConventionBuilder MapControllerRoute(
            this IEndpointRouteBuilder routes,
            string name,
            string template,
            object defaults = null,
            object constraints = null,
            object dataTokens = null)
        {
            if (routes == null)
            {
                throw new ArgumentNullException(nameof(routes));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            return routes.Controllers().MapControllerRoute(name, template, defaults, constraints, dataTokens);
        }
    }
}
