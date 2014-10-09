// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace RazorWebSite.Controllers
{
    public class PartialViewEngineController : Controller
    {
        public IActionResult ViewWithoutLayout()
        {
            return PartialView();
        }

        public IActionResult ViewWithFullPath()
        {
            return PartialView(@"/Views/ViewEngine/ViewWithFullPath.cshtml");
        }

        public IActionResult ViewWithLayout()
        {
            return PartialView();
        }

        public IActionResult ViewWithNestedLayout()
        {
            return PartialView();
        }
    }
}