// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace InlineConstraintsWebSite.Controllers
{
    [Route("producttype/[action]")]
    public class InlineConstraints_ConsumableController : Controller
    {
        [HttpGet("{type:producttype(consumable)}")]
        public IActionResult Index(string type)
        {
            return View("ItemType", type + " From Consumable Controller");
        }
    }
}