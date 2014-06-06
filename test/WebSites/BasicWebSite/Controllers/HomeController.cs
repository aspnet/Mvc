using Microsoft.AspNet.Mvc;
using Microsoft.Framework.DependencyInjection;
using System.Globalization;

namespace BasicWebSite.Controllers
{
    public class HomeController : Controller
    {
        private ActionDescriptorCreationCounter _counterService;

        public HomeController(INestedProvider<ActionDescriptorProviderContext> counterService)
        {
            _counterService = (ActionDescriptorCreationCounter)counterService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CountActionDescriptorInvocations()
        {
            return Content(_counterService.CallCount.ToString(CultureInfo.InvariantCulture));
        }
    }
}