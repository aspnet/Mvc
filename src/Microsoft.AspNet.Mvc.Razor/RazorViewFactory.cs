// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents the default <see cref="IRazorViewFactory"/> implementation that creates
    /// <see cref="RazorView"/> instances with a given <see cref="IRazorPage"/>.
    /// </summary>
    public class RazorViewFactory : IRazorViewFactory
    {
        private readonly IRazorPageActivator _pageActivator;
        private readonly IRazorPageFactory _pageFactory;
        private readonly IViewStartProvider _viewStartProvider;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of RazorViewFactory
        /// </summary>
        /// <param name="pageFactory">The page factory used to instantiate layout and _ViewStart pages.</param>
        /// <param name="pageActivator">The <see cref="IRazorPageActivator"/> used to activate pages.</param>
        /// <param name="viewStartProvider">The <see cref="IViewStartProvider"/> used for discovery of _ViewStart
        /// pages</param>
        public RazorViewFactory(IRazorPageFactory pageFactory,
                                IRazorPageActivator pageActivator,
                                IViewStartProvider viewStartProvider,
                                ILoggerFactory loggerFactory)
        {
            _pageFactory = pageFactory;
            _pageActivator = pageActivator;
            _viewStartProvider = viewStartProvider;
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public IView GetView([NotNull] IRazorPage page, bool isPartial)
        {
            var razorView = new RazorView(_pageFactory, _pageActivator, _viewStartProvider, _loggerFactory, page, isPartial);
            return razorView;
        }
    }
}