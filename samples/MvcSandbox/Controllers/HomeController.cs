// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MvcSandbox.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

    [Route("views/[controller]/[action]/{id?}")]
    public class CustomersController : Controller
    {
        private readonly ILinkGenerator _linkGenerator;

        public CustomersController(ILinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        public IActionResult Index()
        {
            var url = Url.Action(nameof(CustomersController.Create));

            return Content(
                $"<html><body><a href=\"{url}\">Create</a></body></html>",
                "text/html");
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return RedirectToAction(nameof(Details), new { id = 10 });
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            return Content("Details:" + id);
        }
    }

    public class Customer
    {
        public string Name { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

