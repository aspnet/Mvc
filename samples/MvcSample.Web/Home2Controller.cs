using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using MvcSample.Web.Models;

namespace MvcSample.Web.RandomNameSpace
{
    public class Home2Controller : Controller
    {
        private User _user = new User() { Name = "User Name", Address = "Home Address" };

        public string Index()
        {
            return "Hello World: my namespace is " + this.GetType().Namespace;
        }

        public ActionResult Something()
        {
            return new ContentResult
            {
                Content = "Hello World From Content"
            };
        }

        public ActionResult Hello()
        {
            return Content("Hello World", null, null);
        }

        public void Raw()
        {
            Context.Response.WriteAsync("Hello World raw");
        }

        public ActionResult UserJson()
        {
            var jsonResult = Json(_user);
            jsonResult.Indent = false;

            return jsonResult;
        }

        public new User User()
        {
            return _user;
        }
    }
}