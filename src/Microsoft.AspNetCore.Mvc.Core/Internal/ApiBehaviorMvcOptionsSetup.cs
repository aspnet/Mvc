// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ApiBehaviorMvcOptionsSetup : IConfigureOptions<MvcOptions>
    {
        private readonly IErrorDescriptionFactory _errorDescriptionFactory;

        public ApiBehaviorMvcOptionsSetup(IErrorDescriptionFactory errorDescriptionFactory)
        {
            _errorDescriptionFactory = errorDescriptionFactory;
        }

        public void Configure(MvcOptions options)
        {
            options.ApiBehavior.InvalidModelStateResponseFactory = GetInvalidModelStateResponse;

            IActionResult GetInvalidModelStateResponse(ActionContext context)
            {
                var errorDetails = _errorDescriptionFactory.CreateErrorDescription(
                    context.ActionDescriptor, 
                    new ValidationProblemDetails(context.ModelState));

                return new BadRequestObjectResult(errorDetails)
                {
                    ContentTypes =
                    {
                        "application/problem+json",
                        "application/problem+xml",
                    },
                };
            }
        }
    }
}
