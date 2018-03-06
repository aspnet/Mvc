﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ApiBehaviorOptionsSetup : IConfigureOptions<ApiBehaviorOptions>
    {
        private readonly IErrorDescriptionFactory _errorDescriptionFactory;

        public ApiBehaviorOptionsSetup(IErrorDescriptionFactory errorDescriptionFactory)
        {
            _errorDescriptionFactory = errorDescriptionFactory;
        }

        public void Configure(ApiBehaviorOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.InvalidModelStateResponseFactory = GetInvalidModelStateResponse;

            IActionResult GetInvalidModelStateResponse(ActionContext context)
            {
                var errorDetails = _errorDescriptionFactory.CreateErrorDescription(
                    context.ActionDescriptor,
                    context.ModelState);

                var result = (errorDetails is ModelStateDictionary modelState) ?
                    new BadRequestObjectResult(modelState) :
                    new BadRequestObjectResult(errorDetails);

                result.ContentTypes.Add("application/json");

                return result;
            }
        }
    }
}
