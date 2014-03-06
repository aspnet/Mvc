using System.Collections.Generic;
using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.Mvc.Common;

namespace Microsoft.AspNet.Mvc
{
    public class ActionContext
    {
        public ActionContext([NotNull]HttpContext httpContext, 
                             [NotNull]IDictionary<string, object> routeValues, 
                             [NotNull]ActionDescriptor actionDescriptor)
        {
            HttpContext = httpContext;
            RouteValues = routeValues;
            ActionDescriptor = actionDescriptor;
        }

        public HttpContext HttpContext { get; private set; }

        public IDictionary<string, object> RouteValues { get; private set; }

        public ActionDescriptor ActionDescriptor { get; private set; }
    }
}
