namespace Microsoft.AspnetCore.Mvc.Mobile.Device.Resolvers
{
    using AspNetCore.Http;

    public interface IDeviceResolver
    {
        int Priority { get; }
        IDevice ResolveDevice(HttpContext context);
    }
}