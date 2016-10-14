// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// A filter that specifies the status code returned by the action.
    /// </summary>
    public class ProducesResponse: IApiResponseMetadataProvider
    {
        /// <summary>
        /// Initializes an instance of <see cref="ProducesResponse"/>.
        /// </summary>        
        /// <param name="statusCode">The HTTP response status code.</param>
        public ProducesResponse(int statusCode)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets or sets the type of the value returned by an action.
        /// </summary>
        public Type Type => typeof(void);

        /// <summary>
        /// Gets or sets the HTTP status code of the response.
        /// </summary>
        public int StatusCode { get; set; }

        /// <inheritdoc />
        void IApiResponseMetadataProvider.SetContentTypes(MediaTypeCollection contentTypes)
        {
            // Users are supposed to use the 'Produces' attribute to set the content types that an action can support.
        }
    }
}
