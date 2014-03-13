
namespace Microsoft.AspNet.Mvc
{
    public interface IViewComponentResultHelper
    {
        IViewComponentResult Content(string content);

        IViewComponentResult Json(object value);

        IViewComponentResult View(string viewName, ViewData viewData);
    }
}
