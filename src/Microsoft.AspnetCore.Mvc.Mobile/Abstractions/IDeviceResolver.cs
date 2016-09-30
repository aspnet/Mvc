namespace Microsoft.AspnetCore.Mvc.Mobile.Abstractions
{
    using AspNetCore.Http;

    public interface IDeviceResolver
    {
        IDevice ResolveDevice(HttpContext context);
    }
}