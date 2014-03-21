
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public interface IViewComponentInvoker
    {
        void Invoke([NotNull] ComponentContext context);

        Task InvokeAsync([NotNull] ComponentContext context);
    }
}
