// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ActionConstraints;
using Microsoft.AspNet.Mvc.WebApiCompatShim;
using MvcSample.Web.Models;

namespace MvcSample.Web
{
    public class OverloadController
    {
        // All results implement IActionResult so it can be safely returned.
        public IActionResult Get()
        {
            return Content("Get()");
        }

        [Overload]
        public ActionResult Get(int id)
        {
            return Content("Get(id)");
        }

        [Overload]
        public ActionResult Get(int id, string name)
        {
            return Content("Get(id, name)");
        }

        [Overload]
        public ActionResult Get(string bleh)
        {
            return Content("Get(bleh)");
        }

        public ActionResult WithUser()
        {
            return Content("WithUser()");
        }

        // Called for all posts regardless of values provided
        [HttpPost]
        public ActionResult WithUser(User user)
        {
            return Content("WithUser(User)");
        }

        public ActionResult WithUser(int projectId, User user)
        {
            return Content("WithUser(int, User)");
        }

        private ContentResult Content(string content)
        {
            var result = new ContentResult
            {
                Content = content,
            };

            return result;
        }

        private class OverloadAttribute : Attribute, IActionConstraintFactory
        {
            public IActionConstraint CreateInstance(IServiceProvider services)
            {
                return new OverloadActionConstraint();
            }
        }
    }
}
