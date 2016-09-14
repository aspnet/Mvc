// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// An <see cref="StatusCodeResult"/>  that when executed will produce an empty
    /// <see cref="StatusCodes.Status412PreconditionFailed"/> response.
    /// </summary>
    public class PreconditionFailedResult : StatusCodeResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreconditionFailedResult"/> class.
        /// </summary>
        public PreconditionFailedResult()
            : base(StatusCodes.Status412PreconditionFailed)
        {
        }
    }
}