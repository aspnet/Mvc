// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.ApiExplorer
{
    public abstract class DefaultApiDescriptionProfile : ApiDescriptionProfile
    {
        public IList<ApiResponseType> ResponseTypes { get; } = new List<ApiResponseType>();

        public override void ApplyTo(ApiDescription description)
        {
            if (description == null)
            {
                throw new ArgumentNullException(nameof(description));
            }

            for (var i = 0; i < ResponseTypes.Count; i++)
            {
                description.SupportedResponseTypes.Add(ResponseTypes[i]);
            }
        }
    }
}
