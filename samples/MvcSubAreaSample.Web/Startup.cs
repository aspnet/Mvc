// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace MvcSubAreaSample.Web
{
    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new SubAreaViewLocationExpander());
            });

            services.AddMvc();

            return services.BuildServiceProvider();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app)
        {
            // Add MVC to the request pipeline.
            app.UseFileServer();
            app.UseMvc(routes =>
            {
                routes.MapRoute("subarearoute", "{area:exists}/{subarea:exists}/{controller=Home}/{action=Index}");
                routes.MapRoute("areaRoute", "{area:exists}/{controller=Home}/{action=Index}");
                routes.MapRoute("controllerActionRoute", "{controller=Home}/{action=Index}");
            });
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseDefaultConfiguration(args)
                .UseIISPlatformHandlerUrl()
                .UseServer("Microsoft.AspNetCore.Server.Kestrel")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
