﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace VersioningWebSite
{
    // This is the version 2 for an API. The version 1 is unconstrained
    [VersionRoute("Items/{id}", "2", maxVersion: null, Order = -1)]
    public class ItemsV2Controller : Controller
    {
        private readonly TestResponseGenerator _generator;

        public ItemsV2Controller(TestResponseGenerator generator)
        {
            _generator = generator;
        }

        [VersionGet("/Items", "2", maxVersion: null, Order = -1)]
        public IActionResult Get()
        {
            return _generator.Generate();
        }

        [HttpGet]
        public IActionResult Get(int id)
        {
            return _generator.Generate();
        }

        [VersionPost("/Items", "2", maxVersion: null, Order = -1)]
        public IActionResult Post()
        {
            return _generator.Generate();
        }

        [HttpPut]
        public IActionResult Put(int id)
        {
            return _generator.Generate();
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            return _generator.Generate();
        }
    }
}