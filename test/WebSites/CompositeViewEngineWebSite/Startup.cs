// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;

namespace CompositeViewEngineWebSite
{
    public class Startup
    {
        // Set up application services
        public void ConfigureServices(IServiceCollection services)
        {
            // Add a view engine as the first one in the list.
            services
                .AddMvc()
                .AddViewOptions(options =>
                {
                    options.ViewEngines.Insert(0, new TestViewEngine());
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIISPlatformHandler();
            app.UseCultureReplacer();

            // Add MVC to the request pipeline
            app.UseMvcWithDefaultRoute();
        }
    }
}
