// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MvcSandbox
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

            // Translations done via google translate :)
            var translations = new Dictionary<string, Translations>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "en",
                    new Translations(new (string, string)[]
                    {
                        ("Home", "Home"),
                        ("Store", "Store"),
                        ("Index", "Index"),
                        ("Details", "Details"),
                        ("ShoppingCart", "ShoppingCart"),
                        ("Checkout", "Checkout"),
                    })
                },
                {
                    "es",
                    new Translations(new (string, string)[]
                    {
                        ("Home", "Casa"),
                        ("Store", "Almacenar"),
                        ("Index", "Índice"),
                        ("Details", "Detalles"),
                        ("ShoppingCart", "CarritoDeCompras"),
                        ("Checkout", "Revisa"),
                    })
                },
                {
                    "fr",
                    new Translations(new (string, string)[]
                    {
                        ("Home", "Maison"),
                        ("Store", "Magasin"),
                        ("Index", "Indice"),
                        ("Details", "Détails"),
                        ("ShoppingCart", "Chariot"),
                        ("Checkout", "CheckOut"),
                    })
                },
            };


            app.UseMvc(routes =>
            {
                routes.Routes.Add(new TranslationRoute(
                    translations, 
                    routes.DefaultHandler, 
                    routeName: null, 
                    routeTemplate: "en/{controller=Home}/{action=Index}/{id?}",
                    defaults: new RouteValueDictionary(new { language = "en", }),
                    constraints: null,
                    dataTokens: null,
                    inlineConstraintResolver: routes.ServiceProvider.GetRequiredService<IInlineConstraintResolver>()));

                routes.Routes.Add(new TranslationRoute(
                    translations,
                    routes.DefaultHandler,
                    routeName: null,
                    routeTemplate: "es/{controller=Casa}/{action=Índice}/{id?}",
                    defaults: new RouteValueDictionary(new { language = "es", }),
                    constraints: null,
                    dataTokens: null,
                    inlineConstraintResolver: routes.ServiceProvider.GetRequiredService<IInlineConstraintResolver>()));

                routes.Routes.Add(new TranslationRoute(
                    translations,
                    routes.DefaultHandler,
                    routeName: null,
                    routeTemplate: "fr/{controller=Maison}/{action=Indice}/{id?}",
                    defaults: new RouteValueDictionary(new { language = "fr", }),
                    constraints: null,
                    dataTokens: null,
                    inlineConstraintResolver: routes.ServiceProvider.GetRequiredService<IInlineConstraintResolver>()));
            });
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
                .ConfigureLogging(factory =>
                {
                    factory
                        .AddConsole()
                        .AddDebug();
                })
                .UseIISIntegration()
                .UseKestrel()
                .UseStartup<Startup>();
    }
}

