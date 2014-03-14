﻿#if NET45
using System;
using Autofac;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.DependencyInjection.Autofac;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Routing;

namespace MvcSample.Web
{
    public class Startup
    {
        public void Configuration(IBuilder builder)
        {
            var containerBuilder = new ContainerBuilder();
            var services = MvcServices.GetDefaultServices();

            AutofacRegistration.Populate(containerBuilder, builder.ServiceProvider, services);
            containerBuilder.RegisterInstance<PassThroughAttribute>(new PassThroughAttribute());

            var serviceProvider = containerBuilder.Build().Resolve<IServiceProvider>();

            var routes = new RouteCollection()
            {
                DefaultHandler = new MvcApplication(serviceProvider),
            };

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
