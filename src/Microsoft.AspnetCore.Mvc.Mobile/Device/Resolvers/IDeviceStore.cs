namespace Microsoft.AspnetCore.Mvc.Mobile.Device.Resolvers
{
    using AspNetCore.Http;

    public interface IDeviceStore
    {
        void StoreDevice(HttpContext context, IDevice device);
        void ResetStore(HttpContext context);
    }
}