namespace Microsoft.AspnetCore.Mvc.Mobile.Preference
{
    using System.Collections.Generic;
    using System.Linq;
    using AspNetCore.Http;
    using Device;
    using Device.Resolvers;

    public class SitePreferenceRepository : ISitePreferenceRepository
    {
        private readonly IEnumerable<IDeviceResolver> _resolvers;
        private readonly IDeviceStore _store;

        public SitePreferenceRepository(IEnumerable<IDeviceResolver> resolvers, IDeviceStore store)
        {
            _resolvers = resolvers;
            _store = store;
        }

        public IDevice LoadPreference(HttpContext context)
        {
            return _resolvers
                        .OrderByDescending(t => t.Priority)
                        .Select(t => t.ResolveDevice(context))
                        .FirstOrDefault(t => t != null) ?? new LiteDevice(DeviceType.Normal);
        }

        public void SavePreference(HttpContext context, IDevice device)
        {
            _store.StoreDevice(context, device);
        }

        public void ResetPreference(HttpContext context)
        {
            _store.ResetStore(context);
        }
    }
}