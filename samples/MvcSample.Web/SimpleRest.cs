using Microsoft.AspNet.Mvc;

namespace MvcSample.Web
{
    [Route("api/REST")]
    public class SimpleRest : Controller
    {
        [HttpGet]
        public string ThisIsAGetMethod()
        {
            return "Get method";
        }

        [HttpGet("[action]")]
        public string GetOtherThing()
        {
            return "Get other thing";
        }

        [HttpGet("Link")]
        public string GenerateLink(string action = null, string controller = null)
        {
            return Url.Action(action, controller);
        }
    }
}
