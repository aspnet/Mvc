
using Microsoft.AspNet.Mvc;

namespace MvcSample.Web
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}