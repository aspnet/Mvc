
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc.TagHelpers;
using Microsoft.Framework.DependencyInjection;

namespace TagHelperSample.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ScriptTagHelperOptions>(options =>
            {
                // Set custom options here
                //options.MinExtension = ".mini.js";
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            
            app.UseServices(services => services.AddMvc());
            app.UseMvc();
        }
    }
}
