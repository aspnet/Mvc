using System;
using Microsoft.AspNet.Mvc.HeaderValueAbstractions;

namespace Microsoft.AspNet.Mvc.ResourceModel
{
    public class ResourceOutputFormat
    {
        public Type DataType { get; set; }

        public IOutputFormatter Formatter { get; set; }

        public MediaTypeHeaderValue MediaType { get; set; }
    }
}