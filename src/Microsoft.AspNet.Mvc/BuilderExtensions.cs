// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Builder
{
    public static class BuilderExtensions
    {
        public static IBuilder UseMvc([NotNull] this IBuilder app)
        {
            return app.UseMvc(routes =>
            {
                // Action style actions
                routes.MapRoute(null, "{controller}/{action}/{id?}", new { controller = "Home", action = "Index" });

                // Rest style actions
                routes.MapRoute(null, "{controller}/{id?}");
            });
        }

        public static IBuilder UseMvc([NotNull] this IBuilder app, [NotNull] Action<IRouteBuilder> configureRoutes)
        {
            // Verify if AddMvc was done before calling UseMvc
            // Try to get 2 sample services. If it returns null then AddMvc was not called.
            if(app.ApplicationServices.GetServiceOrNull(typeof(IActionDescriptorsCollectionProvider)) == null ||
                app.ApplicationServices.GetServiceOrNull(typeof(IInlineConstraintResolver)) == null)
            {
                throw new InvalidOperationException(Resources.UnableToFindServices);
            }

            var routes = new RouteBuilder
            {
                DefaultHandler = new MvcRouteHandler(),
                ServiceProvider = app.ApplicationServices
            };

            routes.Routes.Add(AttributeRouting.CreateAttributeMegaRoute(
                routes.DefaultHandler, 
                app.ApplicationServices));

            configureRoutes(routes);

            return app.UseRouter(routes.Build());
        }
    }
}