// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DispatchingWebSite
{
    public class Startup
    {
        private static readonly byte[] _homePayload = Encoding.UTF8.GetBytes("Dispatcher sample endpoints:" + Environment.NewLine + "/plaintext");

        // Set up application services
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDispatcher(options =>
            {
                options.DataSources.Add(new DefaultEndpointDataSource(new[]
                {
                    new MatcherEndpoint(
                        (next) => (httpContext) =>
                        {
                            var response = httpContext.Response;
                            var payloadLength = _homePayload.Length;
                            response.StatusCode = 200;
                            response.ContentType = "text/plain";
                            response.ContentLength = payloadLength;
                            return response.Body.WriteAsync(_homePayload, 0, payloadLength);
                        },
                        "/",
                        new { controller = "Home", action = "Index" },
                        0,
                        EndpointMetadataCollection.Empty,
                        "ActionAsMethod"),
                    new MatcherEndpoint(
                        (next) => (httpContext) =>
                        {
                            var response = httpContext.Response;
                            var payloadLength = _homePayload.Length;
                            response.StatusCode = 200;
                            response.ContentType = "text/plain";
                            response.ContentLength = payloadLength;
                            return response.Body.WriteAsync(_homePayload, 0, payloadLength);
                        },
                        "{controller}/{action}",
                        new { controller = "Home", action = "Index" },
                        0,
                        EndpointMetadataCollection.Empty,
                        "ActionAsMethod"),
                }));
            });

            services.AddMvc();

            services.AddScoped<TestResponseGenerator>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<EndpointDataSource, MvcEndpointDataSource>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDispatcher();

            //app.UseMvc(routes =>
            //{
            //    routes.MapAreaRoute(
            //       "flightRoute",
            //       "adminRoute",
            //       "{area:exists}/{controller}/{action}",
            //       new { controller = "Home", action = "Index" },
            //       new { area = "Travel" });

            //    routes.MapRoute(
            //        "ActionAsMethod",
            //        "{controller}/{action}",
            //        defaults: new { controller = "Home", action = "Index" });

            //    routes.MapRoute(
            //        "RouteWithOptionalSegment",
            //        "{controller}/{action}/{path?}");
            //});

            app.UseEndpoint();
        }

        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole();
                })
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

