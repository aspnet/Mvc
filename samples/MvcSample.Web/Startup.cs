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

                routes.MapRoute(
                    "namedRoute",
                    "{controller}/{action}",
                    new { controller = "Home", action = "Index" });

                routes.MapRoute(
                    "controllerRoute",
                    "{controller}",
                    new { controller = "Home" });
            });
        }
    }
}
