using System.Collections.Generic;

namespace Mvc.KodKod.Tool
{
    public class SwaggerApi
    {
        public string Path { get; set; }

        public List<SwaggerOperation> Operations { get; } = new List<SwaggerOperation>();
    }
}