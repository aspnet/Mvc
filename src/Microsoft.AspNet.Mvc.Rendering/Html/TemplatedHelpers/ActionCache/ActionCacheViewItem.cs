using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Rendering
{
    internal class ActionCacheViewItem : ActionCacheItem
    {
        public string ViewName { get; set; }

        public override string Execute(HtmlHelper html, ViewDataDictionary viewData)
        {
            ViewEngineResult viewEngineResult = ViewEngines.Engines.FindPartialView(html.ViewContext, ViewName);
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                viewEngineResult.View.Render(new ViewContext(html.ViewContext, viewEngineResult.View, viewData, html.ViewContext.TempData, writer), writer);
                return writer.ToString();
            }
        }
    }
}