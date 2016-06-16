namespace Microsoft.AspnetCore.Mvc.Mobile
{
    using System.Threading.Tasks;
    using Abstractions;
    using AspNetCore.Builder;
    using AspNetCore.Http;
    using AspNetCore.Mvc.Razor;
    using AspNetCore.Routing;
    using Device;
    using Device.Resolvers;
    using Extensions.DependencyInjection;
    using Extensions.DependencyInjection.Extensions;
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

        public static IServiceCollection AddDeviceDetector(this IServiceCollection services) => services.AddDeviceDetector<DefaultDeviceFactory>();

        public static IServiceCollection AddDeviceDetector<TDeviceFactory>(this IServiceCollection services) where TDeviceFactory : class, IDeviceFactory
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IDeviceFactory, TDeviceFactory>();
            services.AddTransient<IDevicePreference, CookiePreference>();
            services.AddTransient<IDevicePreference, UrlPreference>();
            services.AddTransient<IDevicePreference, CookiePreference>();

            services.AddTransient<IDeviceResolver, AgentResolver>();
            services.AddTransient<ISitePreferenceRepository, SitePreferenceRepository>();

            services.AddTransient<DeviceOptions>();
            services.AddScoped<IDeviceAccessor, DeviceAccessor>();
            services.AddTransient<DeviceViewLocationExpander>();
            services.Configure<RazorViewEngineOptions>(
                options =>
                {
                    options.ViewLocationExpanders.Add(services.BuildServiceProvider().GetService<DeviceViewLocationExpander>());
                });

            return services;
        }

        public static IServiceCollection AddDeviceSwitcher<TPreference>(this IServiceCollection services, SwitcherOptions options = null) where TPreference : IDevicePreference
        {
            services.AddDeviceDetector();
            services.AddSingleton<PreferenceSwitcher>();
            services.AddSingleton(_ => options ?? new SwitcherOptions(services.BuildServiceProvider().GetService<TPreference>()));
            return services;
        }
    }
}