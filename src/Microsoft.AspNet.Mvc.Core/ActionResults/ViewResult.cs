﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.Mvc.Rendering;

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

        public async Task ExecuteResultAsync([NotNull] ActionContext context)
        {
            var viewName = ViewName ?? context.ActionDescriptor.Name;
            var view = await FindView(context.RouteValues, viewName);

            using (view as IDisposable)
            {
                context.HttpContext.Response.ContentType = "text/html";
                using (var writer = new StreamWriter(context.HttpContext.Response.Body, Encoding.UTF8, 1024, leaveOpen: true))
                {
                    await view.RenderAsync(viewContext, writer);
                }
            }
        }

        private async Task<IView> FindView([NotNull] IDictionary<string, object> context,[NotNull] string viewName)
        {
            var result = await _viewEngine.FindView(context, viewName);
            if (!result.Success)
            {
                var locationsText = string.Join(Environment.NewLine, result.SearchedLocations);
                const string message = @"The view &apos;{0}&apos; was not found. The following locations were searched:{1}.";
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, message, viewName, locationsText));
            }

            return result.View;
        }

        private ViewContext CreateViewContext([NotNull] ActionContext actionContext, [NotNull] TextWriter writer)
        {
            var urlHelper = new UrlHelper(actionContext.HttpContext, actionContext.Router, actionContext.RouteValues);

            var viewContext = new ViewContext(_serviceProvider, actionContext.HttpContext, actionContext.RouteValues)
            {
                ViewData = ViewData,
                Url = urlHelper,
                Writer = writer,
            };

            viewContext.Component = new DefaultViewComponentHelper(
                _serviceProvider.GetService<IViewComponentSelector>(),
                _serviceProvider.GetService<IViewComponentInvokerFactory>(),
                viewContext,
                writer);

            return viewContext;
        }
    }
}
