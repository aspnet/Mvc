// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationModelWebSite
{
    public class Startup
    {
        // Set up application services
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Conventions.Add(new ApplicationDescription("Common Application Description"));
                options.Conventions.Add(new ControllerLicenseConvention());
                options.Conventions.Add(new FromHeaderConvention());
                options.Conventions.Add(new MultipleAreasControllerConvention());
                options.Conventions.Add(new CloneActionConvention());
            });

            services.Configure<MvcEndpointDataSourceOptions>(o =>
            {
                o.Endpoints.Add(new EndpointInfo()
                {
                    Template = "{area}/{controller=Home}/{action=Index}",
                    Name = "areaRoute"
                });
                o.Endpoints.Add(new EndpointInfo()
                {
                    Template = "{controller}/{action}/{id?}",
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

