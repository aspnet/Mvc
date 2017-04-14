// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace ApplicationModelWebSite
{
    public class HomeController : Controller
    {
        public string GetCommonDescription()
        {
            return ControllerContext.ActionDescriptor.Properties["description"].ToString();
        }

        [HttpGet("Home/GetHelloWorld")]
        public object GetHelloWorld([FromHeader] string helloWorld)
        {
            return ControllerContext.ActionDescriptor.Properties["source"].ToString() + " - " + helloWorld;
        }

        [HttpGet("Home/CannotBeRouted")]
        [HttpGet("Home/CanBeRouted")]
        [SuppressLinkGenerationConvention]
        public object SuppressLinkGeneration()
        {
            return "Hello world";
        }

        private class SuppressLinkGenerationConvention : Attribute, IActionModelConvention
        {
            public void Apply(ActionModel model)
            {
                var selector = model.Selectors.First(f => f.AttributeRouteModel.Template == "Home/CannotBeRouted");
                selector.AttributeRouteModel.SuppressForPathMatching = true;
            }
        }
    }
}