// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.DependencyInjection;

namespace RazorExtensionWebSite
{
    public class Startup
    {
        public void Configure(IBuilder app)
        {
            var configuration = app.GetTestConfiguration();

            // Set up application services
            app.UseServices(services =>
            {
                // Add MVC services to the services container
                services.AddMvc(configuration)
                        .SetupOptions<MvcOptions>(options =>
                        {
                            options.ViewEngineOptions.ViewExtension = ".razor";
                        });
            });

            // Add MVC to the request pipeline
            app.UseMvc();
        }
    }
}
