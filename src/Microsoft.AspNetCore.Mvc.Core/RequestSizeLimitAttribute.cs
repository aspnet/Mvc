// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// A filter that specifies the request body size limit.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequestSizeLimitAttribute : Attribute, IRequestSizePolicy, IOrderedFilter, IResourceFilter
    {
        private readonly long _bytes;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a new instance of <see cref="RequestSizeLimitAttribute"/>.
        /// </summary>
        public RequestSizeLimitAttribute(long bytes, ILoggerFactory loggerFactory)
        {
            _bytes = bytes;
            if (loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger<RequestSizeLimitAttribute>();
            }
        }

        /// <inheritdoc />
        public int Order { get; set; }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {

        }

        /// <summary>
        /// As an <see cref="IResourceFilter"/>, this filter sets the <see cref="IHttpMaxRequestBodySizeFeature.MaxRequestBodySize"/>
        /// to the specified size.
        /// </summary>
        /// <param name="context">The <see cref="ResourceExecutingContext"/>.</param>
        /// <remarks>If <see cref="IHttpMaxRequestBodySizeFeature"/> is not enabled or is read-only, the attribute is not applied.</remarks>
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (IsClosestRequestSizePolicy(context.Filters))
            {
                var maxRequestBodySizeFeature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();

                if (maxRequestBodySizeFeature == null)
                {
                    _logger.FeatureNotFound(nameof(RequestSizeLimitAttribute), nameof(IHttpMaxRequestBodySizeFeature));
                }
                else if (maxRequestBodySizeFeature.IsReadOnly)
                {
                    _logger.FeatureIsReadOnly(nameof(IHttpMaxRequestBodySizeFeature));
                }
                else
                {
                    maxRequestBodySizeFeature.MaxRequestBodySize = _bytes;
                    _logger.MaxRequestBodySizeSet(_bytes.ToString());
                }
            }
        }

        private bool IsClosestRequestSizePolicy(IList<IFilterMetadata> filters)
        {
            // Determine if this instance is the 'effective' request size policy.
            for (var i = filters.Count - 1; i >= 0; i--)
            {
                var filter = filters[i];
                if (filter is IRequestSizePolicy)
                {
                    return ReferenceEquals(this, filter);
                }
            }

            Debug.Fail("The current instance should be in the list of filters.");
            return false;
        }
    }
}
