// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApiExplorerWebSite
{
    public class Startup
    {
        // Set up application services
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ILoggerFactory, LoggerFactory>();
            services.AddMvc(options =>
            {
                options.Filters.AddService(typeof(ApiExplorerDataFilter));

                options.Conventions.Add(new ApiExplorerVisibilityEnabledConvention());
                options.Conventions.Add(new ApiExplorerVisibilityDisabledConvention(
                    typeof(ApiExplorerVisbilityDisabledByConventionController)));

                JsonOutputFormatter jsonOutputFormatter = null;
                for (var i = 0; i < options.OutputFormatters.Count; i++)
                {
                    var formatter = options.OutputFormatters[i];
                    jsonOutputFormatter = formatter as JsonOutputFormatter;
                    if (jsonOutputFormatter != null)
                    {
                        break;
                    }
                }

                options.OutputFormatters.Clear();
                options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                if (jsonOutputFormatter != null)
                {
                    options.OutputFormatters.Add(jsonOutputFormatter);
                }
            });

            services.AddSingleton<ApiExplorerDataFilter>();
        }


        public void Configure(IApplicationBuilder app)
        {
            app.UseCultureReplacer();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller}/{action}");
            });
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseDefaultHostingConfiguration(args)
                .UseKestrel()
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}

