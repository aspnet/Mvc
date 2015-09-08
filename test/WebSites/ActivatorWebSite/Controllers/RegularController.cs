// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ActionResults;

namespace ActivatorWebSite
{
    public class RegularController : Controller
    {
        public async Task<EmptyResult> Index()
        {
            // This verifies that ModelState and Context are activated.
            if (ModelState.IsValid)
            {
                await Context.Response.WriteAsync("Hello world");
            }

            return new EmptyResult();
        }
    }
}