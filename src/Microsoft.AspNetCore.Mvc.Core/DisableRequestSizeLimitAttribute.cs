// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc.Core
{
    public class DisableRequestSizeLimitAttribute : Attribute, IRequestSizePolicy, IOrderedFilter, IResourceFilter
    {
        /// <inheritdoc />
        public bool IsReusable => true;

        /// <inheritdoc />
        public int Order { get; set; } = 1000;

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

            if (IsClosestRequestSizePolicy(context.Filters))
            {
                var maxRequestBodySizeFeature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
                maxRequestBodySizeFeature.MaxRequestBodySize = null;
            }
        }

        private bool IsClosestRequestSizePolicy(IList<IFilterMetadata> filters)
        {
            // Determine if this instance is the 'effective' antiforgery policy.
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
