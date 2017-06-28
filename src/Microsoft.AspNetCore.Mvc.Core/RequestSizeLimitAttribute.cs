// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// This attribute allows setting the maximum request body size limit to the specified value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequestSizeLimitAttribute : Attribute, IOrderedFilter
    {
        /// <summary>
        /// The request body size limit specified using the attribute.
        /// </summary>
        public long Bytes { get; set; }

        /// <inheritdoc />
        public int Order { get; set; }

        /// <inheritdoc />
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var filter = serviceProvider.GetRequiredService<RequestSizeLimitResourceFilter>();
            filter.Bytes = Bytes;
            return filter;
        }
    }
}
