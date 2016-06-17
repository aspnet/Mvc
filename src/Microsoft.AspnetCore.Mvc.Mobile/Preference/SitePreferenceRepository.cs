namespace Microsoft.AspnetCore.Mvc.Mobile.Preference
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AspNetCore.Http;
    using Device;
    using Extensions.Options;

    public class SitePreferenceRepository : ISitePreferenceRepository
    {
        private readonly IEnumerable<IDevicePreference> _preferences;
        private readonly IOptions<SwitcherOptions> _options;
        private readonly IDeviceResolver _deviceResolver;

        public SitePreferenceRepository(IEnumerable<IDevicePreference> preferences, IOptions<SwitcherOptions> options, IDeviceResolver deviceResolver)
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

        public void ResetPreference(HttpContext context) => _options.Value.Preference.ResetStore(context);

        public void SavePreference(HttpContext context, IDevice device) => _options.Value.Preference.StoreDevice(context, device);
    }
}