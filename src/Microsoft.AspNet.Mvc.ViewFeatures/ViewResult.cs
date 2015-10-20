// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ViewEngines;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc
{
    internal static class ViewResultLoggerExtensions
    {
        private static Action<ILogger, string, string, Exception> _resultCreated;

        static ViewResultLoggerExtensions()
        {
            _resultCreated = LoggerMessage.Define<string, string>(
                LogLevel.Information,
                3, "ViewResult executed for action {ActionName} got view at path {ViewPath}");
        }

        public static void ViewResultExecuted(this ILogger logger,
            ActionContext actionContext, IView view, Exception exception = null)
        {
            var actionName = actionContext.ActionDescriptor.DisplayName;
            var viewPath = view.Path;
            _resultCreated(logger, actionName, viewPath, exception);
        }
    }


    /// <summary>
    /// Represents an <see cref="ActionResult"/> that renders a view to the response.
    /// </summary>
    public class ViewResult : ActionResult
    {
        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the view to render.
        /// </summary>
        /// <remarks>
        /// When <c>null</c>, defaults to <see cref="Abstractions.ActionDescriptor.Name"/>.
        /// </remarks>
        public string ViewName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ViewDataDictionary"/> for this result.
        /// </summary>
        public ViewDataDictionary ViewData { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITempDataDictionary"/> for this result.
        /// </summary>
        public ITempDataDictionary TempData { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IViewEngine"/> used to locate views.
        /// </summary>
        /// <remarks>When <c>null</c>, an instance of <see cref="ICompositeViewEngine"/> from
        /// <c>ActionContext.HttpContext.RequestServices</c> is used.</remarks>
        public IViewEngine ViewEngine { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MediaTypeHeaderValue"/> representing the Content-Type header of the response.
        /// </summary>
        public MediaTypeHeaderValue ContentType { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var services = context.HttpContext.RequestServices;
            var executor = services.GetRequiredService<ViewResultExecutor>();

            var logFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<ViewResult>();

            var result = executor.FindView(context, this);
            result.EnsureSuccessful();

            var view = result.View;
            using (view as IDisposable)
            {
                await executor.ExecuteAsync(context, view, this);

                logger.ViewResultExecuted(context, view);
            }
        }
    }
}
