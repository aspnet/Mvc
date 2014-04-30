using Microsoft.AspNet;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Routing;

namespace MvcSample.Web
{
    public class Startup
    {
        public void Configuration(IBuilder app)
        {
            app.UseServices(services =>
            {
                services.AddMvc();
                services.AddSingleton<PassThroughAttribute, PassThroughAttribute>();
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute("areaRoute", "{area}/{controller}/{action}");
                routes.MapRoute("travelAreaRoute", 
                                "{area}/{controller}/{action}",
                                new { area = "Travel" });

                routes.MapRoute(
                    "controllerActionRoute",
                    "{controller}/{action}",
                    new { controller = "Home", action = "Index" });

                routes.MapRoute(
                    "controllerOnlyRoute",
                    "{controller}",
                    new { controller = "Home" });
            });
        }
    }
}
