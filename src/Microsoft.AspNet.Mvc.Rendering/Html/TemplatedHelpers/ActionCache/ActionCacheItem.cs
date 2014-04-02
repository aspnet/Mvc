using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Rendering
{
    internal abstract class ActionCacheItem
    {
        public abstract string Execute(HtmlHelper html, ViewDataDictionary viewData);
    }
}