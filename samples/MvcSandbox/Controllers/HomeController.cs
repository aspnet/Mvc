// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MvcSandbox.Controllers
{
    public class HomeController : Controller
    {
        private static int Have = 100;

        [ModelBinder]
        public string Id { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        [ProblemErrorPolicy]
        [HttpGet("/test1")]
        public ActionResult<Person> PostTest([FromQuery] int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            return new Person { Id = id };
        }

        [ProblemErrorPolicy]
        [HttpPost("/test2")]
        public ActionResult<Person> ModelStateTest([FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return person;
        }

        [HttpPost("/problem")]
        public ActionResult<Person> Problem(int cooks)
        {
            if (cooks > 10)
            {
                return new Problem
                {
                    Title = "Too many cooks",
                    Status = StatusCodes.Status429TooManyRequests,
                    ["items-being-cooked"] = new[]
                        {
                            "Pancakes",
                            "Donuts",
                            "Waffles"
                        },
                    ["Donuts-Eaten"] = 5,
                };
            }

            return new Person { Id = 1 };
        }

        [HttpPost("/derived-problem")]
        public ActionResult<Item> DerivedProblem(int cost)
        {
            if (cost > Have)
            {
                return new PaymentRequiredProblem(Have, cost)
                {
                    Accounts = new[]
                    {
                        "/accounts/12345",
                    }
                };
            }

            Have -= cost;

            return new Item { Name = Guid.NewGuid().ToString(), Cost = cost };
        }

        public class Person
        {
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }
        }

        public class Item
        {
            public string Name { get; set; }

            public int Cost { get; set; }
        }


        public class PaymentRequiredProblem : Problem
        {
            public PaymentRequiredProblem()
            {
                // To make Xml formatter happy
            }

            public PaymentRequiredProblem(int have, int want)
            {
                Title = "You require more vespense gas.";
                Detail = $"Your current balance is {have} but that costs {want}.";
                Balance = have;
                Status = StatusCodes.Status402PaymentRequired;
            }

            public int Balance { get; }

            public string[] Accounts { get; set; }
        }
    }
}
