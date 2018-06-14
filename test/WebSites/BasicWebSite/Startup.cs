// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace BasicWebSite
{
    public class Startup
    {
        // Set up application services
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Api", _ => { });
            services.AddTransient<IAuthorizationHandler, ManagerHandler>();

            services
                .AddMvc(options =>
                {
                    options.Conventions.Add(new ApplicationDescription("This is a basic website."));
                    // Filter that records a value in HttpContext.Items
                    options.Filters.Add(new TraceResourceFilter());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddXmlDataContractSerializerFormatters();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                var previous = options.InvalidModelStateResponseFactory;
                options.InvalidModelStateResponseFactory = context =>
                {
                    var result = (BadRequestObjectResult) previous(context);
                    if (context.ActionDescriptor.FilterDescriptors.Any(f => f.Filter is VndErrorAttribute))
                    {
                        result.ContentTypes.Clear();
                        result.ContentTypes.Add("application/vnd.error+json");
                    }

                    return result;
                };
            });

            services.ConfigureBaseWebSiteAuthPolicies();

            services.AddTransient<IAuthorizationHandler, ManagerHandler>();

            services.AddLogging();
            services.AddSingleton<IActionDescriptorProvider, ActionDescriptorCreationCounter>();
            services.AddHttpContextAccessor();
            services.AddSingleton<ContactsRepository>();
            services.AddScoped<RequestIdService>();
            services.AddTransient<ServiceActionFilter>();

            services.Configure<MvcEndpointDataSourceOptions>(o =>
            {
                o.Endpoints.Add(new EndpointInfo()
                {
                    Template = "{area}/{controller=Home}/{action=Index}",
                    Name = "areaRoute"
                });
                o.Endpoints.Add(new EndpointInfo()
                {
                    Template = "{controller=Home}/{action=Index}",
                    Name = "ActionAsMethod"
                });
                o.Endpoints.Add(new EndpointInfo()
                {
                    Template = "{controller}/{action}/{page}",
                    Name = "PageRoute"
                });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDispatcher();

            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            // Initializes the RequestId service for each request
            app.UseMiddleware<RequestIdMiddleware>();

            app.UseEndpoint();
        }
    }
}
