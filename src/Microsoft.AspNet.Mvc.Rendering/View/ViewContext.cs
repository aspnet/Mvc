using System;
using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc
{
    public class ViewContext
    {
        public ViewContext(HttpContext context, ViewData viewData)
        {
            HttpContext = context;
            ViewData = viewData;
        }

        public HttpContext HttpContext { get; private set; }

        public IServiceProvider ServiceProvider { get; set; }

        public ViewData ViewData { get; private set; }
    }
}
