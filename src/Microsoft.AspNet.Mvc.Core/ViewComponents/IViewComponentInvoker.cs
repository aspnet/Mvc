
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public interface IViewComponentInvoker
    {
        void Invoke([NotNull] ViewComponentInvokerContext context);

        Task InvokeAsync([NotNull] ViewComponentInvokerContext context);
    }
}
