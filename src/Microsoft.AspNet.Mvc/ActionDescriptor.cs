﻿using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.AspNet.Mvc
{
    [DebuggerDisplay("{Path}:{Name}")]
    public class ActionDescriptor
    {
        public ActionDescriptor()
        {
            
        }
        public virtual string Path { get; set; }

        public virtual string Name { get; set; }

        public List<RouteDataActionConstraint> RouteConstraints { get; set; }

        public List<HttpMethodConstraint> MethodConstraints { get; set; }

        public IEnumerable<IActionConstraint> DynamicConstraints { get; set; }
    }
}
