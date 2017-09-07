// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class ProblemAttributeApiDescriptionProvider : IApiDescriptionProvider
    {
        private readonly IModelMetadataProvider _modelMetadaProvider;

        public ProblemAttributeApiDescriptionProvider(IModelMetadataProvider modelMetadataProvider)
        {
            _modelMetadaProvider = modelMetadataProvider;
        }

        /// <remarks>
        /// The order is set to execute after the <see cref="DefaultApiDescriptionProvider"/>.
        /// </remarks>
        public int Order => -1000 + 10;

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
        }

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            foreach (var apiDescription in context.Results)
            {
                if (!apiDescription.ActionDescriptor.Properties.ContainsKey(ProblemDescriptionApplicationModelProvider.ProblemDescriptionAttributeKey))
                {
                    continue;
                }

                var parameters = apiDescription.ActionDescriptor.Parameters.Concat(apiDescription.ActionDescriptor.BoundProperties);
                if (parameters.Any())
                {
                    apiDescription.SupportedResponseTypes.Add(CreateProblemResponse(StatusCodes.Status400BadRequest));

                    if (parameters.Any(p => string.Equals("id", p.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        apiDescription.SupportedResponseTypes.Add(CreateProblemResponse(StatusCodes.Status404NotFound));
                    }
                }

                apiDescription.SupportedResponseTypes.Add(CreateProblemResponse(statusCode: 0));
            }
        }

        private ApiResponseType CreateProblemResponse(int statusCode)
        {
            return new ApiResponseType
            {
                ApiResponseFormats = new List<ApiResponseFormat>
                    {
                        new ApiResponseFormat
                        {
                            MediaType = "application/problem+json",
                        },
                        new ApiResponseFormat
                        {
                            MediaType = "application/problem+xml",
                        },
                    },
                ModelMetadata = _modelMetadaProvider.GetMetadataForType(typeof(ProblemDescription)),
                StatusCode = statusCode,
                Type = typeof(ProblemDescription),
            };
        }
    }
}
