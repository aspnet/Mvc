// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using MvcSandbox.AuthorizationMiddleware;

namespace MvcSandbox.Controllers
{
    public class HomeController : Controller
    {
        [ModelBinder]
        public string Id { get; set; }

        [AuthorizeMetadata(new[] { "admin" })]
        public IActionResult Index()
        {
            return View();
        }
    }
}
