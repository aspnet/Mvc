﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Core;

namespace BasicWebSite
{
    public class MonitorController : Controller
    {
        private readonly ActionDescriptorCreationCounter _counterService;

        public MonitorController(IEnumerable<IActionDescriptorProvider> providers)
        {
            _counterService = providers.OfType<ActionDescriptorCreationCounter>().Single();
        }

        public IActionResult CountActionDescriptorInvocations()
        {
            return Content(_counterService.CallCount.ToString(CultureInfo.InvariantCulture));
        }
    }
}