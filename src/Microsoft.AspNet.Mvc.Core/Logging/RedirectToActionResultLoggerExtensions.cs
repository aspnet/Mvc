using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public static class RedirectToRouteResultLoggerExtensions
    {
        private static Action<ILogger, string, string, Exception> _redirectToRouteResult;

        static RedirectToRouteResultLoggerExtensions()
        {
            _redirectToRouteResult = LoggerMessage.Define<string, string>(LogLevel.Information, 1, "RedirectToRouteResult for action {ActionName} executed. The destination was {Destination}");
        }

        public static void RedirectToRouteResultExecuted(this ILogger logger, ActionContext context, string destination)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            _redirectToRouteResult(logger, actionName, destination, null);
        }
    }
}
