// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace RoutingWebSite
{
    public class Startup
    {
        // Set up application services
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddScoped<TestResponseGenerator>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();


            services.Configure<MvcEndpointDataSourceOptions>(o =>
            {
                o.Endpoints.Add(new EndpointInfo()
                {
                    Template = "{area=Travel}/{controller=Home}/{action=Index}/{id?}",
                    Name = "flightRoute"
                });
                o.Endpoints.Add(new EndpointInfo()
                {
                    Template = "{controller=Home}/{action=Index}",
                    Name = "ActionAsMethod"
                });
                o.Endpoints.Add(new EndpointInfo()
                {
                    Template = "{controller}/{action}/{path?}",
                    Name = "RouteWithOptionalSegment"
                });
            });

        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDispatcher();
            app.UseEndpoint();
        }

        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args)
                .Build();

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseKestrel()
                .UseIISIntegration();
    }
}

