using System.Collections.Generic;
using System.Text;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding;
using MvcSample.Web.Models;

namespace MvcSample.Web.RandomNameSpace
{
    public class BindingController : Controller
    {
        // This should not be model bound by default.
        public int Count { get; set; }

        // Should be model bound using QueryValueProvider.
        [FromQuery]
        public int Count2 { get; set; }

        // Everything for this employee should be tried to be bound from query.
        [FromQuery]
        public Employer Employer { get; set; }

        // Should be model bound from services.
        [Activate]
        public IModelMetadataProvider ModelMetadataProvider { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ActionArguments.ContainsKey("Count2"))
            {
                context.ActionArguments["Count2"] = 123;
            }

            base.OnActionExecuting(context);
        }

        // Assuming no values from the value providers:
        // acceptHeader is non null because of FromHeaderMarker on the parameter.
        // person is non null because of BindAlways marker on the parameter.
        // Address is not null because of HouseNumber property inside Addres is forced to be bound from Query.
        // emp is null because there is no top level property which is marked as force bind.
        public string Bind([FromHeader("Accept")] string acceptHeader, [BindAlways] Person person, Address address, Employer emp)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("AcceptHeader: " + acceptHeader);
            stringBuilder.AppendLine("ModelBoundProperty:Count: " + Count);
            stringBuilder.AppendLine("InjectedProperty:ModelMetadataProvider: " + ModelMetadataProvider.ToString());
            stringBuilder.AppendLine("ModelBoundComplexObject:Person.Name: " + person.Name);
            stringBuilder.AppendLine("ModelBoundComplexObject:Person.Age: " + person.Age);
            stringBuilder.AppendLine("ModelBoundComplexObject:Person.Parent: " + person.Parent);

            return stringBuilder.ToString();
        }

        public string GetEmployerName(Employer emp)
        {
            return emp.Name;
        }

        public void InspectAddress([FromForm] Address add)
        {
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            return Json(new { key = "value" });
        }
    }

    public class Address
    {
        public string Street { get; set; }

        public string State { get; set; }

        [FromQuery]
        public string HouseNumber { get; set; }

        public string Zip { get; set; }
    }

    public class Employer
    {
        public string Name { get; set; }

        public StockInfo StockInfo { get; set; }
    }

    public class StockInfo
    {
        [FromForm]
        public string Symbol { get; set; }
    }

    public class Person
    {
        // should be set from the query string.
        public int Age { get; set; }

        [FromHeader("Host")]
        public string Name { get; set; }

        // Should be set to null if there is no value from the value providers.
        public Person Parent { get; set; }

        // Since Person2 is marked with from body, this will be bound using body.
        public Person2 GrandParent { get; set; }
    }

    [FromBody2]
    public class Person2
    {
        // should be set from the query string.
        public int Age { get; set; }

        public string Name { get; set; }

        // Should be set to null
        public Person Parent { get; set; }
    }
}