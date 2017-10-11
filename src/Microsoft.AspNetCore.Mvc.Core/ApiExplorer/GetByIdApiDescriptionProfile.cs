// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class GetByIdApiDescriptionProfile : DefaultApiDescriptionProfile
    {
        public GetByIdApiDescriptionProfile()
        {
            ResponseTypes.Add(new ApiResponseType()
            {
                Type = typeof(ProblemDetails),
                StatusCode = 404,
            });
            ResponseTypes.Add(new ApiResponseType()
            {
                Type = typeof(ProblemDetails),
                StatusCode = 400,
            });
            ResponseTypes.Add(new ApiResponseType()
            {
                Type = typeof(ProblemDetails),
                IsDefaultResponse = true,
            });
        }

        public override string DisplayName => "GetById";

        public override bool IsMatch(ApiDescription description)
        {
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }
            
            // Example
            //
            // [HttpGet]
            // public ActionResult<Product> GetProduct(int productId)
            if (string.Equals("GET", description.HttpMethod, StringComparison.OrdinalIgnoreCase) &&
                description.ParameterDescriptions.Count == 1 &&
                IdParameter.IsIdParameter(description.ParameterDescriptions[0]) &&
                description.SupportedResponseTypes.Count == 1 &&
                !description.SupportedResponseTypes[0].ModelMetadata.IsCollectionType)
            {
                return true;
            }

            return false;
        }
    }
}
