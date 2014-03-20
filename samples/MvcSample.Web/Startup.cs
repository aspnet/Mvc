using System;
using System.Collections.Generic;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.RequestContainer;
using Microsoft.AspNet.Routing;

namespace MvcSample.Web
{
    public class Startup
    {
        public void Configuration(IBuilder builder)
        {
            // This is a temporary change until we figure out a way to replace services already configured.
            var services = CreateServices();
            var serviceProvider = services.BuildServiceProvider(builder.ServiceProvider);
            ConfigurationCore(builder, serviceProvider);
        }

        public ServiceCollection CreateServices()
        {
            var services = new ServiceCollection();
            services.Add(MvcServices.GetDefaultServices());
            services.AddSingleton<PassThroughAttribute, PassThroughAttribute>();
            return services;
        }

        public void ConfigurationCore(IBuilder builder, IServiceProvider serviceProvider)
        {
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

            builder.UseContainer(serviceProvider);
            builder.UseRouter(routes);
        }
    }
}
