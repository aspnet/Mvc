﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace RoutingWebSite.Products
{
    [CountryNeutral]
    public class ProductsController : Controller
    {
        private readonly TestResponseGenerator _generator;

        public ProductsController(TestResponseGenerator generator)
        {
            _generator = generator;
        }

        public IActionResult GetProducts()
        {
            return _generator.Generate("/api/Products/CA/GetProducts");
        }
    }
}