namespace Microsoft.AspnetCore.Mvc.Mobile.Preference
{
    using Abstractions;
    using AspNetCore.Http;

    public class CookieSwitcher : IDeviceSwitcher
    {
        private const string DevicePreferenceCookieKey = ".Aspnet.Device.Preference";
        private const string MobilePreferenceKey = "Mobile";
        private const string TabletPreferenceKey = "Tablet";
        private const string NormalPreferenceKey = "Normal";

        private readonly IDeviceFactory _deviceFactory;
        private readonly IDeviceRedirector _deviceRedirector;

        public CookieSwitcher(IDeviceFactory deviceFactory, IDeviceRedirector deviceRedirector)
        {
            _deviceFactory = deviceFactory;
            _deviceRedirector = deviceRedirector;
        }

        public int Priority => 1;

        public IDevice LoadPreference(HttpContext context)
        {
            if (!context.Request.Cookies.ContainsKey(DevicePreferenceCookieKey)) return null;

            switch (context.Request.Cookies[DevicePreferenceCookieKey])
            {
                case MobilePreferenceKey:
                    return _deviceFactory.Mobile();
                case TabletPreferenceKey:
                    return _deviceFactory.Tablet();
                case NormalPreferenceKey:
                    return _deviceFactory.Normal();
            }

            return null;
        }

        public void StoreDevice(HttpContext context, IDevice device)
        {
            if (device.IsMobile)
            {
                context.Response.Cookies.Append(DevicePreferenceCookieKey, MobilePreferenceKey);
            }
            else if (device.IsTablet)
            {
                context.Response.Cookies.Append(DevicePreferenceCookieKey, TabletPreferenceKey);
            }
            else if (device.IsNormal)
            {
                context.Response.Cookies.Append(DevicePreferenceCookieKey, NormalPreferenceKey);
            }

            _deviceRedirector.RedirectToDevice(context);
        }

        public void ResetStore(HttpContext context)
        {
            context.Response.Cookies.Delete(DevicePreferenceCookieKey);

            _deviceRedirector.RedirectToDevice(context);
        }
    }
}