using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Rendering
{
    internal abstract class ActionCacheItem
    {
        public abstract Task<string> Execute(ViewContext viewContext, ViewDataDictionary viewData);
    }
}
