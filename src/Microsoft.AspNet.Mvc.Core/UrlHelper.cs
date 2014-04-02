using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Routing;

namespace Microsoft.AspNet.Mvc
{
    public class UrlHelper : IUrlHelper
    {
        private readonly HttpContext _httpContext;
        private readonly IRouter _router;
        private readonly IDictionary<string, object> _ambientValues;

        public UrlHelper(IContextAccessor<ActionContext> contextAccessor)
        {
            _httpContext = contextAccessor.Value.HttpContext;
            _router = contextAccessor.Value.Router;
            _ambientValues = contextAccessor.Value.RouteValues;
        }

        public string Action(string action, string controller, object values)
        {
            var valuesDictionary = new RouteValueDictionary(values);

            if (action != null)
            {
                valuesDictionary["action"] = action;
            }

            if (controller != null)
            {
                valuesDictionary["controller"] = controller;
            }

            return RouteCore(valuesDictionary);
        }

        public string Route(object values)
        {
            return RouteCore(new RouteValueDictionary(values));
        }

        private string RouteCore(IDictionary<string, object> values)
        {
            var context = new VirtualPathContext(_httpContext, _ambientValues, values);
            
            var path = _router.GetVirtualPath(context);
            if (path == null)
            {
                return null;
            }

            // HttpAbstractions #28 tracks centrallizing this.
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}://{1}/{2}",
                _httpContext.Request.Scheme,
                _httpContext.Request.Host,
                _httpContext.Request.PathBase + path);
        }
    }
}
