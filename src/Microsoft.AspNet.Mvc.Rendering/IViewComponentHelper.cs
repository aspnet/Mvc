
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc
{
    public interface IViewComponentHelper
    {
        string Invoke(string name, params object[] args);

        string Invoke(Type componentType, params object[] args);

        void RenderInvoke(string name, params object[] args);

        void RenderInvoke(Type componentType, params object[] args);

        Task<string> InvokeAsync(string name, params object[] args);

        Task<string> InvokeAsync(Type componentType, params object[] args);

        Task RenderInvokeAsync(string name, params object[] args);

        Task RenderInvokeAsync(Type componentType, params object[] args);
    }
}
