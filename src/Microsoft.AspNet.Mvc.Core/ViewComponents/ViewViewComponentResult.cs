
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    public class ViewViewComponentResult : IViewComponentResult
    {
        private readonly IViewEngine _viewEngine;
        private readonly string _viewName;
        private readonly ViewData _viewData;

        public ViewViewComponentResult([NotNull] IViewEngine viewEngine, [NotNull] string viewName, [NotNull] ViewData viewData)
        {
            _viewEngine = viewEngine;
            _viewName = viewName;
            _viewData = viewData;
        }

        public void Execute([NotNull] ViewContext viewContext, [NotNull] TextWriter writer)
        {
            throw new NotImplementedException("There's no support for syncronous views right now.");
        }

        public async Task ExecuteAsync([NotNull] ViewContext viewContext, [NotNull] TextWriter writer)
        {
            var childViewContext = new ViewContext(
                viewContext.ServiceProvider,
                viewContext.HttpContext,
                viewContext.ViewEngineContext,
                _viewData ?? viewContext.ViewData)
            {
                Component = viewContext.Component,
                Url = viewContext.Url,
                Writer = writer,
            };

            var view = await FindView(viewContext.ViewEngineContext, _viewName);
            using (view as IDisposable)
            {
                await view.RenderAsync(childViewContext, writer);
            }
        }

        private async Task<IView> FindView([NotNull] IDictionary<string, object> context, [NotNull] string viewName)
        {
            // Issue #161 in Jira tracks unduping this code.
            var result = await _viewEngine.FindView(context, viewName);
            if (!result.Success)
            {
                var locationsText = string.Join(Environment.NewLine, result.SearchedLocations);
                const string message = @"The view &apos;{0}&apos; was not found. The following locations were searched:{1}.";
                throw new InvalidOperationException(String.Format(
                    CultureInfo.CurrentCulture,
                    message,
                    viewName,
                    locationsText));
            }

            return result.View;
        }
    }
}
