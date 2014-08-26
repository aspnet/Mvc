// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Represents an <see cref="HttpStatusCodeResult"/> that when
    /// executed will produce a Bad Request (400) response.
    /// </summary>
    public class HttpBadRequestResult : HttpStatusCodeResult
    {
        /// <summary>
        /// Creates a new <see cref="HttpBadRequestResult"/> instance.
        /// </summary>
        public HttpBadRequestResult() : base(400)
        {
        }
    }
}