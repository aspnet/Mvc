// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JsonPatchSample.Web.Models;
using Microsoft.AspNet.JsonPatch;
using Microsoft.AspNet.Mvc;

namespace JsonPatchSample.Web.Controllers
{
    [Route("jsonpatch/[action]")]
    public class JsonPatchController : Controller
    {
        [HttpPatch]
        public IActionResult JsonPatchWithModelState([FromBody] JsonPatchDocument<Customer> patchDoc)
        {
            if (patchDoc != null)
            {
                var customer = CreateCustomer();

                patchDoc.ApplyTo(customer, ModelState);

                if (!ModelState.IsValid)
                {
                    return HttpBadRequest(ModelState);
                }

                return new ObjectResult(customer);
            }
            else
            {
                return HttpBadRequest(ModelState);
            }
        }

        [HttpPatch]
        public IActionResult JsonPatchWithModelStateAndPrefix(
            [FromBody] JsonPatchDocument<Customer> patchDoc,
            string prefix)
        {
            var customer = CreateCustomer();

            patchDoc.ApplyTo(customer, ModelState, prefix);

            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            return new ObjectResult(customer);
        }

        [HttpPatch]
        public IActionResult JsonPatchWithoutModelState([FromBody] JsonPatchDocument<Customer> patchDoc)
        {
            var customer = CreateCustomer();

            patchDoc.ApplyTo(customer);

            return new ObjectResult(customer);
        }

        [HttpPatch]
        public IActionResult JsonPatchForProduct([FromBody] JsonPatchDocument<Product> patchDoc)
        {
            var product = new Product();

            patchDoc.ApplyTo(product);

            return new ObjectResult(product);
        }

        private Customer CreateCustomer()
        {
            return new Customer
            {
                CustomerName = "John",
                Orders = new List<Order>()
                {
                    new Order
                    {
                        OrderName = "Order0"
                    },
                    new Order
                    {
                        OrderName = "Order1"
                    }
                }
            };
        }
    }
}
