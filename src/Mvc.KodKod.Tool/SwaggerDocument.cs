using System;
using System.Collections.Generic;

namespace Mvc.KodKod.Tool
{
    public class SwaggerDocument
    {
        public string SwaggerVersion { get; set; }

        public string BasePath { get; set; }

        public List<SwaggerApi> Apis { get; } = new List<SwaggerApi>();
    }
}
