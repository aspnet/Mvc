// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.ErrorDescription;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ProblemDescriptionFilter : IResultFilter
    {
        private readonly IErrorDescriptionFactory _factory;

        public ProblemDescriptionFilter(IErrorDescriptionFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (!(context.Result is StatusCodeResult statusCodeResult))
            {
                return;
            }

            var statusCode = statusCodeResult.StatusCode;
            if (statusCode == StatusCodes.Status400BadRequest)
            {
                var problem = _factory.CreateErrorDescription(context.ActionDescriptor, new ProblemDescription
                {
                    Status = statusCodeResult.StatusCode,
                    Title = Resources.ProblemDescription_400_Title,
                });

                context.Result = new BadRequestObjectResult(problem)
                {
                    StatusCode = statusCodeResult.StatusCode,
                };
            }
            else if (statusCode == StatusCodes.Status404NotFound && BindsIdParameter())
            {
                var problem = _factory.CreateErrorDescription(context.ActionDescriptor, new ProblemDescription
                {
                    Status = statusCodeResult.StatusCode,
                    Title = Resources.ProblemDescription_404_Title,
                });

                context.Result = new BadRequestObjectResult(problem)
                {
                    StatusCode = statusCodeResult.StatusCode,
                };
            }

            bool BindsIdParameter()
            {
                for (var i = 0; i < context.ActionDescriptor.Parameters.Count; i++)
                {
                    var parameter = context.ActionDescriptor.Parameters[i];
                    if (string.Equals(parameter.Name, "id", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                for (var i = 0; i < context.ActionDescriptor.BoundProperties.Count; i++)
                {
                    var property = context.ActionDescriptor.BoundProperties[i];
                    if (string.Equals(property.Name, "id", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}
