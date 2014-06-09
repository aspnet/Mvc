using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;

namespace InlineConstraints
{
    public class Startup
    {
        public Startup()
        {
            RouteCollectionProvider = AddDefaultRoutes;
        }

        public Action<IRouteBuilder> RouteCollectionProvider { get; set; }
        public void Configure(IBuilder app)
        {
            // Set up application services
            app.UseServices(services =>
            {
                // Add MVC services to the services container
                services.AddMvc();

                // Add a custom assembly provider so that we add only controllers present in 
                // this assembly.
                services.AddTransient<IControllerAssemblyProvider, TestControllerAssemblyProvider>();
            });

            // Add MVC to the request pipeline
            app.UseMvc(RouteCollectionProvider);
        }

        private void AddDefaultRoutes(IRouteBuilder routes)
        {
            routes.MapRoute("areaRoute",
                             "{area:exists}/{controller}/{action}",
                             new { controller = "Home", action = "Index" });

            routes.MapRoute("ActionAsMethod", "{controller}/{action}",
                defaults: new { controller = "Home", action = "Index" });
        }
    }
}
