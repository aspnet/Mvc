﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Identifies an action that only supports the HTTP HEAD method.
    /// </summary>
    public class HttpHeadAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> _supportedMethods = new string[] { "HEAD" };

        /// <summary>
        /// Creates a new <see cref="HttpHeadAttribute"/>.
        /// </summary>
        public HttpHeadAttribute()
            : base(_supportedMethods)
        {
        }

        /// <summary>
        /// Creates a new <see cref="HttpHeadAttribute"/> with the given route template.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        public HttpHeadAttribute([NotNull] string template)
            : base(_supportedMethods, template)
        {
        }
    }
}