// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc
{
    internal static class RedirectToRouteResultLoggerExtensions
    {
        private static Action<ILogger, string, string, Exception> _resultExecuted;

        static RedirectToRouteResultLoggerExtensions()
        {
            _resultExecuted = LoggerMessage.Define<string, string>(LogLevel.Information, 11,
                "RedirectToRouteResult for action {ActionName} executed. The destination was {Destination}");
        }

        public static void RedirectToRouteResultExecuted(this ILogger logger, ActionContext context,
            string destination, Exception exception = null)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            _resultExecuted(logger, actionName, destination, exception);
        }
    }

    public class RedirectToRouteResult : ActionResult, IKeepTempDataResult
    {
        public RedirectToRouteResult(object routeValues)
            : this(routeName: null, routeValues: routeValues)
        {
        }

        public RedirectToRouteResult(
            string routeName,
            object routeValues)
            : this(routeName, routeValues, permanent: false)
        {
        }

        public RedirectToRouteResult(
            string routeName,
            object routeValues,
            bool permanent)
        {
            RouteName = routeName;
            RouteValues = PropertyHelper.ObjectToDictionary(routeValues);
            Permanent = permanent;
        }

        public IUrlHelper UrlHelper { get; set; }

        public string RouteName { get; set; }

        public IDictionary<string, object> RouteValues { get; set; }

        public bool Permanent { get; set; }

        public override void ExecuteResult(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var urlHelper = GetUrlHelper(context);

            var destinationUrl = urlHelper.RouteUrl(RouteName, RouteValues);
            if (string.IsNullOrEmpty(destinationUrl))
            {
                throw new InvalidOperationException(Resources.NoRoutesMatched);
            }

            context.HttpContext.Response.Redirect(destinationUrl, Permanent);

            var logFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<RedirectToRouteResult>();

            logger.RedirectToRouteResultExecuted(context, destinationUrl);
        }

        private IUrlHelper GetUrlHelper(ActionContext context)
        {
            return UrlHelper ?? context.HttpContext.RequestServices.GetRequiredService<IUrlHelper>();
        }
    }
}
