// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ProblemErrorPolicyAttribute : Attribute, IErrorPolicy, IExceptionFilter, IResultFilter
    {
        public void Apply(ErrorPolicyContext context)
        {
            var parameters = context.Description.ActionDescriptor.Parameters;
            if (parameters.Count > 0)
            {
                context.Description.SupportedResponseTypes.Add(CreateProblemResponse(StatusCodes.Status400BadRequest));

                if (parameters.Any(p => string.Equals("id", p.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Description.SupportedResponseTypes.Add(CreateProblemResponse(StatusCodes.Status404NotFound));
                }
            }

            context.Description.SupportedResponseTypes.Add(CreateProblemResponse(statusCode: 0));

            ApiResponseType CreateProblemResponse(int statusCode)
            {
                return new ApiResponseType
                {
                    ApiResponseFormats = new List<ApiResponseFormat>
                    {
                        new ApiResponseFormat
                        {
                            MediaType = "application/problem+json",
                        },
                    },
                    ModelMetadata = context.MetadataProvider.GetMetadataForType(typeof(Problem)),
                    StatusCode = statusCode,
                    Type = typeof(Problem),
                };
            }
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Result == null)
            {
                var problem = new Problem
                {
                    Type = context.Exception.HelpLink,
                    Title = context.Exception.Message,
                    Status = 500,
                    Detail = context.Exception.StackTrace,
                };

                context.Result = new ObjectResult(problem);
                context.ExceptionHandled = true;
            }
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is StatusCodeResult statusCodeResult)
            {
                var statusCode = statusCodeResult.StatusCode;
                if (statusCode == StatusCodes.Status400BadRequest)
                {
                    var problem = new Problem
                    {
                        Status = statusCodeResult.StatusCode,
                        Title = "400 Bad Request",
                    };
                    context.Result = new ObjectResult(problem)
                    {
                        StatusCode = statusCodeResult.StatusCode,
                    };
                }
                else if (statusCode == StatusCodes.Status404NotFound)
                {
                    var problem = new Problem
                    {
                        Status = statusCodeResult.StatusCode,
                        Title = "No value found for the specified id.",
                    };
                    context.Result = new ObjectResult(problem)
                    {
                        StatusCode = statusCodeResult.StatusCode,
                    };
                }
            }
            else if (context.Result is BadRequestObjectResult badRequestObjectResult && badRequestObjectResult.Value is SerializableError serializableError)
            {
                var problem = new Problem
                {
                    Status = badRequestObjectResult.StatusCode,
                    Title = "One or more errors occured during model binding.",
                };

                foreach (var item in serializableError)
                {
                    problem.AdditionalProperties[item.Key] = item.Value;
                }

                context.Result = new ObjectResult(problem)
                {
                    StatusCode = badRequestObjectResult.StatusCode,
                };
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}