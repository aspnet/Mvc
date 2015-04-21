﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace RoutingWebSite.Controllers
{
    [Route("Order/[action]/{orderId?}", Name = "Order_[action]")]
    public class OrderController : Controller
    {
        private readonly TestResponseGenerator _generator;

        public OrderController(TestResponseGenerator generator)
        {
            _generator = generator;
        }

        [HttpGet]
        public IActionResult Add(int orderId)
        {
            return _generator.Generate("/Order/Add/1");
        }

        [HttpPost]
        public IActionResult Add()
        {
            return _generator.Generate("/Order/Add");
        }

        [HttpPut]
        public IActionResult Edit(int orderId)
        {
            return _generator.Generate("/Order/Edit/1");
        }
    }
}
