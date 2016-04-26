// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BasicWebSite.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;

namespace BasicWebSite.Controllers.ContentNegotiation
{
    public class FallbackOnTypeBasedMatchController : Controller
    {
        private readonly IOptions<MvcOptions> _mvcOptions;
        private readonly JsonOutputFormatter _jsonOutputFormatter;

        public FallbackOnTypeBasedMatchController(IOptions<MvcOptions> mvcOptions)
        {
            _mvcOptions = mvcOptions;

            for (var i = 0; i < mvcOptions.Value.OutputFormatters.Count; i++)
            {
                _jsonOutputFormatter = mvcOptions.Value.OutputFormatters[i] as JsonOutputFormatter;
                if (_jsonOutputFormatter != null)
                {
                    break;
                }
            }
        }

        public int UseTheFallback_WithDefaultFormatters(int input)
        {
            return input;
        }

        public IActionResult UseTheFallback_UsingCustomFormatters(int input)
        {
            var objectResult = new ObjectResult(input);

            // Request content type is application/custom.
            // PlainTextFormatter cannot write because it does not support the type.
            // JsonOutputFormatter cannot write in the first attempt because it does not support the
            // request content type.
            objectResult.Formatters.Add(new PlainTextFormatter());
            objectResult.Formatters.Add(_jsonOutputFormatter);

            return objectResult;
        }

        public IActionResult FallbackGivesNoMatch(int input)
        {
            var objectResult = new ObjectResult(input);

            // Request content type is application/custom.
            // PlainTextFormatter cannot write because it does not support the type.
            objectResult.Formatters.Add(new PlainTextFormatter());

            return objectResult;
        }

        public IActionResult OverrideTheFallback_UsingCustomFormatters(int input)
        {
            var objectResult = new ObjectResult(input);
            objectResult.Formatters.Add(new HttpNotAcceptableOutputFormatter());
            objectResult.Formatters.Add(new PlainTextFormatter());
            objectResult.Formatters.Add(_jsonOutputFormatter);

            return objectResult;
        }

        public IActionResult OverrideTheFallback_WithDefaultFormatters(int input)
        {
            var objectResult = new ObjectResult(input);
            objectResult.Formatters.Add(new HttpNotAcceptableOutputFormatter());
            foreach (var formatter in _mvcOptions.Value.OutputFormatters)
            {
                objectResult.Formatters.Add(formatter);
            }

            return objectResult;
        }

        public IActionResult ReturnString(
            bool matchFormatterOnObjectType,
            [FromServices] IOptions<MvcOptions> optionsAccessor)
        {
            var objectResult = new ObjectResult("Hello World!");
            if (matchFormatterOnObjectType)
            {
                objectResult.Formatters.Add(new HttpNotAcceptableOutputFormatter());
            }

            foreach (var formatter in optionsAccessor.Value.OutputFormatters)
            {
                objectResult.Formatters.Add(formatter);
            }

            return objectResult;
        }
    }
}