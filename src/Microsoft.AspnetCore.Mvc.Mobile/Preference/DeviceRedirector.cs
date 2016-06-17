namespace Microsoft.AspnetCore.Mvc.Mobile.Preference
{
    using Abstractions;
    using AspNetCore.Http;
    using AspNetCore.Http.Extensions;

    public class DeviceRedirector : IDeviceRedirector
    {
        public virtual void RedirectToDevice(HttpContext context, string code = "")
        {
            var referrerUrl = context.Request.GetDisplayUrl() + "/";
            if (context.Request.Headers.ContainsKey("Referer"))
            {
                referrerUrl = context.Request.Headers["Referer"];
            }

            context.Response.Redirect(referrerUrl);
        }
    }
}