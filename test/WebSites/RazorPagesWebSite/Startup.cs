﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EmbeddedPagesClassLibrary;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace RazorPagesWebSite
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication().AddCookie(options => options.LoginPath = "/Login");
            services.AddMvc()
                .AddCookieTempDataProvider()
                .AddRazorOptions(options =>
                {
                    var embeddedFileProvider = new EmbeddedFileProvider(
                        typeof(EmbeddedModel).Assembly,
                        $"{nameof(EmbeddedPagesClassLibrary)}.Resources");

                    options.FileProviders.Add(embeddedFileProvider);
                })
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
            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
