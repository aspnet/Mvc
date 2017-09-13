// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ProblemDetailsFilter : IActionFilter
    {
        private readonly IErrorDescriptionFactory _factory;

        public ProblemDetailsFilter(IErrorDescriptionFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Result == null || !(context.Result is StatusCodeResult statusCodeResult))
            {
                return;
            }

            var isExactlyStatusCodeResult = context.Result.GetType() == typeof(StatusCodeResult);
            ProblemDetails problemDetails;
            switch (context.Result)
            {
                case NotFoundResult notFoundResult:
                    problemDetails = GetNotFoundProblemDetails(notFoundResult);
                    break;
                case BadRequestResult badRequestResult:
                    problemDetails = GetBadRequestProblemDetails(badRequestResult);
                    break;
                case UnsupportedMediaTypeResult unsupportedMediaTypeResult:
                    problemDetails = GetUnsupportedMediaTypeProblemDetails(unsupportedMediaTypeResult);
                    break;
                case StatusCodeResult result when isExactlyStatusCodeResult && result.StatusCode == StatusCodes.Status400BadRequest:
                    problemDetails = GetBadRequestProblemDetails(result);
                    break;
                case StatusCodeResult result when isExactlyStatusCodeResult && result.StatusCode == StatusCodes.Status404NotFound:
                    problemDetails = GetNotFoundProblemDetails(result);
                    break;
                case StatusCodeResult result when isExactlyStatusCodeResult && result.StatusCode == StatusCodes.Status415UnsupportedMediaType:
                    problemDetails = GetUnsupportedMediaTypeProblemDetails(result);
                    break;
                default:
                    return;
            }

            var errorDetails = _factory.CreateErrorDescription(context.ActionDescriptor, problemDetails);
            context.Result = new BadRequestObjectResult(errorDetails)
            {
                StatusCode = statusCodeResult.StatusCode,
            };
        }

        private static ProblemDetails GetNotFoundProblemDetails(StatusCodeResult notFoundResult)
        {
            return new ProblemDetails
            {
                Status = notFoundResult.StatusCode,
                Title = Resources.ProblemDetails_404_Title,
            };
        }

        private static ProblemDetails GetBadRequestProblemDetails(StatusCodeResult badRequestResult)
        {
            return new ProblemDetails
            {
                Status = badRequestResult.StatusCode,
                Title = Resources.ProblemDetails_400_Title,
            };
        }

        private static ProblemDetails GetUnsupportedMediaTypeProblemDetails(StatusCodeResult unsupportedMediaTypeResult)
        {
            return new ProblemDetails
            {
                Status = unsupportedMediaTypeResult.StatusCode,
                Title = Resources.ProblemDetails_415_Title,
            };
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
