using System.Collections.Generic;
using System.Net;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using MvcSample.Web.Models;

namespace MvcSample.Web.RandomNameSpace
{
    public class Home2Controller
    {
        private readonly List<User> _users = new List<User>
        {
            new User { Name = "Test" }
        };
        private readonly User _user = new User() { Name = "User Name", Address = "Home Address" };
        
        [Activate]
        public HttpResponse Response
        {
            get; set;
        }

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
            Response.WriteAsync("Hello World raw");
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

        public ObjectContetResult<User> Mutate(string name)
        {
            var user = _users.Find(u => string.Equals(u.Name, name, System.StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                return HttpStatusCode.NotFound;
            }

            user.Age++;
            return user;
        }
    }
}