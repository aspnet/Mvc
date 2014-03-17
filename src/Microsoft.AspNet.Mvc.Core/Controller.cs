using Microsoft.AspNet.Abstractions;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc
{
    public class Controller
    {
        public void Initialize(IActionResultHelper actionResultHelper, IHtmlSettings htmlSettings,
            IModelMetadataProvider metadataProvider)
        {
            Result = actionResultHelper;
            HtmlSettings = htmlSettings;
            ViewData = new ViewData<object>(metadataProvider);
        }

        public IActionResultHelper Result { get; private set; }

        public HttpContext Context { get; set; }

        public IHtmlSettings HtmlSettings { get; private set; }

        public IUrlHelper Url { get; set; }

        public ViewData<object> ViewData { get; set; }

        public dynamic ViewBag
        {
            get { return ViewData; }
        }

        public IActionResult View()
        {
            return View(view: null);
        }

        public IActionResult View(string view)
        {
            return View(view, model: (object)null);
        }

        public IActionResult View<TModel>(TModel model)
        {
            return View(view: null, model: model);
        }

        public IActionResult View<TModel>(string view, TModel model)
        {
            var viewData = ViewData as ViewData<TModel> ?? new ViewData<TModel>(ViewData);
            if (typeof(TModel).IsValueType() || (object)model != null)
            {
                viewData.Model = model;
            }

            return Result.View(view, viewData);
        }
    }
}
