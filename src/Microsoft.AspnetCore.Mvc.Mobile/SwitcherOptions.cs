namespace Microsoft.AspnetCore.Mvc.Mobile
{
    using Abstractions;

    public class SwitcherOptions
    {
        public SwitcherOptions(IDevicePreference preference)
        {
            Preference = preference;
        }

        public string MobileKey { get; set; } = "mobile";
        public string TabletKey { get; set; } = "tablet";
        public string NormalKey { get; set; } = "normal";
        public string ResetKey { get; set; } = "reset";
        public IDevicePreference Preference { get; }
    }
}