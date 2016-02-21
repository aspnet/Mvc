// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ApiExplorerWebSite
{
    [Route("ApiExplorerResponseTypeWithAttribute/[Action]")]
    public class ApiExplorerResponseTypeWithAttributeController : Controller
    {
        [HttpGet]
        [Produces(typeof(Customer))]
        public void GetVoid()
        {
        }

        [HttpGet]
        [Produces("application/json", Type = typeof(Product))]
        public object GetObject()
        {
            return null;
        }

        [HttpGet]
        [Produces("application/json", Type = typeof(string))]
        public IActionResult GetIActionResult()
        {
            return new EmptyResult();
        }

        [HttpGet]
        [Produces("application/json", Type = typeof(int))]
        public Task GetTask()
        {
            return null;
        }

        [HttpGet]
        [Produces(typeof(Customer))] // It's possible to lie about what type you return
        public Product GetProduct()
        {
            return null;
        }
    }
}