﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Razor.TagHelpers
{
    /// <summary>
    /// A <see cref="TagHelperComponentTagHelper"/> targeting &lt;head&gt; HTML element.
    /// </summary>
    [HtmlTargetElement("head")]
    public class HeadTagHelper : TagHelperComponentTagHelper
    {
        /// <summary>
        /// Creates a new <see cref="HeadTagHelper"/>.
        /// </summary>
        /// <param name="components">The list of <see cref="ITagHelperComponent"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public HeadTagHelper(IEnumerable<ITagHelperComponent> components, ILoggerFactory loggerFactory)
            : base(components, loggerFactory)
        {
        }
    }
}
