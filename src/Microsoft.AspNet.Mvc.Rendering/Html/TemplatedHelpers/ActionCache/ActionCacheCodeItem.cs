using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Rendering
{
    internal class ActionCacheCodeItem : ActionCacheItem
    {
        public Func<HtmlHelper, string> Action { get; set; }

        public override string Execute(HtmlHelper html, ViewDataDictionary viewData)
        {
            return Action(MakeHtmlHelper(html, viewData));
        }
    }
}