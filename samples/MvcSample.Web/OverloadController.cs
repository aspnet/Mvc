using Microsoft.AspNet.Mvc;
using MvcSample.Web.Models;

namespace MvcSample.Web
{
    public class OverloadController : Controller
    {
        // All results implement IActionResult so it can be safely returned.
        public ContentResult Get()
        {
            return Content("Get()", null, null);
        }

        public ActionResult Get(int id)
        {
            return Content("Get(id)", null, null);
        }

        public ActionResult Get(int id, string name)
        {
            return Content("Get(id, name)", null, null);
        }

        public ActionResult WithUser()
        {
            return Content("WithUser()", null, null);
        }

        // Called for all posts regardless of values provided
        [HttpPost]
        public ActionResult WithUser(User user)
        {
            return Content("WithUser(User)", null, null);
        }

        public ActionResult WithUser(int projectId, User user)
        {
            return Content("WithUser(int, User)", null, null);
        }
    }
}
