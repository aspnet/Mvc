namespace Microsoft.AspnetCore.Mvc.Mobile.Device.Resolvers
{
    using Abstractions;
    using AspNetCore.Http;
    using AspNetCore.Http.Extensions;

    public class UrlPreference : IDevicePreference
    {
        private readonly DeviceOptions _options;
        private readonly IDeviceFactory _deviceFactory;
        private readonly IDeviceRedirector _deviceRedirector;

        public UrlPreference(DeviceOptions options, IDeviceFactory deviceFactory, IDeviceRedirector deviceRedirector)
        {
            _options = options;
            _deviceFactory = deviceFactory;
            _deviceRedirector = deviceRedirector;
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
            => _deviceRedirector.RedirectToDevice(context, device.DeviceCode);

        public void ResetStore(HttpContext context)
            => _deviceRedirector.RedirectToDevice(context);
    }
}