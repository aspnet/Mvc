// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MvcSandbox
{
    using Microsoft.AspnetCore.Mvc.Mobile;
    using Microsoft.AspnetCore.Mvc.Mobile.Abstractions;
    using Microsoft.AspnetCore.Mvc.Mobile.Preference;

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddTransient<ISitePreferenceRepository, SitePreferenceRepository>();
            services.AddDeviceSwitcher<CookiePreference>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            loggerFactory.AddConsole();
            app.UseMvc(routes =>
            {
                routes.MapDeviceSwitcher();
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "mobile_default",
                    template: "m/{controller=Home}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "tablet_default",
                    template: "t/{controller=Home}/{action=Index}/{id?}");
            });
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}

