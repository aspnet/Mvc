// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Represents an <see cref="ActionResult"/> that renders a view to the response.
    /// </summary>
    public class ViewResult : ActionResult
    {
        /// <summary>
        /// Gets or sets the name of the view to render.
        /// </summary>
        /// <remarks>
        /// When <c>null</c>, defaults to <see cref="ActionDescriptor.Name"/>.
        /// </remarks>
        public string ViewName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ViewDataDictionary"/> for this result.
        /// </summary>
        public ViewDataDictionary ViewData { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IViewEngine"/> used to locate views.
        /// </summary>
        /// <remarks>When <c>null</c>, an instance of <see cref="ICompositeViewEngine"/> from
        /// <c>ActionContext.HttpContext.RequestServices</c> is used.</remarks>
        public IViewEngine ViewEngine { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ILoggerFactory"/> used to create loggers.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteResultAsync([NotNull] ActionContext context)
        {
            var viewEngine = ViewEngine ??
                             context.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();

            var viewName = ViewName ?? context.ActionDescriptor.Name;
            var view = viewEngine.FindView(context, viewName)
                                 .EnsureSuccessful()
                                 .View;

            var loggerFactory = LoggerFactory ??
                         context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.Create<ViewResult>();
            if (logger.IsEnabled(LogLevel.Verbose))
            {
                logger.WriteVerbose(new ViewDataValues(ViewData));
            }

            using (view as IDisposable)
            {
                await ViewExecutor.ExecuteAsync(view, context, ViewData, contentType: null);
            }
        }
    }
}
