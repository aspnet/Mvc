using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Rendering
{
    internal class ActionCacheViewItem : ActionCacheItem
    {
        public string ViewName { get; set; }

        public override async Task<string> Execute(ViewContext viewContext, ViewDataDictionary viewData)
        {
            var viewEngine = viewContext.ServiceProvider.GetService<IViewEngine>();
            var viewEngineResult = await viewEngine.FindPartialView(viewContext.ViewEngineContext, ViewName);

            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                // TODO: Pass through TempData
                await viewEngineResult.View.RenderAsync(new ViewContext(viewContext)
                {
                    ViewData = viewData,
                    Writer = writer,
                });

                return writer.ToString();
            }
        }
    }
}
