// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace FormatterWebSite
{
    public class StartupWithRespectBrowserAcceptHeader
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.RespectBrowserAcceptHeader = true;
            });


            services.Configure<MvcEndpointDataSourceOptions>(o =>
            {
                o.Endpoints.Add(new EndpointInfo()
                {
                    Template = "{controller=Home}/{action=Index}",
                    Name = "default"
                });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDispatcher();
            app.UseEndpoint();
        }
    }
}

