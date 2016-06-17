namespace Microsoft.AspnetCore.Mvc.Mobile.Preference
{
    using System;
    using Abstractions;
    using AspNetCore.Http;
    using AspNetCore.Http.Extensions;
    using Extensions.Options;

    public class DeviceRedirector : IDeviceRedirector
    {
        private readonly IOptions<SwitcherOptions> _switcherOptions;
        private readonly IOptions<DeviceOptions> _options;

        public DeviceRedirector(IOptions<DeviceOptions> options, IOptions<SwitcherOptions> switcherOptions)
        {
            _options = options;
            _switcherOptions = switcherOptions;
        }

        public virtual void RedirectToDevice(HttpContext context, string code = "")
        {
            var referrerUrl = context.Request.GetDisplayUrl();
            if (context.Request.Headers.ContainsKey("Referer"))
            {
                referrerUrl = context.Request.Headers["Referer"];
            }

            context.Response.Redirect(DeviceUrl(ResetUrl(new Uri(referrerUrl).AbsolutePath), code));
        }

        protected virtual string DeviceUrl(string resetUrl, string code)
            => $"/{code}{resetUrl}".Replace("//", "/");

        protected virtual string ResetUrl(string referrerUrl)
        {
            var url =
                referrerUrl
                    .Replace($"/{_switcherOptions.Value.SwitchUrl}/{_switcherOptions.Value.NormalKey}", "")
                    .Replace($"/{_switcherOptions.Value.SwitchUrl}/{_switcherOptions.Value.MobileKey}", "")
                    .Replace($"/{_switcherOptions.Value.SwitchUrl}/{_switcherOptions.Value.TabletKey}", "")
                    .Replace($"/{_switcherOptions.Value.SwitchUrl}/{_switcherOptions.Value.ResetKey}", "")
                    .Replace($"/{_options.Value.TabletCode}/", "/")
                    .Replace($"/{_options.Value.MobileCode}/", "/");

            return string.IsNullOrWhiteSpace(url) ? "/" : url;
        }
    }
}