namespace Microsoft.AspnetCore.Mvc.Mobile.Device.Resolvers
{
    using Abstractions;
    using AspNetCore.Http;
    using AspNetCore.Http.Extensions;

    public class UrlPreference : IDevicePreference
    {
        private readonly DeviceOptions _options;
        private readonly IDeviceFactory _deviceFactory;

        public UrlPreference(DeviceOptions options, IDeviceFactory deviceFactory)
        {
            _options = options;
            _deviceFactory = deviceFactory;
        }

        public int Priority => 2;

        public IDevice LoadPreference(HttpContext context)
        {
            var url = context.Request.GetDisplayUrl();

            if (url.Contains($"/{_options.MobileCode}/") || url.Contains($"/{_options.MobileCode}."))
            {
                return _deviceFactory.Mobile();
            }

            if (url.Contains($"/{_options.TabletCode}/") || url.Contains($"/{_options.MobileCode}."))
            {
                return _deviceFactory.Tablet();
            }

            return null;
        }

        public void StoreDevice(HttpContext context, IDevice device)
        {
        }

        public void ResetStore(HttpContext context)
        {

        }
    }
}