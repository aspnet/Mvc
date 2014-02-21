using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding;
using MvcSample.Models;

namespace MvcSample
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View("MyView", new User());
        }

        public IActionResult SaveUser(User user)
        {
            return View("MyView", user);
        }

        public IActionResult Post([MustBeReadFromRequestBody]User user)
        {
            return View("MyView", user);
        }

        public IActionResult Something()
        {
            return new ContentResult
            {
                Content = "Hello World From Content"
            };
        }

        public IActionResult Hello()
        {
            return Result.Content("Hello World");
        }

        public void Raw()
        {
            Context.Response.WriteAsync("Hello World raw");
        }

        public User User()
        {
            User user = new User()
            {
                Name = "My name",
                Address = "My address"
            };

            return user;
        }

        public IActionResult MyView()
        {
            return View(User());
        }
    }
}