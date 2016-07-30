// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace MvcSandbox.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(new Models.SimpleDateTimeModel
            {
                PlainProp1 = System.DateTime.Now,
                PlainProp2 = System.DateTime.Now,
                Prop1 = System.DateTime.Now,
                Prop2 = System.DateTime.Now
            });
        }
    }
}
