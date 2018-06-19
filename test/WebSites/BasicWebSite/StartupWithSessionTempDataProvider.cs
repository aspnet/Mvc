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
            services.AddDispatcher();
            // CookieTempDataProvider is the default ITempDataProvider, so we must override it with session.
            services
                .AddMvc()
                .AddSessionStateTempDataProvider();
            services.AddSession();

            services.ConfigureBaseWebSiteAuthPolicies();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDispatcher();
            app.UseDeveloperExceptionPage();
            app.UseSession();
            app.UseMvcWithEndpoint(r =>
            {
                r.MapEndpoint("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

