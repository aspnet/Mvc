namespace Microsoft.AspnetCore.Mvc.Mobile
{
    using System;
    using System.Threading.Tasks;
    using Abstractions;
    using AspNetCore.Builder;
    using AspNetCore.Http;
    using AspNetCore.Mvc.Razor;
    using AspNetCore.Routing;
    using Device;
    using Extensions.DependencyInjection;
    using Extensions.DependencyInjection.Extensions;
    using Extensions.Options;
    using Preference;

    public static class Configuration
    {
        public static IRouteBuilder MapDeviceSwitcher(this IRouteBuilder route)
        {
            route.MapGet(
                route.ServiceProvider.GetService<IOptions<SwitcherOptions>>().Value.SwithUrl + "/{device}",
                route.ServiceProvider.GetService<PreferenceSwitcher>().Handle);
            return route;
        }

        public static IApplicationBuilder UseDeviceDetector(this IApplicationBuilder app)
        {
            return app;
        }

        public static IServiceCollection AddDeviceDetector(this IServiceCollection services,
            Action<DeviceOptions> device = null)
            => services.AddDeviceDetector<DefaultDeviceFactory>(device);

        public static IServiceCollection AddDeviceDetector<TDeviceFactory>(this IServiceCollection services,
            Action<DeviceOptions> device = null)
            where TDeviceFactory : class, IDeviceFactory
        {
            services.AddOptions();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddTransient<IDeviceFactory, TDeviceFactory>();
            services.AddTransient<IDevicePreference, CookiePreference>();
            services.AddTransient<IDevicePreference, UrlPreference>();

            services.TryAddTransient<IDeviceResolver, AgentResolver>();
            services.TryAddTransient<ISitePreferenceRepository, SitePreferenceRepository>();

            services.TryAddTransient<IDeviceAccessor, DeviceAccessor>();
            services.TryAddTransient<DeviceViewLocationExpander>();

            services.Configure(device ?? (options => { }));
            services.Configure<RazorViewEngineOptions>(
                options =>
                {
                    options.ViewLocationExpanders.Add(
                        services.BuildServiceProvider().GetService<DeviceViewLocationExpander>());
                });

            return services;
        }

        public static IServiceCollection AddDeviceSwitcher(
            this IServiceCollection services,
            Action<SwitcherOptions> switcher = null,
            Action<DeviceOptions> device = null)
            => services.AddDeviceSwitcher<CookiePreference>(switcher, device);

        public static IServiceCollection AddDeviceSwitcher<TPreference>(
            this IServiceCollection services,
            Action<SwitcherOptions> switcher = null,
            Action<DeviceOptions> device = null)
            where TPreference : class, IDevicePreference
            => services.AddDeviceSwitcher<TPreference, DeviceRedirector>(switcher, device);

        public static IServiceCollection AddDeviceSwitcher<TPreference, TRedirector>(
            this IServiceCollection services,
            Action<SwitcherOptions> switcher = null,
            Action<DeviceOptions> device = null)
            where TPreference : class, IDevicePreference
            where TRedirector : class, IDeviceRedirector
            => services.AddDeviceSwitcher<TPreference, TRedirector, DefaultDeviceFactory>(switcher, device);


        public static IServiceCollection AddDeviceSwitcher<TPreference, TRedirector, TDeviceFactory>(
            this IServiceCollection services,
            Action<SwitcherOptions> switcher = null,
            Action<DeviceOptions> device = null)
            where TPreference : class, IDevicePreference
            where TRedirector : class, IDeviceRedirector
            where TDeviceFactory : class, IDeviceFactory
        {
            services.AddDeviceDetector<TDeviceFactory>(device);
            services.TryAddTransient<IDeviceRedirector, TRedirector>();
            services.TryAddTransient<TPreference>();
            services.TryAddTransient<PreferenceSwitcher>();
            services.Configure(switcher ?? (options =>
            {
                options.Preference = services.BuildServiceProvider().GetRequiredService<TPreference>();
            }));

            return services;
        }
    }
}