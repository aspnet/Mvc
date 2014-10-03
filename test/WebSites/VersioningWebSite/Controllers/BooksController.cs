﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace VersioningWebSite
{
    // Scenario
    // Actions define version ranges and some
    // versions overlap.
    public class BooksController : Controller
    {
        private readonly TestResponseGenerator _generator;

        public BooksController(TestResponseGenerator generator)
        {
            _generator = generator;
        }

        [VersionGet("Books", "1", "6", Order = 1)]
        public IActionResult Get()
        {
            return _generator.Generate();
        }

        [VersionGet("Books", "3", "5", Order = 0)]
        public IActionResult GetBreakingChange()
        {
            return _generator.Generate();
        }
    }
}