// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
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
                    Status = Microsoft.AspNetCore.Http.StatusCodes.Status429TooManyRequests,
                    ["items-being-cooked"] = new[]
                    {
                        "Pancakes",
                        "Donuts",
                        "Waffles"
                    }
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
    }
}
