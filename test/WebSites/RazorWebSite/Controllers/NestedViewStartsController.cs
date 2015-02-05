﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace RazorWebSite.Controllers
{
    public class NestedViewStartsController : Controller
    {
        public ViewResult Index()
        {
            return View("NestedViewStarts/Index");
        }

        public ViewResult NestedViewStartUsingParentDirectives()
        {
            var model = new Person
            {
                Name = "Controller-Person"
            };

            return View("~/Views/NestedViewStartUsingParentDirectives/Nested/Index.cshtml", model);
        }
    }
}