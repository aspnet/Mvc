// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ActionResults;

namespace BasicWebSite.Controllers
{
    [Route("/appointments")]
    public class Appointments : ApplicationBaseController
    {
        [HttpGet("")]
        public IActionResult Get()
        {
            return new ContentResult
            {
                Content = "2 appointments available."
            };
        }
    }
}