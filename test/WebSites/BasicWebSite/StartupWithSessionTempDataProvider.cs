// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BasicWebSite
{
    public class StartupWithSessionTempDataProvider
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // CookieTempDataProvider is the default ITempDataProvider, so we must override it with session.
            services
                .AddMvc(options => options.EnableEndpointRouting = true)
                .AddSessionStateTempDataProvider();
            services.AddSession();

            services.ConfigureBaseWebSiteAuthPolicies();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseSession();
            app.UseMvcWithDefaultRoute();
        }
    }
}

