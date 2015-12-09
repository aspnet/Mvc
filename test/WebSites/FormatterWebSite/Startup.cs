// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace FormatterWebSite
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.ModelMetadataDetailsProviders.Add(new ValidationExcludeFilter(typeof(Developer)));
                options.ModelMetadataDetailsProviders.Add(new ValidationExcludeFilter(typeof(Supplier)));
                
                options.InputFormatters.Add(new StringInputFormatter());
            })
            .AddXmlDataContractSerializerFormatters();
        }


        public void Configure(IApplicationBuilder app)
        {
            app.UseCultureReplacer();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute("ActionAsMethod", "{controller}/{action}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
