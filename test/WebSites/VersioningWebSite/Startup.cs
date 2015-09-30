// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;

namespace VersioningWebSite
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Add MVC services to the services container
            services.AddMvc();

            services.AddScoped<TestResponseGenerator>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIISPlatformHandler();
            
            app.UseCultureReplacer();

            app.UseMvcWithDefaultRoute();
        }
    }
}
