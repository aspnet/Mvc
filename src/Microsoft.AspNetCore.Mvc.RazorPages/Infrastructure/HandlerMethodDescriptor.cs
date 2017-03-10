// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure
{
    public class HandlerMethodDescriptor
    {
        public MethodInfo Method { get; set; }

        public Func<Page, object, Task<IActionResult>> Executor { get; set; }

        public IEnumerable<IFilterMetadata> Filters {get; set;}
    }
}