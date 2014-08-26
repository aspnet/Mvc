﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Default implementation for <see cref="IRazorView"/> that executes one or more <see cref="RazorPage"/> 
    /// instances as part of view rendering.
    /// </summary>
    public class RazorView : IRazorView
    {
        private readonly IRazorPageFactory _pageFactory;
        private readonly IRazorPageActivator _pageActivator;
        private readonly IViewStartProvider _viewStartProvider;
        private IRazorPage _razorPage;
        private bool _isPartial;

        /// <summary>
        /// Initializes a new instance of RazorView
        /// </summary>
        /// <param name="pageFactory">The view factory used to instantiate layout and _ViewStart pages.</param>
        /// <param name="pageActivator">The <see cref="IRazorPageActivator"/> used to activate pages.</param>
        /// <param name="viewStartProvider">The <see cref="IViewStartProvider"/> used for discovery of _ViewStart
        /// pages</param>
        public RazorView(IRazorPageFactory pageFactory,
                         IRazorPageActivator pageActivator,
                         IViewStartProvider viewStartProvider)
        {
            _pageFactory = pageFactory;
            _pageActivator = pageActivator;
            _viewStartProvider = viewStartProvider;
        }

        /// <inheritdoc />
        public virtual void Contextualize(IRazorPage razorPage, bool isPartial)
        {
            _razorPage = razorPage;
            _isPartial = isPartial;
        }

        private bool IsBuffered { get; set; } = true;

        /// <inheritdoc />
        public virtual async Task RenderAsync([NotNull] ViewContext context)
        {
            if (_razorPage == null)
            {
                var message = Resources.FormatViewMustBeContextualized(nameof(Contextualize), nameof(RenderAsync));
                throw new InvalidOperationException(message);
            }

            if (!_isPartial)
            {
                var bodyWriter = await RenderPageAsync(_razorPage, context, executeViewStart: true);
                await RenderLayoutAsync(context, bodyWriter);
            }
            else
            {
                await RenderPageCoreAsync(_razorPage, context);
            }
        }

        private async Task<RazorTextWriter> RenderPageAsync(IRazorPage page,
                                                            ViewContext context,
                                                            bool executeViewStart)
        {
            if (IsBuffered)
            {
                using (var bufferedWriter = new RazorTextWriter(context.Writer.Encoding))
                {
                    // The writer for the body is passed through the ViewContext, allowing things like HtmlHelpers
                    // and ViewComponents to reference it.
                    var oldWriter = context.Writer;
                    context.Writer = bufferedWriter;

                    page.FlushPointDelegate = async () =>
                    {
                        if (IsBuffered)
                        {
                            // When FlushAsync is invoked the first time around, we copy the buffered contents into
                            // the original TextWriter that was passed in via the ViewContext, followed by flushing it.
                            // Any further invocations of FlushAsync simply need to pass through the Flush call to the
                            // original TextWriter.
                            IsBuffered = false;
                            context.Writer = oldWriter;
                            await bufferedWriter.CopyToAsync(oldWriter);
                        }
                        await oldWriter.FlushAsync();
                    };

                    try
                    {
                        await RenderPageWithViewStart(page, context, executeViewStart);
                        return bufferedWriter;
                    }
                    finally
                    {
                        context.Writer = oldWriter;
                    }
                }
            }
            else
            {
                page.FlushPointDelegate = context.Writer.FlushAsync;
                await RenderPageWithViewStart(page, context, executeViewStart);
                return null;
            }
        }

        private async Task RenderPageWithViewStart(IRazorPage page,
                                                   ViewContext context,
                                                   bool executeViewStart)
        {
            if (executeViewStart)
            {
                // Execute view starts using the same context + writer as the page to render.
                await RenderViewStartAsync(context);
            }

            await RenderPageCoreAsync(page, context);
        }

        private async Task RenderPageCoreAsync(IRazorPage page, ViewContext context)
        {
            page.ViewContext = context;
            _pageActivator.Activate(page, context);
            await page.ExecuteAsync();
        }

        private async Task RenderViewStartAsync(ViewContext context)
        {
            var viewStarts = _viewStartProvider.GetViewStartPages(_razorPage.Path);

            foreach (var viewStart in viewStarts)
            {
                await RenderPageCoreAsync(viewStart, context);

                // Copy over interesting properties from the ViewStart page to the entry page.
                _razorPage.Layout = viewStart.Layout;
            }
        }

        private async Task RenderLayoutAsync(ViewContext context,
                                             RazorTextWriter bodyWriter)
        {
            // A layout page can specify another layout page. We'll need to continue
            // looking for layout pages until they're no longer specified.
            var previousPage = _razorPage;
            while (!string.IsNullOrEmpty(previousPage.Layout))
            {
                if (!IsBuffered)
                {
                    var message = Resources.FormatLayoutCannotBeRendered("FlushAsync");
                    throw new InvalidOperationException(message);
                }

                var layoutPage = _pageFactory.CreateInstance(previousPage.Layout);
                if (layoutPage == null)
                {
                    var message = Resources.FormatLayoutCannotBeLocated(previousPage.Layout);
                    throw new InvalidOperationException(message);
                }

                layoutPage.PreviousSectionWriters = previousPage.SectionWriters;
                layoutPage.RenderBodyDelegate = bodyWriter.CopyTo;
                bodyWriter = await RenderPageAsync(layoutPage, context, executeViewStart: false);

                // Verify that RenderBody is called, or that RenderSection is called for all sections
                layoutPage.EnsureBodyAndSectionsWereRendered();

                previousPage = layoutPage;
            }

            if (IsBuffered)
            {
                // Only copy buffered content to the Output if we're currently buffering.
                await bodyWriter.CopyToAsync(context.Writer);
            }
        }
    }
}
