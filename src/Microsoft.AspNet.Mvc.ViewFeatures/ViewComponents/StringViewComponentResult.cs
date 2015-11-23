// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Internal;

namespace Microsoft.AspNet.Mvc.ViewComponents
{
    /// <summary>
    /// An <see cref="IViewComponentResult"/> which writes text when executed.
    /// </summary>
    /// <remarks>
    /// The provided content will be HTML-encoded when written. To write pre-encoded content, use an
    /// <see cref="HtmlContentViewComponentResult"/>.
    /// </remarks>
    public class StringViewComponentResult : IViewComponentResult
    {
        /// <summary>
        /// Initializes a new <see cref="StringViewComponentResult"/>.
        /// </summary>
        /// <param name="content">Content to write. The content will be HTML encoded when written.</param>
        public StringViewComponentResult(string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            Content = content;
        }

        /// <summary>
        /// Gets the content.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Encodes and writes the <see cref="Content"/>.
        /// </summary>
        /// <param name="context">The <see cref="ViewComponentContext"/>.</param>
        public void Execute(ViewComponentContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.HtmlEncoder.Encode(context.Writer, Content);
        }

        /// <summary>
        /// Encodes and writes the <see cref="Content"/>.
        /// </summary>
        /// <param name="context">The <see cref="ViewComponentContext"/>.</param>
        /// <returns>A completed <see cref="Task"/>.</returns>
        public Task ExecuteAsync(ViewComponentContext context)
        {
            Execute(context);

            return TaskCache.CompletedTask;
        }
    }
}
