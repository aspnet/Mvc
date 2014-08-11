// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace ModelBindingWebSite.Controllers
{
    public class HomeController : Controller
    {
        [HttpPost]
        public IActionResult Index(byte[] byteValues)
        {
            return Content(byteValues[0].ToString() + "," + byteValues[1].ToString()
                            + "," + byteValues[2].ToString());
        }
    }
}