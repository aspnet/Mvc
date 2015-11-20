// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace MvcSample.Web
{
    [AutoGenerateRouteNames]
    [Route("api/REST")]
    public class SimpleRest : Controller
    {
        [HttpGet]
        public string ThisIsAGetMethod()
        {
            return "Get method";
        }

        [HttpGet("[action]")]
        public string GetOtherThing()
        {
            // Will be GetOtherThing
            return (string)RouteData.Values["action"];
        }

        [HttpGet("Link")]
        public string GenerateLink(string action = null, string controller = null)
        {
            return Url.Action(action, controller);
        }

        [HttpGet("Link/{name}")]
        public string GenerateLinkByName(string name = null)
        {
            // This action leverages [AutoGenerateRouteNames]. Try a URL like api/Rest/Link/SimpleRest_ThisIsAGetMethod
            // which matches the auto-generated name for the ThisIsAGetMethod action.
            return Url.RouteUrl(name);
        }
    }
}
