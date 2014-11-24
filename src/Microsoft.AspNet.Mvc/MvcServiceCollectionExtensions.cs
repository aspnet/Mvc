// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Security.DataProtection;
using Microsoft.Framework.ConfigurationModel;

namespace Microsoft.Framework.DependencyInjection
{
    public static class MvcServiceCollectionExtensions
    {
        public static IServiceCollection AddMvc(this IServiceCollection services, IConfiguration configuration = null)
        {
            ConfigureDefaultServices(services);
            services.TryAdd(MvcServices.GetDefaultServices(configuration));
            return services;
        }

        private static void ConfigureDefaultServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddDataProtection();
            services.AddRouting();
            services.AddContextAccessor();
            services.Configure<RouteOptions>(routeOptions =>
                                                    routeOptions.ConstraintMap
                                                         .Add("exists",
                                                              typeof(KnownRouteValueConstraint)));
        }
    }
}
