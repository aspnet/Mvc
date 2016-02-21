// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace TagHelpersWebSite.Controllers
{
    public class EncodersController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Using the default HTML encoder";
            return View();
        }

        public IActionResult CustomEncoder()
        {
            ViewData["Title"] = "Using a custom HTML encoder";
            return View();
        }

        public IActionResult NullEncoder()
        {
            ViewData["Title"] = "Using the null HTML encoder";
            return View();
        }

        // We've defined the behavior when multiple tag helpers target the same element. But this is an extreme corner
        // case since one tag helper even using anything but the default HTML encoder is not going to be common.
        public IActionResult TwoEncoders()
        {
            ViewData["Title"] = "Using two HTML encoders";
            return View();
        }

        // We've defined the behavior when multiple tag helpers target the same element. But this is an extreme corner
        // case since one tag helper even using anything but the default HTML encoder is not going to be common.
        public IActionResult ThreeEncoders()
        {
            ViewData["Title"] = "Using three HTML encoders";
            return View();
        }
    }
}
