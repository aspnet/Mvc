namespace Microsoft.AspnetCore.Mvc.Mobile.Preference
{
    using System.Threading.Tasks;
    using Abstractions;
    using AspNetCore.Http;
    using AspNetCore.Routing;
    using Extensions.Options;

    public class PreferenceSwitcher
    {
        private readonly IOptions<SwitcherOptions>_options;
        private readonly IDeviceFactory _deviceFactory;
        private readonly ISitePreferenceRepository _repository;

        public PreferenceSwitcher(IOptions<SwitcherOptions> options, ISitePreferenceRepository repository, IDeviceFactory deviceFactory)
        {
            _options = options;
            _repository = repository;
            _deviceFactory = deviceFactory;
        }

        public virtual Task Handle(HttpContext context)
        {
            return Task.Run(() =>
            {
                var device = context.GetRouteValue("device").ToString();
                if (!string.IsNullOrWhiteSpace(device))
                {
                    if (device == _options.Value.MobileKey)
                    {
                        _repository.SavePreference(context, _deviceFactory.Mobile());
                    }
                    else if (device == _options.Value.TabletKey)
                    {
                        _repository.SavePreference(context, _deviceFactory.Tablet());
                    }
                    else if (device == _options.Value.NormalKey)
                    {
                        _repository.SavePreference(context, _deviceFactory.Normal());
                    }
                    else if (device == _options.Value.ResetKey)
                    {
                        _repository.ResetPreference(context);
                    }
                }
            });
        }
    }
}