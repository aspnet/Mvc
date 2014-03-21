
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    [ViewComponent]
    public abstract class ViewComponent
    {
        public IViewComponentResultHelper Result { get; private set; }

        public ViewData ViewData { get; set; }

        public void Initialize(IViewComponentResultHelper result)
        {
            Result = result;
        }

        public IViewComponentResult View()
        {
            return View<object>(null, null);
        }

        public IViewComponentResult View(string viewName)
        {
            return View<object>(viewName, null);
        }

        public IViewComponentResult View<TModel>(TModel model)
        {
            return View(null, model);
        }

        public IViewComponentResult View<TModel>(string viewName, TModel model)
        {
            var viewData = new ViewData<TModel>(ViewData);
            if (model != null)
            {
                viewData.Model = model;
            }

            return Result.View(viewName ?? "Default", viewData);
        }
    }
}
