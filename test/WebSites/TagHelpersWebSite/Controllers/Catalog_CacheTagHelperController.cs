// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace TagHelpersWebSite.Controllers
{
    public class Catalog_CacheTagHelperController : Controller
    {
        [HttpGet("/catalog")]
        public ViewResult Splash(int categoryId, int correlationId)
        {
            var category = categoryId == 1 ? "Laptops" : "Phones";
            ViewData["Category"] = category;
            ViewData["Locale"] = Request.Headers["Locale"] == "N" ? "North" : "Default";
            ViewData["CorrelationId"] = correlationId;

            return View();
        }

        [HttpGet("/catalog/{id:int}")]
        public ViewResult Details(int id)
        {
            ViewData["ProductId"] = id;
            return View();
        }
    }
}
