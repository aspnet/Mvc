namespace Microsoft.AspnetCore.Mvc.Mobile.Device.Resolvers
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions;
    using AspNetCore.Http;

    public class AgentResolver : IDeviceResolver
    {
        private readonly DeviceOptions _options;
        private readonly IDeviceFactory _deviceFactory;

        private static readonly string[] KnownMobileUserAgentPrefixes =
        {
            "w3c ", "w3c-", "acs-", "alav", "alca", "amoi", "audi", "avan", "benq",
            "bird", "blac", "blaz", "brew", "cell", "cldc", "cmd-", "dang", "doco",
            "eric", "hipt", "htc_", "inno", "ipaq", "ipod", "jigs", "kddi", "keji",
            "leno", "lg-c", "lg-d", "lg-g", "lge-", "lg/u", "maui", "maxo", "midp",
            "mits", "mmef", "mobi", "mot-", "moto", "mwbp", "nec-", "newt", "noki",
            "palm", "pana", "pant", "phil", "play", "port", "prox", "qwap", "sage",
            "sams", "sany", "sch-", "sec-", "send", "seri", "sgh-", "shar", "sie-",
            "siem", "smal", "smar", "sony", "sph-", "symb", "t-mo", "teli", "tim-",
            "tosh", "tsm-", "upg1", "upsi", "vk-v", "voda", "wap-", "wapa", "wapi",
            "wapp", "wapr", "webc", "winw", "winw", "xda ", "xda-"
        };

        private static readonly string[] KnownMobileUserAgentKeywords =
        {
            "blackberry", "webos", "ipod", "lge vx", "midp", "maemo", "mmp", "mobile",
            "netfront", "hiptop", "nintendo DS", "novarra", "openweb", "opera mobi",
            "opera mini", "palm", "psp", "phone", "smartphone", "symbian", "up.browser",
            "up.link", "wap", "windows ce"
        };

        private static readonly string[] KnownTabletUserAgentKeywords = { "ipad", "playbook", "hp-tablet", "kindle" };

        public AgentResolver(DeviceOptions options, IDeviceFactory deviceFactory)
        {
            _options = options;
            _deviceFactory = deviceFactory;
        }

        public IDevice ResolveDevice(HttpContext context)
        {
            var agent = context.Request.Headers["User-Agent"].FirstOrDefault()?.ToLowerInvariant();
            // UserAgent keyword detection of Normal devices
            if (agent != null && NormalUserAgentKeywords.Any(normalKeyword => agent.Contains(normalKeyword)))
            {
                return _deviceFactory.Normal();
            }

            // UserAgent keyword detection of Tablet devices
            if (agent != null && TabletUserAgentKeywords.Any(keyword => agent.Contains(keyword) && !agent.Contains("mobile")))
            {
                return _deviceFactory.Tablet();
            }

            // UAProf detection
            if (agent != null && context.Request.Headers.ContainsKey("x-wap-profile") || context.Request.Headers.ContainsKey("Profile"))
            {
                return _deviceFactory.Mobile();
            }

            // User-Agent prefix detection
            if (agent != null && agent.Length >= 4 && MobileUserAgentPrefixes.Any(prefix => agent.StartsWith(prefix)))
            {
                return _deviceFactory.Mobile();
            }

            // Accept-header based detection
            var accept = context.Request.Headers["Accept"];
            if (accept.Any(t => t.ToLowerInvariant() == "wap"))
            {
                return _deviceFactory.Mobile();
            }

            // UserAgent keyword detection for Mobile devices
            if (agent != null && MobileUserAgentKeywords.Any(keyword => agent.Contains(keyword)))
            {
                return _deviceFactory.Mobile();
            }

            // OperaMini special case
            if (context.Request.Headers.Any(header => header.Value.Any(value => value.Contains("OperaMini"))))
            {
                return _deviceFactory.Mobile();
            }

            return _deviceFactory.Normal();
        }

        protected virtual IEnumerable<string> NormalUserAgentKeywords
            => _options.NormalUserAgentKeywords;
        protected virtual IEnumerable<string> MobileUserAgentPrefixes
            => KnownMobileUserAgentPrefixes.Concat(_options.MobileUserAgentPrefixes);
        protected virtual IEnumerable<string> MobileUserAgentKeywords
            => KnownMobileUserAgentKeywords.Concat(_options.MobileUserAgentKeywords);
        protected virtual IEnumerable<string> TabletUserAgentKeywords
            => KnownTabletUserAgentKeywords.Concat(_options.TabletUserAgentKeywords);
    }
}