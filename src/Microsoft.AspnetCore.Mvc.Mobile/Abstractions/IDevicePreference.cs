namespace Microsoft.AspnetCore.Mvc.Mobile.Abstractions
{
    using AspNetCore.Http;

    public interface IDevicePreference
    {
        int Priority { get; }
        IDevice LoadPreference(HttpContext context);
        void StoreDevice(HttpContext context, IDevice device);
        void ResetStore(HttpContext context);
    }
}