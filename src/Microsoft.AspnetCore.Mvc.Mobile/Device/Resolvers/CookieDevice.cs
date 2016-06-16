namespace Microsoft.AspnetCore.Mvc.Mobile.Device.Resolvers
{
    using AspNetCore.Http;

    public class CookieDevice : IDeviceResolver, IDeviceStore
    {
        private const string DevicePreferenceCookieKey = "ASP_DEVICE_PREFERENCE";
        private const string MobilePreferenceKey = "MOBILE";
        private const string TabletPreferenceKey = "TABLET";
        private const string NormalPreferenceKey = "NORMAL";

        private readonly DeviceOptions _options;

        public CookieDevice(DeviceOptions options)
        {
            _options = options;
        }

        public int Priority => 1;

        public IDevice ResolveDevice(HttpContext context)
        {
            if (context.Request.Cookies.ContainsKey(DevicePreferenceCookieKey))
            {
                switch (context.Request.Cookies[DevicePreferenceCookieKey])
                {
                    case MobilePreferenceKey:
                        return new LiteDevice(DeviceType.Mobile, _options.MobileCode);
                    case TabletPreferenceKey:
                        return new LiteDevice(DeviceType.Mobile, _options.TabletCode);
                    case NormalPreferenceKey:
                        return new LiteDevice(DeviceType.Normal);
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
        }

        public void ResetStore(HttpContext context)
        {
            context.Response.Cookies.Delete(DevicePreferenceCookieKey);
        }
    }
}