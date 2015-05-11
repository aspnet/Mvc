// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using BasicWebSite.Models;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.WebUtilities;

namespace BasicWebSite.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult PlainView()
        {
            return View();
        }

        public IActionResult ActionLinkView()
        {
            // This view contains a link generated with Html.ActionLink
            // that provides a host with non unicode characters.
            return View();
        }

        public IActionResult RedirectToActionReturningTaskAction()
        {
            return RedirectToAction("ActionReturningTask");
        }

        public IActionResult RedirectToRouteActionAsMethodAction()
        {
            return RedirectToRoute("ActionAsMethod", new { action = "ActionReturningTask", controller = "Home" });
        }

        public IActionResult NoContentResult()
        {
            return new HttpStatusCodeResult(StatusCodes.Status204NoContent);
        }

        [AcceptVerbs("GET", "POST")]
        [RequireHttps]
        public IActionResult HttpsOnlyAction()
        {
            return new HttpStatusCodeResult(StatusCodes.Status200OK);
        }

        public Task ActionReturningTask()
        {
            Response.Headers.Add("Message", new[] { "Hello, World!" });
            return Task.FromResult(true);
        }

        public IActionResult JsonHelperInView()
        {
            Person person = new Person
            {
                Id = 9000,
                Name = "John <b>Smith</b>"
            };

            return View(person);
        }

        public IActionResult JsonTextInView()
        {
            return View();
        }
        
        public string GetApplicationDescription()
        {
            var actionDescriptor = (ControllerActionDescriptor)ActionContext.ActionDescriptor;
            return actionDescriptor.Properties["description"].ToString();
        }
    }
}