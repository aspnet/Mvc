// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.JsonPatch;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ActionResults;

namespace MvcSample.Web.Controllers
{
    [Route("api/[controller]")]
    public class JsonPatchController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPatch]
        public IActionResult Patch([FromBody] JsonPatchDocument<Customer> patchDoc)
        {
            var customer = new Customer
            {
                Name = "John",
                Orders = new List<Order>()
                {
                    new Order
                    {
                        OrderName = "Order1"
                    },
                    new Order
                    {
                        OrderName = "Order2"
                    }
                }
            };

            patchDoc.ApplyTo(customer, ModelState);

            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            return new ObjectResult(customer);
        }

        public class Customer
        {
            public string Name { get; set; }

            public List<Order> Orders { get; set; }
        }

        public class Order
        {
            public string OrderName { get; set; }
        }
    }
}
