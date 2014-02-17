using System;
using System.Reflection;

namespace Microsoft.AspNet.Mvc
{
    public class TypeMethodBasedActionDescriptor : ActionDescriptor
    {
        public override string Path
        {
            get
            {
                return ControllerDescriptor.Name;
            }
            set
            {
                throw new InvalidOperationException("Cannot override path");
            }
        }

        public string ControllerName
        {
            get
            {
                return ControllerDescriptor.Name;
            }
        }

        public MethodInfo MethodInfo { get; set; }

        public ControllerDescriptor ControllerDescriptor { get; set; }
    }
}
