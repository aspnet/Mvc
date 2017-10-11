// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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

            // These are somewhat highly sensitive to order, so be careful.
            //
            // [HttpGet]
            options.ApiDescriptionProfiles.Add(new GetAllApiDescriptionProfile());
            options.ApiDescriptionProfiles.Add(new GetByIdApiDescriptionProfile());
            options.ApiDescriptionProfiles.Add(new FindAllApiDescriptionProvider());
            options.ApiDescriptionProfiles.Add(new FindSingleApiDescriptionProvider());

            // Fallback
            options.ApiDescriptionProfiles.Add(new UnknownApiDescriptionProfile());
        }
    }
}
