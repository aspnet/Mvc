using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class EndpointInfo
    {
        public string Template { get; set; }
        public string Name { get; set; }
        public object Defaults { get; set; }
    }

    public class MvcEndpointDataSourceOptions
    {
        public List<EndpointInfo> Endpoints { get; set; } = new List<EndpointInfo>();
    }
}
