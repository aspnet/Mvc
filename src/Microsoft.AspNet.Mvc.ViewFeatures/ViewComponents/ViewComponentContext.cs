// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewFeatures;

namespace Microsoft.AspNet.Mvc.ViewComponents
{
    /// <summary>
    /// A context for View Components.
    /// </summary>
    public class ViewComponentContext
    {
        /// <summary>
        /// Creates a new <see cref="ViewComponentContext"/>.
        /// </summary>
        /// <remarks>
        /// The default constructor is provided for unit test purposes only.
        /// </remarks>
        public ViewComponentContext()
        {
            ViewComponentDescriptor = new ViewComponentDescriptor();
            ViewContext = new ViewContext();
        }

        /// <summary>
        /// Creates a new <see cref="ViewComponentContext"/>.
        /// </summary>
        /// <param name="viewComponentDescriptor">
        /// The <see cref="ViewComponentContext"/> for the View Component being invoked.
        /// </param>
        /// <param name="arguments">The View Component arguments.</param>
        /// <param name="viewContext">The <see cref="ViewContext"/>.</param>
        /// <param name="writer">The <see cref="TextWriter"/> for writing output.</param>
        public ViewComponentContext(
            ViewComponentDescriptor viewComponentDescriptor,
            IDictionary<string, object> arguments,
            HtmlEncoder htmlEncoder,
            ViewContext viewContext,
            TextWriter writer)
        {
            if (viewComponentDescriptor == null)
            {
                throw new ArgumentNullException(nameof(viewComponentDescriptor));
            }

            if (htmlEncoder == null)
            {
                throw new ArgumentNullException(nameof(htmlEncoder));
            }

            if (viewContext == null)
            {
                throw new ArgumentNullException(nameof(viewContext));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            ViewComponentDescriptor = viewComponentDescriptor;
            Arguments = arguments;
            HtmlEncoder = htmlEncoder;

            // We want to create a defensive copy of the VDD here so that changes done in the VC
            // aren't visible in the calling view.
            ViewContext = new ViewContext(
                viewContext,
                viewContext.View,
                new ViewDataDictionary(viewContext.ViewData),
                writer);
        }

        /// <summary>
        /// Gets or sets the View Component arguments.
        /// </summary>
        /// <remarks>
        /// The property setter is provided for unit test purposes only.
        /// </remarks>
        public IDictionary<string, object> Arguments { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Text.Encodings.Web.HtmlEncoder"/>.
        /// </summary>
        /// <remarks>
        /// The property setter is provided for unit test purposes only.
        /// </remarks>
        public HtmlEncoder HtmlEncoder { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ViewComponents.ViewComponentDescriptor"/> for the View Component being invoked.
        /// </summary>
        /// <remarks>
        /// The property setter is provided for unit test purposes only.
        /// </remarks>
        public ViewComponentDescriptor ViewComponentDescriptor { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rendering.ViewContext"/>.
        /// </summary>
        /// <remarks>
        /// The property setter is provided for unit test purposes only.
        /// </remarks>
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Gets the <see cref="ViewDataDictionary"/>.
        /// </summary>
        /// <remarks>
        /// This is an alias for <c>ViewContext.ViewData</c>.
        /// </remarks>
        public ViewDataDictionary ViewData => ViewContext.ViewData;

        /// <summary>
        /// Gets the <see cref="TextWriter"/> for output.
        /// </summary>
        /// <remarks>
        /// This is an alias for <c>ViewContext.Writer</c>.
        /// </remarks>
        public TextWriter Writer => ViewContext.Writer;
    }
}
