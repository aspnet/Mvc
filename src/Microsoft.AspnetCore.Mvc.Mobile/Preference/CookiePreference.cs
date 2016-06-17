namespace Microsoft.AspnetCore.Mvc.Mobile.Device.Resolvers
{
    using Abstractions;
    using AspNetCore.Http;

    public class CookiePreference : IDevicePreference
    {
        private const string DevicePreferenceCookieKey = "ASP_DEVICE_PREFERENCE";
        private const string MobilePreferenceKey = "MOBILE";
        private const string TabletPreferenceKey = "TABLET";
        private const string NormalPreferenceKey = "NORMAL";

        private readonly IDeviceFactory _deviceFactory;
        private readonly IDeviceRedirector _deviceRedirector;

        public CookiePreference(IDeviceFactory deviceFactory, IDeviceRedirector deviceRedirector)
        {
            _deviceFactory = deviceFactory;
            _deviceRedirector = deviceRedirector;
        }

        public int Priority => 1;

        public IDevice LoadPreference(HttpContext context)
        {
            if (context.Request.Cookies.ContainsKey(DevicePreferenceCookieKey))
            {
                switch (context.Request.Cookies[DevicePreferenceCookieKey])
                {
                    case MobilePreferenceKey:
                        return _deviceFactory.Mobile();
                    case TabletPreferenceKey:
                        return _deviceFactory.Tablet();
                    case NormalPreferenceKey:
                        return _deviceFactory.Normal();
                }
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