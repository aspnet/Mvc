// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using Microsoft.AspNet.Mvc.ViewEngines;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents the default <see cref="IRazorViewFactory"/> implementation that creates
    /// <see cref="RazorView"/> instances with a given <see cref="IRazorPage"/>.
    /// </summary>
    public class RazorViewFactory : IRazorViewFactory
    {
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IRazorPageActivator _pageActivator;

        /// <summary>
        /// Initializes a new instance of RazorViewFactory
        /// </summary>
        /// <param name="pageActivator">The <see cref="IRazorPageActivator"/> used to activate pages.</param>
        /// <param name="viewStartProvider">The <see cref="IViewStartProvider"/> used for discovery of _ViewStart
        /// pages</param>
        public RazorViewFactory(
            IRazorPageActivator pageActivator,
            HtmlEncoder htmlEncoder)
        {
            _pageActivator = pageActivator;
            _htmlEncoder = htmlEncoder;
        }

        /// <inheritdoc />
        public IView GetView(
            IRazorViewEngine viewEngine,
            IRazorPage page,
            IReadOnlyList<IRazorPage> viewStartPages,
            bool isPartial)
        {
            if (viewEngine == null)
            {
                throw new ArgumentNullException(nameof(viewEngine));
            }

            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var razorView = new RazorView(
                viewEngine,
                _pageActivator,
                viewStartPages,
                page,
                _htmlEncoder,
                isPartial);
            return razorView;
        }
    }
}