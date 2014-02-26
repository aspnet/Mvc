using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public interface IActionSelector
    {
        Task<ActionDescriptor> Select(RequestContext context);

        bool Match(ActionDescriptor descriptor, RequestContext context);
    }
}
