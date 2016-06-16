namespace Microsoft.AspnetCore.Mvc.Mobile.Preference
{
    using System.Threading.Tasks;
    using AspNetCore.Http;
    using AspNetCore.Routing;
    using Device;

    public class PreferenceSwitcher
    {
        private readonly DeviceOptions _deviceOptions;
        private readonly SwitcherOptions _options;
        private readonly ISitePreferenceRepository _repository;

        public PreferenceSwitcher(SwitcherOptions options, ISitePreferenceRepository repository, DeviceOptions deviceOptions)
        {
            _options = options;
            _repository = repository;
            _deviceOptions = deviceOptions;
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
                        _repository.SavePreference(context,
                            new LiteDevice(DeviceType.Mobile, _deviceOptions.MobileCode));
                    }
                    else if (device == _options.TabletKey)
                    {
                        _repository.SavePreference(context,
                            new LiteDevice(DeviceType.Tablet, _deviceOptions.MobileCode));
                    }
                    else if (device == _options.NormalKey)
                    {
                        _repository.SavePreference(context,
                           new LiteDevice(DeviceType.Normal));
                    }
                    else
                    {
                        _repository.ResetPreference(context);
                    }
                }

                context.Response.Redirect("/");
            });
        }
    }
}