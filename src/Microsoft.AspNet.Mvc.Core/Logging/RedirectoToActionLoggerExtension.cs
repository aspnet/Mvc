using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public static class RedirectToActionResultLoggerExtensions
    {
        private static Action<ILogger, string, string, Exception> _redirectToActionResult;

        static RedirectToActionResultLoggerExtensions()
        {
            _redirectToActionResult = LoggerMessage.Define<string, string>(LogLevel.Information, 1, "RedirectToActionResult for action {ActionName} executed. The destination was {Destination}");
        }

        public static void RedirectToActionResultExecuted(this ILogger logger, ActionContext context, string destination)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            _redirectToActionResult(logger, actionName, destination, null);
        }
    }
}
