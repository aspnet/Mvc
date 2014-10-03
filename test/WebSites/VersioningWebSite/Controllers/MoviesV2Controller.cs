﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;

namespace VersioningWebSite
{
    public class MoviesV2Controller : Controller
    {
        private readonly TestResponseGenerator _generator;

        public MoviesV2Controller(TestResponseGenerator generator)
        {
            _generator = generator;
        }

        [VersionPut("Movies/{id}", "2", null)]
        public IActionResult Put(int id)
        {
            return _generator.Generate();
        }
    }
}