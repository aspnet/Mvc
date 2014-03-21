﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.Abstractions;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class ViewContext
    {
        public ViewContext(IServiceProvider serviceProvider, HttpContext httpContext, IDictionary<string, object> viewEngineContext, ViewData viewData)
        {
            ServiceProvider = serviceProvider;
            HttpContext = httpContext;
            ViewEngineContext = viewEngineContext;
            ViewData = viewData;
        }

        public IViewComponentHelper Component { get; set; }

        public HttpContext HttpContext { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public IUrlHelper Url { get; set; }

        public ViewData ViewData { get; private set; }

        public IDictionary<string, object> ViewEngineContext { get; private set; }

        public TextWriter Writer { get; set; }
    }
}
