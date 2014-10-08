using System;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;

namespace FormatterWebSite.Controllers
{
    public class JsonFormattterController : Controller
    {
        public IActionResult ReturnsIndentedJson()
        {
            var user = new User()
            {
                Id = 1,
                Alias = "john",
                description = "Administrator",
                Designation = "Administrator",
                Name = "John Williams"
            };

            var jsonFrmtr = new JsonOutputFormatter();
            jsonFrmtr.SerializerSettings.Formatting = Formatting.Indented;

            var objResult = new ObjectResult(user);
            objResult.Formatters.Add(jsonFrmtr);

            return objResult;
        }
    }
}