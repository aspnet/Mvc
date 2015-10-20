using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public static class HttpStatusCodeLoggerExtensions
    {
        private static Action<ILogger, string, int, Exception> _resultCreated;

        static HttpStatusCodeLoggerExtensions()
        {
            _resultCreated = LoggerMessage.Define<string, int>(LogLevel.Information, 1, "HttpStatusCodeResult executed for action {ActionName} and status {StatusCode}");
        }

        public static void HttpStatusCodeResultExecuted(this ILogger logger, ActionContext actionContext, int statusCode)
        {
            var actionName = actionContext.ActionDescriptor.DisplayName;
            _resultCreated(logger, actionName, statusCode, null);
        }
    }
}
