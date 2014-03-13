
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public interface IViewComponentInvoker
    {
        Task InvokeAsync(ComponentInvokerContext context);
    }
}
