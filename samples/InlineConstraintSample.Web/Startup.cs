// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using InlineConstraintSample.Web.Constraints;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Localization;
using Microsoft.AspNet.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;

namespace InlineConstraintSample.Web
{
    public class Startup
    {
        // Set up application services
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureRouting(
                routeOptions => routeOptions.ConstraintMap.Add(
                    "IsbnDigitScheme10",
                    typeof(IsbnDigitScheme10Constraint)));

            services.ConfigureRouting(
                routeOptions => routeOptions.ConstraintMap.Add(
                    "IsbnDigitScheme13",
                    typeof(IsbnDigitScheme10Constraint)));

            // Update an existing constraint from ConstraintMap for test purpose.
            services.ConfigureRouting(
                routeOptions =>
                {
                    if (routeOptions.ConstraintMap.ContainsKey("IsbnDigitScheme13"))
                    {
                        routeOptions.ConstraintMap["IsbnDigitScheme13"] =
                            typeof(IsbnDigitScheme13Constraint);
                    }
                });

            // Add MVC services to the services container
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            // Ignore ambient and client locale. Use same values as ReplaceCultureAttribute / CultureReplacerMiddleware.
            var localizationOptions = new RequestLocalizationOptions();
            localizationOptions.RequestCultureProviders.Clear();
            app.UseRequestLocalization(localizationOptions, new RequestCulture("en-GB", "en-US"));

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "isbn10",
                    template: "book/{action}/{isbnNumber:IsbnDigitScheme10(true)}",
                    defaults: new { controller = "InlineConstraint_Isbn10" });

                routes.MapRoute(
                    "StoreId",
                    "store/{action}/{id:guid?}",
                    defaults: new { controller = "InlineConstraint_Store" });

                routes.MapRoute(
                    "StoreLocation",
                    "store/{action}/{location:minlength(3):maxlength(10)}",
                    defaults: new { controller = "InlineConstraint_Store" },
                    constraints: new { location = new AlphaRouteConstraint() });

                // Used by tests for the 'exists' constraint.
                routes.MapRoute("areaExists-area", "area-exists/{area:exists}/{controller=Home}/{action=Index}");
                routes.MapRoute("areaExists", "area-exists/{controller=Home}/{action=Index}");
            });
        }
    }
}
