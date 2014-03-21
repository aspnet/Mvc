
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
        private const string ViewPathFormat = "Components/{0}/{1}";

        private readonly IViewEngine _viewEngine;
        private readonly string _viewName;
        private readonly ViewData _viewData;

        public ViewViewComponentResult([NotNull] IViewEngine viewEngine, [NotNull] string viewName, [NotNull] ViewData viewData)
        {
            _viewEngine = viewEngine;
            _viewName = viewName;
            _viewData = viewData;
        }

        public void Execute([NotNull] ComponentContext componentContext)
        {
            ExecuteAsync(componentContext).Wait();
            return;
            throw new NotImplementedException("There's no support for syncronous views right now.");
        }

        public async Task ExecuteAsync([NotNull] ComponentContext componentContext)
        {
            var childViewContext = new ViewContext(
                componentContext.ViewContext.ServiceProvider,
                componentContext.ViewContext.HttpContext,
                componentContext.ViewContext.ViewEngineContext,
                _viewData ?? componentContext.ViewContext.ViewData)
            {
                Component = componentContext.ViewContext.Component,
                Url = componentContext.ViewContext.Url,
                Writer = componentContext.Writer,
            };

            string qualifiedViewName;
            if (_viewName.Length > 0 && _viewName[0] == '/')
            {
                // View name that was passed in is already a rooted path, the view engine will handle this.
                qualifiedViewName = _viewName;
            }
            else
            {
                // This will produce a string like: 
                //  
                //  Components/Cart/Default
                //
                // The view engine will combine this with other path info to search paths like:
                //
                //  Views/Shared/Components/Cart/Default.cshtml
                //  Views/Home/Components/Cart/Default.cshtml
                //  Areas/Blog/Views/Shared/Components/Cart/Default.cshtml
                //
                // This support a controller or area providing an override for component views.
                qualifiedViewName = string.Format(
                    CultureInfo.InvariantCulture,
                    ViewPathFormat,
                    ViewComponentMetadata.GetComponentName(componentContext.ComponentType),
                    _viewName);
            }

            var view = await FindView(componentContext.ViewContext.ViewEngineContext, qualifiedViewName);
            using (view as IDisposable)
            {
                await view.RenderAsync(childViewContext, componentContext.Writer);
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
