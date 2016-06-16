namespace Microsoft.AspnetCore.Mvc.Mobile.Preference
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AspNetCore.Http;
    using Device;
    using Device.Resolvers;

    public class SitePreferenceRepository : ISitePreferenceRepository
    {
        private readonly IEnumerable<IDevicePreference> _preferences;
        private readonly SwitcherOptions _options;
        private readonly IDeviceResolver _deviceResolver;

        public SitePreferenceRepository(IEnumerable<IDevicePreference> preferences, SwitcherOptions options, IDeviceResolver deviceResolver)
        {
            _preferences = preferences;
            _options = options;
            _deviceResolver = deviceResolver;
        }

        public IDevice LoadPreference(HttpContext context)
            => _preferences
                    .OrderByDescending(t => t.Priority)
                    .Select(t => t.LoadPreference(context))
                    .FirstOrDefault(t => t != null) ?? _deviceResolver.ResolveDevice(context);

        public void ResetPreference(HttpContext context) => _options.Preference.ResetStore(context);

        public void SavePreference(HttpContext context, IDevice device) => _options.Preference.StoreDevice(context, device);
    }
}