using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using MvcSample.Web.Models;

namespace MvcSample.Web.RandomNameSpace
{
    public class Home2Controller
    {
        private User _user = new User() { Name = "User Name", Address = "Home Address" };

        public HttpContext Context
        {
            get
            {
                return ActionContext.HttpContext;
            }
        }

        // The property ActionContext gets injected by InitializeController from DefaultControllerFactory.
        public ActionContext ActionContext { get; set; }

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
            return new ContentResult
            {
                Content = "Hello World",
            };
        }

        public void Raw()
        {
            Context.Response.WriteAsync("Hello World raw");
        }

        public ActionResult UserJson()
        {
            var jsonResult = new JsonResult(_user);
            jsonResult.Indent = false;

            return jsonResult;
        }

        public User User()
        {
            return _user;
        }
    }
}