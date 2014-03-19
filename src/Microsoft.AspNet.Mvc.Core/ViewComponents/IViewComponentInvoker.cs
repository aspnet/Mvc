
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public interface IViewComponentInvoker
    {
        void Invoke(ComponentInvokerContext context);

        Task InvokeAsync(ComponentInvokerContext context);
    }
}
