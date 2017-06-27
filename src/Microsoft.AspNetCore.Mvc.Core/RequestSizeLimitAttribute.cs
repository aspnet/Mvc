// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc.Core
{
    /// <summary>
    /// A filter that specifies the request body size limit.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequestSizeLimitAttribute : Attribute, IResourceFilter
    {
        /// <summary>
        /// Initializes an instance of <see cref="RequestSizeLimitAttribute"/>.
        /// </summary>
        /// <param name="bytes">The maximum request body size accepted by the application.</param>
        /// <param name="disabled">Indicates whether this attribute is disabled.</param>
        public RequestSizeLimitAttribute(long bytes, bool disabled)
        {
            if (bytes < 0)
            {
                Disabled = true;
            }

            else
            {
                Bytes = bytes;
                Disabled = disabled;
            }
        }

        /// <summary>
        /// The request body size limit specified using <see cref="RequestSizeLimitAttribute"/>.
        /// </summary>
        public long Bytes { get; }

        /// <summary>
        /// Indicates if <see cref="RequestSizeLimitAttribute"/> is disabled.
        /// </summary>
        public bool Disabled { get; }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {

        }

        /// <summary>
        /// As an <see cref="IResourceFilter"/>, this filter looks at the request and rejects it before going ahead if
        /// the request body size is greater than the specified limit.
        /// </summary>
        /// <param name="context">The <see cref="ResourceExecutingContext"/>.</param>
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (Disabled == false)
            {
                var requestLength = context.HttpContext.Request.Body.Length;
                if (requestLength > Bytes)
                {
                    throw new InvalidOperationException(Resources.FormatRequestBodySize_GreaterThanLimit(requestLength, Bytes));
                }
            }
        }
    }
}
