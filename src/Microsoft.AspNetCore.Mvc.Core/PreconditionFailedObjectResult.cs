// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// An <see cref="ObjectResult"/> that when executed performs content negotiation, formats the entity body, and
    /// will produce a <see cref="StatusCodes.Status412PreconditionFailed"/> response if negotiation and formatting succeed.
    /// </summary>
    public class PreconditionFailedObjectResult : ObjectResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreconditionFailedObjectResult"/> class.
        /// </summary>
        /// <param name="value">The content to format into the entity body.</param>
        public PreconditionFailedObjectResult(object value)
            : base(value)
        {
            this.StatusCode = StatusCodes.Status412PreconditionFailed;
        }
    }
}