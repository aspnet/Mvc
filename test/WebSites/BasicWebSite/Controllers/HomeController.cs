// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using BasicWebSite.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;

namespace BasicWebSite.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Keep the return type as object to ensure that we don't
        // wrap IActionResult instances into ObjectResults.
        public object PlainView()
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

        public IActionResult RedirectToActionWithFragment()
        {
            return RedirectToAction("ActionReturningTask", "Home", null, "test");
        }

        public IActionResult RedirectToRouteActionAsMethodAction()
        {
            return RedirectToRoute("ActionAsMethod", new { action = "ActionReturningTask", controller = "Home" });
        }

        public IActionResult RedirectToRouteUsingRouteNameAndFragment()
        {
            return RedirectToRoutePermanent("ActionAsMethod", new { action = "ActionReturningTask", controller = "Home" }, fragment: "test");
        }

        public IActionResult RedirectToRouteUsingRouteName()
        {
            return RedirectToRoute("OrdersApi", new { id = 10 });
        }

        public IActionResult NoContentResult()
        {
            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }

        [AcceptVerbs("GET", "POST")]
        [RequireHttps]
        public IActionResult HttpsOnlyAction()
        {
            return Ok();
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
                id = 9000,
                FullName = "John <b>Smith</b>"
            };

            return View(person);
        }

        public IActionResult JsonHelperWithSettingsInView(bool snakeCase)
        {
            Person person = new Person
            {
                id = 9000,
                FullName = "John <b>Smith</b>"
            };
            ViewData["naming"] = snakeCase ? (NamingStrategy)new SnakeCaseNamingStrategy() : new DefaultNamingStrategy();

            return View(person);
        }

        public IActionResult ViewWithPrefixedAttributeValue()
        {
            return View();
        }

        public string GetApplicationDescription()
        {
            return ControllerContext.ActionDescriptor.Properties["description"].ToString();
        }
    }
}