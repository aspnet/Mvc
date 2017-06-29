﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace RazorPagesWebSite
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddCookieAuthentication(options => options.LoginPath = "/Login")
                .AddMvc()
                .AddCookieTempDataProvider()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizePage("/HelloWorldWithAuth");
                    options.Conventions.AuthorizeFolder("/Pages/Admin");
                    options.Conventions.AllowAnonymousToPage("/Pages/Admin/Login");
                    options.Conventions.AddPageRoute("/HelloWorldWithRoute", "Different-Route/{text}");
                    options.Conventions.AddPageRoute("/Pages/NotTheRoot", string.Empty);
                })
                .WithRazorPagesAtContentRoot();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCultureReplacer();

            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
