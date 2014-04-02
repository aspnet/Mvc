using System.Threading.Tasks;
using Microsoft.AspNet.Routing;

namespace Microsoft.AspNet.Mvc
{
    public interface IActionSelector
    {
        Task<ActionDescriptor> SelectAsync(RequestContext context);

        bool Match(ActionDescriptor descriptor, RequestContext context);

        bool IsValidAction(VirtualPathContext context);
    }
}
