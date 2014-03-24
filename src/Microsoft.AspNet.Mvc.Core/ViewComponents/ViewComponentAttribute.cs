
using System;

namespace Microsoft.AspNet.Mvc
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ViewComponentAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
