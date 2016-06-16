namespace Microsoft.AspnetCore.Mvc.Mobile
{
    using System.Threading.Tasks;
    using AspNetCore.Builder;
    using AspNetCore.Http;
    using AspNetCore.Mvc.Razor;
    using AspNetCore.Routing;
    using Device;
    using Device.Resolvers;
    using Extensions.DependencyInjection;
    using Preference;

    public static class Configuration
    {
        public static IRouteBuilder MapDeviceSwitcher(this IRouteBuilder route, string url = "choose")
        {
            route.MapGet(url + "/{device}", route.ServiceProvider.GetService<PreferenceSwitcher>().Handle);
            return route;
        }

        public static IApplicationBuilder UseDeviceDetector(this IApplicationBuilder app)
        {
            return app;
        }

        public static IServiceCollection AddDeviceDetector(this IServiceCollection services)
        {
            services.AddScoped<IDeviceResolver, CookieDevice>();
            services.AddScoped<IDeviceResolver, CookieDevice>();
            services.AddScoped<IDeviceResolver, UrlDevice>();

            services.AddScoped<DeviceViewLocationExpander>();
            services.AddScoped<DeviceOptions>();
            services.AddScoped<IDeviceAccessor, DeviceAccessor>();
            services.AddScoped<IDeviceResolver, AgentDevice>();
            services.Configure<RazorViewEngineOptions>(
                options =>
                {
                    options.ViewLocationExpanders.Add(services.BuildServiceProvider().GetService<DeviceViewLocationExpander>());
                });

            return services;
        }
        public static IServiceCollection AddDeviceSwitcher(this IServiceCollection services)
        {
            services.AddDeviceDetector();
            services.AddScoped<IDeviceStore, CookieDevice>();
            services.AddScoped<PreferenceSwitcher>();
            services.AddScoped<SwitcherOptions>();
            services.AddScoped<ISitePreferenceRepository, SitePreferenceRepository>();
            return services;
        }
    }
}