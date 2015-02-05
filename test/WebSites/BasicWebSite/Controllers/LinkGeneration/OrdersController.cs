﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;

namespace BasicWebSite.Controllers.LinkGeneration
{
    [Route("api/orders/{id?}", Name = "OrdersApi")]
    public class OrdersController : Controller
    {
        [HttpGet]
        public IActionResult GetAll()
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public IActionResult GetById(int id)
        {
            throw new NotImplementedException();
        }
    }
}