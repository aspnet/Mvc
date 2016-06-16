namespace Microsoft.AspnetCore.Mvc.Mobile.Preference
{
    using AspNetCore.Http;
    using Device;

    public interface ISitePreferenceRepository
    {
        IDevice LoadPreference(HttpContext context);
        void SavePreference(HttpContext context, IDevice device);
        void ResetPreference(HttpContext context);
    }
}