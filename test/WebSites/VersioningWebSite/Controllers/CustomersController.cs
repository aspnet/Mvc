// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc;

namespace VersioningWebSite
{
    // Scenario:
    // Version constraint provided separately from the attribute route.
    [Route("Customers")]
    public class CustomersController
    {
        private readonly TestResponseGenerator _generator;

        public CustomersController(TestResponseGenerator generator)
        {
            _generator = generator;
        }

        [HttpGet("{id}")]
        [Version(MaxVersion = 2)]
        public IActionResult Get(int id)
        {
            return _generator.Generate();
        }

        [HttpGet("{id}")]
        [Version(MinVersion = 3, MaxVersion = 5)]
        public IActionResult GetV3ToV5(int id)
        {
            return _generator.Generate();
        }
    }
}