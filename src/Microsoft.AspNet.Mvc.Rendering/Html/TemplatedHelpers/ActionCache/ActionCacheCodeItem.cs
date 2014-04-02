using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Rendering
{
    internal class ActionCacheCodeItem : ActionCacheItem
    {
        public Func<ViewContext, Task<string>> Action { get; set; }

        public override Task<string> Execute(ViewContext viewContext, ViewDataDictionary viewData)
        {
            return Action(new ViewContext(viewContext)
            {
                ViewData = new ViewDataDictionary(viewData)
            });
        }
    }
}
