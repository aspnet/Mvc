// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace FiltersWebSite.Controllers
{
    [Route("ResourceFilter/[action]")]
    public class ResourceFilterController : Controller
    {
        [HttpPost]
        [ShortCircuitWithFormatter]
        public string Get()
        {
            return "NeverGetsExecuted";
        }

        [HttpPost]
        [ShortCircuit]
        public string Post()
        {
            return "NeverGetsExecuted";
        }

        private class ShortCircuitWithFormatterAttribute : Attribute, IResourceFilter
        {
            private IOutputFormatter[] _formatters;

            public ShortCircuitWithFormatterAttribute()
            {
                _formatters = new IOutputFormatter[] { new JsonOutputFormatter(
                    new JsonSerializerSettings(),
                    ArrayPool<char>.Shared) };
            }

            public void OnResourceExecuted(ResourceExecutedContext context)
            {
            }

            public void OnResourceExecuting(ResourceExecutingContext context)
            {
                var result = new ObjectResult("someValue");
                foreach (var formatter in _formatters)
                {
                    result.Formatters.Add(formatter);
                }

                context.Result = result;
            }
        }

        private class ShortCircuitAttribute : Attribute, IResourceFilter
        {
            public void OnResourceExecuted(ResourceExecutedContext context)
            {
            }

            public void OnResourceExecuting(ResourceExecutingContext context)
            {
                // ShortCircuit.
                context.Result = new ObjectResult("someValue");
            }
        }
    }
}