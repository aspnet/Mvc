namespace Microsoft.AspnetCore.Mvc.Mobile.Abstractions
{
    using AspNetCore.Http;

    public interface IDeviceRedirector
    {
        void RedirectToDevice(HttpContext context, string code = "");
    }
}