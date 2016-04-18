// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CorsWebSite
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.Configure<CorsOptions>(options =>
            {
                options.AddPolicy(
                    "AllowAnySimpleRequest",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .WithMethods("GET", "POST", "HEAD");
                    });

                options.AddPolicy(
                    "AllowSpecificOrigin",
                    builder =>
                    {
                        builder.WithOrigins("http://example.com");
                    });

                options.AddPolicy(
                    "WithCredentials",
                    builder =>
                    {
                        builder.AllowCredentials()
                               .WithOrigins("http://example.com");
                    });

                options.AddPolicy(
                    "WithCredentialsAnyOrigin",
                    builder =>
                    {
                        builder.AllowCredentials()
                               .AllowAnyOrigin()
                               .AllowAnyHeader()
                               .WithMethods("PUT", "POST")
                               .WithExposedHeaders("exposed1", "exposed2");
                    });

                options.AddPolicy(
                    "AllowAll",
                    builder =>
                    {
                        builder.AllowCredentials()
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowAnyOrigin();
                    });

                options.AddPolicy(
                    "Allow example.com",
                    builder =>
                    {
                        builder.AllowCredentials()
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .WithOrigins("http://example.com");
                    });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCultureReplacer();

            app.UseMvc();
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseDefaultHostingConfiguration(args)
                .UseStartup<Startup>()
                .UseKestrel()
                .UseIISIntegration()
                .Build();

            host.Run();
        }
    }
}
