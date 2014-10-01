// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace VersioningWebSite
{
    // Scenario
    // V1 of the API is read-only and unconstrained
    // V2 of the API is constrained
    public class TicketsController : Controller
    {
        private readonly TestResponseGenerator _generator;

        public TicketsController(TestResponseGenerator generator)
        {
            _generator = generator;
        }

        [HttpGet("/Tickets")]
        public IActionResult Get()
        {
            return _generator.Generate();
        }

        [HttpGet("/Tickets/{id}")]
        public IActionResult GetById(int id)
        {
            return _generator.Generate();
        }

        [VersionPost("/Tickets", "2", maxVersion: null)]
        public IActionResult Post()
        {
            return _generator.Generate();
        }

        [VersionPut("/Tickets/{id}", "2", maxVersion: null)]
        public IActionResult Put(int id)
        {
            return _generator.Generate();
        }

        [VersionDelete("/Tickets/{id}", "2", maxVersion: null)]
        public IActionResult Delete(int id)
        {
            return _generator.Generate();
        }
    }
}