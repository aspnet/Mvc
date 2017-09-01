// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MvcSandbox.Controllers
{
    public class HomeController : Controller
    {
        [ModelBinder]
        public string Id { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("/EditModel")]
        public ActionResult<Person> EditModel(Person model)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            return model;
        }

        private const int AvailableFunds = 100;

        [HttpPost("/problem")]
        public ActionResult<Person> Problem(int cooks)
        {
            if (cooks > 10)
            {
                return new TooManyCooksProblem
                {
                    Title = "Too many cooks",
                    Status = StatusCodes.Status429TooManyRequests,
                    ItemsBeingCooked = new[]
                    {
                        "Pancakes",
                        "Donuts",
                        "Waffles"
                    },
                    DonutsEaten = 5,
                };
            }

            return new Person { Id = 1 };
        }

        public class Person
        {
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }
        }


        public class Order
        {
            public int OrderId { get; set; }

            public decimal Total { get; set; }
        }

        private class TooManyCooksProblem : ProblemDescription
        {
            public string[] ItemsBeingCooked { get; set; }

            public int DonutsEaten { get; set; }
        }
    }
}
