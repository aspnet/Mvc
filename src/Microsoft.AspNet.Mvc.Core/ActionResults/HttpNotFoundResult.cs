// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Represents an action result that when executed will produce a 404 response.
    /// </summary>
    public class HttpNotFoundResult : HttpStatusCodeResult
    {
        /// <summary>
        /// Creates a new <see cref="HttpNotFoundResult"/> instance.
        /// </summary>
	    public HttpNotFoundResult() :this(statusDescription: null)
	    {
	    }

        /// <summary>
        /// Creates a new <see cref="HttpNotFoundResult"/> instance
        /// with the given <paramref name="statusDescription"/> message.
        /// </summary>
        /// <param name="statusDescription">
        /// The error message that will be returned in the body of the response.
        /// </param>
        public HttpNotFoundResult(string statusDescription) : base(404, statusDescription)
        {
        }
    }
}