using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public static class PartialViewResultLoggerExtensions
    {
        private static Action<ILogger, string, string, Exception> _partialViewResultExecuted;

        static PartialViewResultLoggerExtensions()
        {
            _partialViewResultExecuted = LoggerMessage.Define<string, string>(LogLevel.Information, 1, "PartialViewResult for action {ActionName} executed. ViewName was {ViewName}.");
        }

        public static void PartialViewResultExecuted(this ILogger logger, ActionContext context,
            string viewName)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            _partialViewResultExecuted(logger, actionName, viewName, null);
        }
    }
}
