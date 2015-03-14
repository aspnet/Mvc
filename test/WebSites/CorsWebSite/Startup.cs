// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.DependencyInjection;

namespace CorsWebSite
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            var configuration = app.GetTestConfiguration();

            app.UseServices(services =>
            {
                services.AddMvc();
                services.Configure<MvcOptions>(options =>
                {
                    options.AddXmlDataContractSerializerFormatter();
                });

                services.ConfigureCors(options =>
                {
                    options.AddPolicy(
                        "AllowAnySimpleRequest",
                        builder =>
                        {
                            builder.AllowAnyOrigin()
                                   .AddMethods("GET", "POST", "HEAD");
                        });

                    options.AddPolicy(
                        "AllowSpecificOrigin",
                        builder =>
                        {
                            builder.AddOrigins("http://example.com");
                        });

                    options.AddPolicy(
                        "WithCredentials",
                        builder =>
                        {
                            builder.AllowCredentials()
                                   .AddOrigins("http://example.com");
                        });

                    options.AddPolicy(
                        "WithCredentialsAnyOrigin",
                        builder =>
                        {
                            builder.AllowCredentials()
                                   .AllowAnyOrigin()
                                   .AllowAnyHeader()
                                   .AddMethods("PUT", "POST")
                                   .AddExposedHeaders("exposed1", "exposed2");
                        });
                });
            });

            app.UseErrorReporter();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}");
            });
        }
    }
}