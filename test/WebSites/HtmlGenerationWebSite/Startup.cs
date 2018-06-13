// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace HtmlGenerationWebSite
{
    public class Startup
    {
        // Set up application services
        public void ConfigureServices(IServiceCollection services)
        {
            // Add MVC services to the services container. Change default FormTagHelper.AntiForgery to false. Usually
            // null which is interpreted as true unless element includes an action attribute.
            services.AddMvc().InitializeTagHelper<FormTagHelper>((helper, _) => helper.Antiforgery = false);

            services.AddSingleton(typeof(ISignalTokenProviderService<>), typeof(SignalTokenProviderService<>));
            services.AddSingleton<ProductsService>();


            services.Configure<MvcEndpointDataSourceOptions>(o =>
            {
                o.Endpoints.Add(new EndpointInfo()
                {
                    Template = "{area}/{controller}/{action=Index}/{id?}",
                    Name = "areaRoute"
                });
                o.Endpoints.Add(new EndpointInfo()
                {
                    Template = "{controller=Product}/{action}",
                    Name = "productRoute"
                });
                o.Endpoints.Add(new EndpointInfo()
                {
                    Template = "{controller=HtmlGeneration_Home}/{action=Index}/{id?}",
                    Name = "default"
                });
            });

        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDispatcher();
            app.UseStaticFiles();
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
