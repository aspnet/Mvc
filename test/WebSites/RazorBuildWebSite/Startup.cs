﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace RazorBuildWebSite
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            
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
