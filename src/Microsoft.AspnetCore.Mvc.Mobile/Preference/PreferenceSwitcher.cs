namespace Microsoft.AspnetCore.Mvc.Mobile.Preference
{
    using System.Threading.Tasks;
    using Abstractions;
    using AspNetCore.Http;
    using AspNetCore.Routing;

    public class PreferenceSwitcher
    {
        private readonly SwitcherOptions _options;
        private readonly IDeviceFactory _deviceFactory;
        private readonly ISitePreferenceRepository _repository;

        public PreferenceSwitcher(SwitcherOptions options, ISitePreferenceRepository repository, IDeviceFactory deviceFactory)
        {
            _options = options;
            _repository = repository;
            _deviceFactory = deviceFactory;
        }

        public Task Handle(HttpContext context)
        {
            return Task.Run(() =>
            {
                var device = context.GetRouteValue("device").ToString();
                if (!string.IsNullOrWhiteSpace(device))
                {
                    if (device == _options.MobileKey)
                    {
                        _repository.SavePreference(context, _deviceFactory.Mobile());
                    }
                    else if (device == _options.TabletKey)
                    {
                        _repository.SavePreference(context, _deviceFactory.Tablet());
                    }
                    else if (device == _options.NormalKey)
                    {
                        _repository.SavePreference(context, _deviceFactory.Normal());
                    }
                    else if (device == _options.ResetKey)
                    {
                        _repository.ResetPreference(context);
                    }
                }
            });
        }
    }
}