using System;

namespace Microsoft.AspNet.Mvc
{
    public class ResourceParameterDescriptor
    {
        public bool IsOptional { get; set; }

        public string Name { get; set; }

        public ParameterDescriptor ParameterDescriptor { get; set; }

        public ResourceParameterSource Source { get; set; }

        public Type Type { get; set; }
    }
}