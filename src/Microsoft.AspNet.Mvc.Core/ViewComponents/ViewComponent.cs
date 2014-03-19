
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    public class ViewComponent
    {
        public IViewComponentResultHelper Result { get; private set; }

        public ViewData ViewData { get; set; }

        public void Initialize(IViewComponentResultHelper result)
        {
            Result = result;
        }
    }
}
