﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InlineConstraintsWebSite.Controllers
{
    [Route("products/[action]")]
    public class InlineConstraints_ProductsController : Controller
    {
        public IDictionary<string, object> Index()
        {
            return ActionContext.RouteData.Values;
        }

        [HttpGet("{id:int?}")]
        public IDictionary<string, object> GetProductById(int id)
        {
            return ActionContext.RouteData.Values;
        }

        [HttpGet("{name:alpha}")]
        public IDictionary<string, object> GetProductByName(string name)
        {
            return ActionContext.RouteData.Values;
        }

        [HttpGet("{dateTime:datetime}")]
        public IDictionary<string, object> GetProductByManufacturingDate(DateTime dateTime)
        {
            return ActionContext.RouteData.Values;
        }

        [HttpGet("{name:length(1,20)?}")]
        public IDictionary<string, object> GetProductByCategoryName(string name)
        {
            return ActionContext.RouteData.Values;
        }

        [HttpGet("{catId:int:range(10, 100)}")]
        public IDictionary<string, object> GetProductByCategoryId(int catId)
        {
            return ActionContext.RouteData.Values;
        }

        [HttpGet("{price:float?}")]
        public IDictionary<string, object> GetProductByPrice(float price)
        {
            return ActionContext.RouteData.Values;
        }
        
        [HttpGet("{manId:int:min(10)?}")]
        public IDictionary<string, object> GetProductByManufacturerId(int manId)
        {
            return ActionContext.RouteData.Values;
        }

        public string GetGeneratedLink()
        {
            var query = ActionContext.HttpContext.Request.Query;
            var values = query
                    .Where(kvp => kvp.Key != "newAction" && kvp.Key != "newController")
                    .ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value[0]);

            return Url.Action(query["newAction"], query["newController"], values);
        }
    }
}