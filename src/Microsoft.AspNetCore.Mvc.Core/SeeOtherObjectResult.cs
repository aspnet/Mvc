// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// An <see cref="ObjectResult"/> that when executed performs content negotiation, formats the entity body, and
    /// will produce a <see cref="StatusCodes.Status303SeeOther"/> response if negotiation and formatting succeed.
    /// </summary>
    public class SeeOtherObjectResult : ObjectResult
    {
        private readonly string _location;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeeOtherObjectResult"/> class.
        /// </summary>
        /// <param name="value">The content to format into the entity body.</param>
        /// <param name="location">An optional location to put in the response header.</param>
        public SeeOtherObjectResult(object value, string location = null)
            : base(value)
        {
            this.StatusCode = StatusCodes.Status303SeeOther;
            this._location = location;
        }

        /// <inheritdoc />
        public override void OnFormatting(ActionContext context)
        {
            base.OnFormatting(context);

            if (!string.IsNullOrEmpty(this._location))
            {
                context.HttpContext.Response.Headers.Add(HeaderNames.Location, new StringValues(this._location));
            }
        }
    }
}