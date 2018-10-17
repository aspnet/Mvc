// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RoutingWebSite
{
    public class DefaultValuesController : Controller
    {
        private readonly TestResponseGenerator _generator;

        public DefaultValuesController(TestResponseGenerator generator)
        {
            _generator = generator;
        }

        public IActionResult Index(string id)
        {
            return _generator.Generate(id == null ? "/DefaultValues" : "/DefaultValues/DefaultValues/Index/" + id);
        }
    }
}