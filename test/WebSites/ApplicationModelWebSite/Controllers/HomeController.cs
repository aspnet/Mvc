// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Controllers;

namespace ApplicationModelWebSite
{
    public class HomeController : Controller
    {
        public string GetCommonDescription()
        {
            var actionDescriptor = (ControllerActionDescriptor)ControllerContext.ActionDescriptor;
            return actionDescriptor.Properties["description"].ToString();
        }

        [HttpGet("Home/GetHelloWorld")]
        public object GetHelloWorld([FromHeader] string helloWorld)
        {
            var actionDescriptor = (ControllerActionDescriptor)ControllerContext.ActionDescriptor;
            return actionDescriptor.Properties["source"].ToString() + " - " + helloWorld;
        }
    }
}