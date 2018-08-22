﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BasicWebSite
{
    public class StartupWithCustomInvalidModelStateFactory
    {
        // Set up application services
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Api", _ => { });

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.Configure<ApiBehaviorOptions>(options =>
            {
                var previous = options.InvalidModelStateResponseFactory;
                options.InvalidModelStateResponseFactory = context =>
                {
                    var result = (BadRequestObjectResult)previous(context);
                    if (context.ActionDescriptor.FilterDescriptors.Any(f => f.Filter is VndErrorAttribute))
                    {
                        result.ContentTypes.Clear();
                        result.ContentTypes.Add("application/vnd.error+json");
                    }

                    return result;
                };
            });

            services.ConfigureBaseWebSiteAuthPolicies();

            services.AddLogging();
            services.AddSingleton<ContactsRepository>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseMvc();
        }
    }
}
