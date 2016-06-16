namespace Microsoft.AspnetCore.Mvc.Mobile.Device
{
    using AspNetCore.Http;
    using Resolvers;

    public class DeviceAccessor : IDeviceAccessor
    {
        private readonly IDeviceResolver _resolver;
        private readonly IHttpContextAccessor _contextAccessor;

        public DeviceAccessor(IDeviceResolver resolver, IHttpContextAccessor contextAccessor)
        {
            _resolver = resolver;
            _contextAccessor = contextAccessor;
        }

        public IDevice Device => _resolver.ResolveDevice(_contextAccessor.HttpContext);
    }
}