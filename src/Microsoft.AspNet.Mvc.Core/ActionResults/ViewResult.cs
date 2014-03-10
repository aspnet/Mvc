﻿using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    public class ViewResult : IActionResult
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IViewEngine _viewEngine;

        public ViewResult(IServiceProvider serviceProvider, IViewEngine viewEngine)
        {
            _serviceProvider = serviceProvider;
            _viewEngine = viewEngine;
        }

        public string ViewName {get; set; }

        public ViewData ViewData { get; set; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IView view = await FindView(context, ViewName);
            using (view as IDisposable)
            {
                context.HttpContext.Response.ContentType = "text/html";
                using (var writer = new StreamWriter(context.HttpContext.Response.Body, Encoding.UTF8, 1024, leaveOpen: true))
                {
                    var viewContext = new ViewContext(context.HttpContext, ViewData)
                    {
                        ServiceProvider = _serviceProvider
                    };
                    await view.RenderAsync(viewContext, writer);
                }
            }
        }

        private async Task<IView> FindView(ActionContext actionContext, string viewName)
        {
            ViewEngineResult result = await _viewEngine.FindView(actionContext, viewName);
            if (!result.Success)
            {
                string locationsText = String.Join(Environment.NewLine, result.SearchedLocations);
                const string message = @"The view &apos;{0}&apos; was not found. The following locations were searched:{1}.";
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, message, viewName, locationsText));
            }

            return result.View;
        }
    }
}
