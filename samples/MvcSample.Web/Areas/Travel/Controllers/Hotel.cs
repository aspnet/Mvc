using Microsoft.AspNet.Mvc;

namespace MvcSample.Web
{
    [Area("Travel")]
    public class Hotel : Controller
    {
        public IActionResult BookHotel()
        {
            return View();
        }
    }
}
