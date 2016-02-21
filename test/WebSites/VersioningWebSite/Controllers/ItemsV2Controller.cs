// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace VersioningWebSite
{
    // This is the version 2 for an API. The version 1 is unconstrained
    [VersionRoute("Items/{id}", versionRange: "2")]
    public class ItemsV2Controller : Controller
    {
        private readonly TestResponseGenerator _generator;

        public ItemsV2Controller(TestResponseGenerator generator)
        {
            _generator = generator;
        }

        [VersionGet("/Items", versionRange: "2")]
        public IActionResult Get()
        {
            return _generator.Generate();
        }

        [HttpGet]
        public IActionResult Get(int id)
        {
            return _generator.Generate();
        }

        [VersionPost("/Items", versionRange: "2")]
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