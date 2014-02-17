using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    public class ActionDescriptor : ViewPaths
    {
        public ActionDescriptor()
        {
        }

        public List<RouteDataActionConstraint> RouteConstraints { get; set; }

        public List<HttpMethodConstraint> MethodConstraints { get; set; }

        public IEnumerable<IActionConstraint> DynamicConstraints { get; set; }
    }
}
