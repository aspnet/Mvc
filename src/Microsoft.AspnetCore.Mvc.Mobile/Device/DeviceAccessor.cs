namespace Microsoft.AspnetCore.Mvc.Mobile.Device
{
    using Abstractions;
    using AspNetCore.Http;
    using Preference;
    using Resolvers;

    public class DeviceAccessor : IDeviceAccessor
    {
        private readonly IDeviceResolver _deviceResolver;
        private readonly ISitePreferenceRepository _repository;
        private readonly IHttpContextAccessor _contextAccessor;

        public DeviceAccessor(
            ISitePreferenceRepository repository,
            IHttpContextAccessor contextAccessor,
            IDeviceResolver deviceResolver)
        {
            _repository = repository;
            _contextAccessor = contextAccessor;
            _deviceResolver = deviceResolver;
        }

        public IDevice Device => _deviceResolver.ResolveDevice(_contextAccessor.HttpContext);
        public IDevice Preference => _repository.LoadPreference(_contextAccessor.HttpContext);
    }
}