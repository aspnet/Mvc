// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public class UnknownApiDescriptionProfile : DefaultApiDescriptionProfile
    {
        public UnknownApiDescriptionProfile()
        {
            ResponseTypes.Add(new ApiResponseType()
            {
                Type = typeof(ProblemDetails),
                IsDefaultResponse = true,
            });
        }

        public override string DisplayName => "Unknown";

        public override bool IsMatch(ApiDescription description)
        {
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            // Example
            //
            // (Anything)
            return true;
        }
    }
}
