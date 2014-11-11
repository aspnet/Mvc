﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace RequestServicesWebSite
{
    [Route("Other/[action]")]
    public class OtherController : Controller
    {
        // This only matches a specific requestId value
        [HttpGet]
        [RequestScopedActionConstraint("b40f6ec1-8a6b-41c1-b3fe-928f581ebaf5")]
        public string FromActionConstraint()
        {
            return "b40f6ec1-8a6b-41c1-b3fe-928f581ebaf5";
        }

        [HttpGet]
        [TypeFilter(typeof(RequestScopedFilter))]
        public void FromFilter()
        {
        }

        [HttpGet]
        public IActionResult FromView()
        {
            return View("View");
        }

        [HttpGet]
        public IActionResult FromTagHelper()
        {
            return View("TagHelper");
        }

        [HttpGet]
        public IActionResult FromViewComponent()
        {
            return View("ViewComponent");
        }
    }
}