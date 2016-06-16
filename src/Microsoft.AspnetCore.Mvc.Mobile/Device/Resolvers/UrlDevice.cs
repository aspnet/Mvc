namespace Microsoft.AspnetCore.Mvc.Mobile.Device.Resolvers
{
    using AspNetCore.Http;
    using AspNetCore.Http.Extensions;

    public class UrlDevice : IDeviceResolver, IDeviceStore
    {
        private readonly DeviceOptions _options;

        public UrlDevice(DeviceOptions options)
        {
            _options = options;
        }

        public int Priority => 2;

        public IDevice ResolveDevice(HttpContext context)
        {
            var url = context.Request.GetDisplayUrl();
            if (url.Contains($"/{_options.MobileCode}/"))
            {
                return new LiteDevice(DeviceType.Mobile, _options.MobileCode);
            }

            if (url.Contains($"/{_options.TabletCode}/"))
            {
                return new LiteDevice(DeviceType.Tablet, _options.TabletCode);
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