// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;

namespace CustomRouteWebSite
{
    public class Startup
    {
        // Set up application services
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIISPlatformHandler();
            
            app.UseCultureReplacer();

            app.UseMvc(routes =>
            {
                routes.DefaultHandler = new LocalizedRoute(routes.DefaultHandler);
                routes.MapRoute("default", "{controller}/{action}");
            });
        }
    }
}