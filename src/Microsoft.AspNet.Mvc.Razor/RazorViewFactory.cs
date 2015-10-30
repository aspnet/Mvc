// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.AspNet.Mvc.ViewEngines;
using Microsoft.Extensions.WebEncoders;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents the default <see cref="IRazorViewFactory"/> implementation that creates
    /// <see cref="RazorView"/> instances with a given <see cref="IRazorPage"/>.
    /// </summary>
    public class RazorViewFactory : IRazorViewFactory
    {
        private readonly IHtmlEncoder _htmlEncoder;
        private readonly IRazorPageActivator _pageActivator;
        private readonly IViewStartProvider _viewStartProvider;
        private readonly DiagnosticSource _diagnosticSource;

        /// <summary>
        /// Initializes a new instance of RazorViewFactory
        /// </summary>
        /// <param name="pageActivator">The <see cref="IRazorPageActivator"/> used to activate pages.</param>
        /// <param name="viewStartProvider">The <see cref="IViewStartProvider"/> used for discovery of _ViewStart
        /// pages</param>
        public RazorViewFactory(
            IRazorPageActivator pageActivator,
            IViewStartProvider viewStartProvider,
            IHtmlEncoder htmlEncoder,
            DiagnosticSource diagnosticSource)
        {
            _pageActivator = pageActivator;
            _viewStartProvider = viewStartProvider;
            _htmlEncoder = htmlEncoder;
            _diagnosticSource = diagnosticSource;
        }

        /// <inheritdoc />
        public IView GetView(
            IRazorViewEngine viewEngine,
            IRazorPage page,
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
                _viewStartProvider,
                page,
                _htmlEncoder,
                _diagnosticSource,
                isPartial);
            return razorView;
        }
    }
}