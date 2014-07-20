// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Represents an action result that when executed will produce an HTTP response
    /// with the given response status code and description.
    /// </summary>
    public class HttpStatusCodeResult : ActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpStatusCodeResult"/> class
        /// with the given <paramref name="statusCode"/>.
        /// </summary>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        public HttpStatusCodeResult(int statusCode)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the<see cref="HttpStatusCodeResult"/> class
        /// with the given <paramref name="statusCode"/> and <paramref name="statusDescription"/>.
        /// </summary>
        /// <param name="statusCode">The status code of the response.</param>
        /// <param name="statusDescription">The status description of the response.</param>
        public HttpStatusCodeResult(int statusCode, string statusDescription)
        {
            StatusCode = statusCode;
            StatusDescription = statusDescription;
        }

        /// <summary>
        /// Gets the HTTP status code.
        /// </summary>
        public int StatusCode { get; private set; }

        /// <summary>
        /// Gets the HTTP status description.
        /// </summary>
        public string StatusDescription { get; private set; }

        /// <inheritdoc />
        public async override Task ExecuteResultAsync([NotNull] ActionContext context)
        {
            var response = context.HttpContext.Response;

            response.StatusCode = StatusCode;
            if (StatusDescription != null)
            {
                response.ContentType = "text/plain; charset=utf-8";
                await response.WriteAsync(StatusDescription);
            }
        }
    }
}
