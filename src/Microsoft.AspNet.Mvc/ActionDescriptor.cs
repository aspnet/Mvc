using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc
{
    public class ActionDescriptor
    {      
        public virtual string Path { get; set; }

        public virtual string Name { get; set; }

        public List<RouteDataActionConstraint> RouteConstraints { get; set; }

        public List<HttpMethodConstraint> MethodConstraints { get; set; }

        public IEnumerable<IActionConstraint> DynamicConstraints { get; set; }
    }
}
