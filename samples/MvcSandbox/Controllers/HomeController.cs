// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace MvcSandbox.Controllers
{
    using Microsoft.AspnetCore.Mvc.Mobile.Abstractions;

    public class HomeController : Controller
    {
        private readonly IDeviceAccessor _device;

        public HomeController(IDeviceAccessor device)
        {
            _device = device;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
