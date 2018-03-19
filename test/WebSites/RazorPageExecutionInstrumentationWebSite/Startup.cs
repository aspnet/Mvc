// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.DependencyInjection;

namespace RazorPageExecutionInstrumentationWebSite
{
    public class Startup
    {
        // Set up application services
        public void ConfigureServices(IServiceCollection services)
        {
            // Normalize line endings to avoid changes in instrumentation locations between systems.
            services.AddTransient<RazorProjectFileSystem, TestRazorProjectFileSystem>();

            // Add MVC services to the services container.
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            var listener = new RazorPageDiagnosticListener();
            var diagnosticSource = app.ApplicationServices.GetRequiredService<DiagnosticListener>();
            diagnosticSource.SubscribeWithAdapter(listener);

            app.Use(async (context, next) =>
            {
                using (var writer = new StreamWriter(context.Response.Body))
                {
                    context.Items[RazorPageDiagnosticListener.WriterKey] = writer;
                    context.Response.Body = Stream.Null;
                    await next();
                }
            });

            // Add MVC to the request pipeline
            app.UseMvcWithDefaultRoute();
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

