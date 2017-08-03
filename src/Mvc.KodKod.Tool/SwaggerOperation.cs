using System.Collections.Generic;

namespace Mvc.KodKod.Tool
{
    public class SwaggerOperation
    {
        public string Method { get; set; }

        public string Type { get; set; }

        public List<SwaggerParameter> Parameters { get; } = new List<SwaggerParameter>();
    }
}