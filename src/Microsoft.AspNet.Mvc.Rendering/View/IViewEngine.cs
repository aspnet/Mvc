
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public interface IViewEngine
    {
        // Currently this is actually the action context (but the type is erased). We should fix
        // this coupling.
        Task<ViewEngineResult> FindView(object actionContext, string viewName);

        Task<ViewEngineResult> FindComponentView(object actionContext, string viewName);
    }
}
