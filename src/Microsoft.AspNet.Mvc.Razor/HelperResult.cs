// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Html.Abstractions;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents a deferred write operation in a <see cref="RazorPage"/>.
    /// </summary>
    public class HelperResult
    {
        /// <summary>
        /// Creates a new instance of <see cref="HelperResult"/>.
        /// </summary>
        /// <param name="renderAction">The asynchronous delegate to invoke when
        /// <see cref="WriteTo(TextWriter, HtmlEncoder)"/> is called.</param>
        /// <remarks>Calls to <see cref="WriteTo(TextWriter, HtmlEncoder)"/> result in a blocking invocation of
        /// <paramref name="renderAction"/>.</remarks>
        public HelperResult(RenderAsyncDelegate renderAction)
        {
            if (renderAction == null)
            {
                throw new ArgumentNullException(nameof(renderAction));
            }

            RenderAction = renderAction;
        }

        /// <summary>
        /// Gets the asynchronous delegate to invoke when <see cref="WriteTo(TextWriter, HtmlEncoder)"/> is called.
        /// </summary>
        public RenderAsyncDelegate RenderAction { get; }

        public void WriteTo(IHtmlContentBuilder content)
        {
            RenderAction(content).GetAwaiter().GetResult();
        }
    }
}
