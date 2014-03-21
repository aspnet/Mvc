﻿using System.Collections;

#if NET45
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Routing;

namespace MvcSample.Web
{
    public class Startup
    {
        public void Configuration(IBuilder builder)
        {
            var services = new ServiceCollection();
            services.Add(MvcServices.GetDefaultServices());
            services.AddSingleton<PassThroughAttribute, PassThroughAttribute>();

            var serviceProvider = services.BuildServiceProvider(builder.ServiceProvider);

            var routes = new RouteCollection()
            {
                DefaultHandler = new MvcApplication(serviceProvider),
            };

            // TODO: Add support for route constraints, so we can potentially constrain by existing routes
            routes.MapRoute("{area}/{controller}/{action}");

            routes.MapRoute(
                "{controller}/{action}",
                new { controller = "Home", action = "Index" });

            routes.MapRoute(
                "{controller}",
                new { controller = "Home" });

            builder.UseRouter(routes);
        }
    }
}
#endif
