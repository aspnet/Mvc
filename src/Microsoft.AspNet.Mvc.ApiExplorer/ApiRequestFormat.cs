// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc.ApiExplorer
{
    /// <summary>
    /// A possible format for the body of a request.
    /// </summary>
    public class ApiRequestFormat
    {
        /// <summary>
        /// The formatter used to read this request.
        /// </summary>
        public IInputFormatter Formatter { get; set; }

        /// <summary>
        /// The media type of the request.
        /// </summary>
        public MediaTypeHeaderValue MediaType { get; set; }
    }
}