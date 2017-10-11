// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class FindSingleApiDescriptionProvider : DefaultApiDescriptionProfile
    {
        public FindSingleApiDescriptionProvider()
        {
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

        public override string DisplayName => "FindSingle";

        public override bool IsMatch(ApiDescription description)
        {
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            // Example
            //
            // [HttpGet]
            // public ActionResult<IEnumerable<Product>> GetProducts()
            if (string.Equals("GET", description.HttpMethod, StringComparison.OrdinalIgnoreCase) &&
                description.ParameterDescriptions.Count > 0 &&
                description.SupportedResponseTypes.Count == 1 &&
                !description.SupportedResponseTypes[0].ModelMetadata.IsCollectionType)
            {
                return true;
            }

            return false;
        }
    }
}
