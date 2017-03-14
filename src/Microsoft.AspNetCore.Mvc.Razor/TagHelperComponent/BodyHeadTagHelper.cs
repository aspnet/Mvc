// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Razor.TagHelperComponent
{
    /// <summary>
    /// A <see cref="TagHelperComponentTagHelper"/> targeting &lt;body&gt; and &lt;head&gt; elements.
    /// </summary>
    [HtmlTargetElement("body")]
    [HtmlTargetElement("head")]
    public class BodyHeadTagHelper : TagHelperComponentTagHelper
    {
        /// <summary>
        /// Creates a new <see cref="BodyHeadTagHelper"/>.
        /// </summary>
        /// <param name="components">The list of <see cref="ITagHelperComponent"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public BodyHeadTagHelper(IEnumerable<ITagHelperComponent> components, ILoggerFactory loggerFactory)
            : base(components, loggerFactory)
        {
        }
    }
}
