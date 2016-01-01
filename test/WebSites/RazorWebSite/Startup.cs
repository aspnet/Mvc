// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Localization;
using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace RazorWebSite
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddRazorOptions(options =>
                {
                    options.ViewLocationExpanders.Add(new NonMainPageViewLocationExpander());
#if DNX451
                    options.ParseOptions = options.ParseOptions.WithPreprocessorSymbols("DNX451", "DNX451_CUSTOM_DEFINE");
#else
                    options.ParseOptions = options.ParseOptions.WithPreprocessorSymbols("DNXCORE50", "DNXCORE50_CUSTOM_DEFINE");
#endif
                })
                .AddViewOptions(options =>
                {
                    options.HtmlHelperOptions.ClientValidationEnabled = false;
                    options.HtmlHelperOptions.Html5DateRenderingMode = Microsoft.AspNet.Mvc.Rendering.Html5DateRenderingMode.Rfc3339;
                    options.HtmlHelperOptions.IdAttributeDotReplacement = "!";
                    options.HtmlHelperOptions.ValidationMessageElement = "validationMessageElement";
                    options.HtmlHelperOptions.ValidationSummaryMessageElement = "validationSummaryElement";
                })
                .AddViewLocalization(LanguageViewLocationExpanderFormat.SubFolder);

            services.AddTransient<InjectedHelper>();
            services.AddTransient<TaskReturningService>();
            services.AddTransient<FrameworkSpecificHelper>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRequestLocalization(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en-GB", "en-US");
                options.SupportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("fr"),
                    new CultureInfo("en-GB"),
                    new CultureInfo("en-US"),
                };
                options.SupportedUICultures = new List<CultureInfo>
                {
                    new CultureInfo("fr"),
                    new CultureInfo("en-GB"),
                    new CultureInfo("en-US"),
                };
            });

            app.UseMvcWithDefaultRoute();
        }

        public static void Main(string[] args)
        {
            var application = new WebApplicationBuilder()
                .UseConfiguration(WebApplicationConfiguration.GetDefault(args))
                .UseStartup<Startup>()
                .Build();

            application.Run();
        }
    }
}
