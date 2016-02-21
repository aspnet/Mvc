// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace UrlHelperSample.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public string UrlContent()
        {
            return Url.Content("~/Bootstrap.min.css");
        }

        public string LinkByUrlAction()
        {
            return Url.Action("UrlContent", "Home", null);
        }

        public string LinkByUrlRouteUrl()
        {
            return Url.RouteUrl("SimplePocoApi", new { id = 10 });
        }
    }
}