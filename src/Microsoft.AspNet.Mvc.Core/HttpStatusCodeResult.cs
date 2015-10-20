// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc
{
    internal static class HttpStatusCodeLoggerExtensions
    {
        private static Action<ILogger, string, int, Exception> _resultCreated;

        static HttpStatusCodeLoggerExtensions()
        {
            _resultCreated = LoggerMessage.Define<string, int>(
                LogLevel.Information,
                3, "HttpStatusCodeResult executed for action {ActionName} and status {StatusCode}");
        }

        public static void HttpStatusCodeResultExecuted(this ILogger logger,
            ActionContext actionContext, int statusCode, Exception exception = null)
        {
            var actionName = actionContext.ActionDescriptor.DisplayName;
            _resultCreated(logger, actionName, statusCode, exception);
        }
    }

    /// <summary>
    /// Represents an <see cref="ActionResult"/> that when executed will
    /// produce an HTTP response with the given response status code.
    /// </summary>
    public class HttpStatusCodeResult : ActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpStatusCodeResult"/> class
        /// with the given <paramref name="statusCode"/>.
        /// </summary>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        public HttpStatusCodeResult(int statusCode)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the HTTP status code.
        /// </summary>
        public int StatusCode { get; private set; }

        /// <inheritdoc />
        public override void ExecuteResult(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.HttpContext.Response.StatusCode = StatusCode;

            var factory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = factory.CreateLogger<HttpStatusCodeResult>();

            logger.HttpStatusCodeResultExecuted(context, StatusCode);
        }
    }
}
