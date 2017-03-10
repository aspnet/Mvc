// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BasicWebSite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BasicWebSite.Controllers
{
    public class TempDataPropertyController : Controller
    {
        [TempData]
        public string Message { get; set; }

        [HttpPost]
        public IActionResult CreateForView(Person person)
        {
            if (ModelState.IsValid)
            {
                // save to db
                Message = "Success (from Temp Data)";
                return RedirectToAction("DetailsView", person);
            }

            Message = "Failure (from Temp Data)";
            return RedirectToAction("DetailsView", person);
        }

        [HttpPost]
        public IActionResult Create(Person person)
        {
            if (ModelState.IsValid)
            {
                // save to db
                Message = "Success (from Temp Data)";
                return RedirectToAction("Details", person);
            }

            Message = "Failure (from Temp Data)";
            return RedirectToAction("Details", person);
        }

        public IActionResult DetailsView(Person person)
        {
            ViewData["Message"] = Message;
            return View(person);
        }

        public string Details(Person person)
        {
            return $"{Message} for person {person.FullName} with id {person.id}.";
        }

        public void CreateNoRedirect(Person person)
        {
            if (ModelState.IsValid)
            {
                // save to db
                Message = "Success (from Temp Data)";
            }
        }

        public string TempDataKept()
        {
            TempData.Keep();
            return Message;
        }

        public string ReadTempData()
        {
            return Message;
        }
    }
}
