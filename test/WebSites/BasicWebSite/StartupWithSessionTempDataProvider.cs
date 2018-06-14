// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace BasicWebSite
{
    public class StartupWithSessionTempDataProvider
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // CookieTempDataProvider is the default ITempDataProvider, so we must override it with session.
            services
                .AddMvc()
                .AddSessionStateTempDataProvider();
            services.AddSession();

            services.ConfigureBaseWebSiteAuthPolicies();

            services.Configure<MvcEndpointDataSourceOptions>(o =>
            {
                o.Endpoints.Add(new EndpointInfo()
                {
                    Template = "{controller=Home}/{action=Index}/{id?}",
                    Name = "default"
                });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDispatcher();
            app.UseDeveloperExceptionPage();
            app.UseSession();
            app.UseEndpoint();
        }
    }
}

