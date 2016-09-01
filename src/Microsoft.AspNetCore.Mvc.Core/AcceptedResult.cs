// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// An <see cref="StatusCodeResult"/>  that when executed will produce an empty
    /// <see cref="StatusCodes.Status202Accepted"/> response.
    /// </summary>
    public class AcceptedResult : StatusCodeResult
    {
        private readonly string _location;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptedResult"/> class.
        /// </summary>
        /// <param name="location">An optional location to put in the response header.</param>
        public AcceptedResult(string location = null)
            : base(StatusCodes.Status202Accepted)
        {
            this._location = location;
        }

        /// <inheritdoc />
        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (!string.IsNullOrEmpty(this._location))
            {
                context.HttpContext.Response.Headers.Add(HeaderNames.Location, new StringValues(this._location));
            }

            return base.ExecuteResultAsync(context);
        }
    }
}