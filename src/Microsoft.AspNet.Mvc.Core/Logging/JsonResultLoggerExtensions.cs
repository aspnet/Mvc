using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public static class JsonResultLoggerExtensions
    {
        private static Action<ILogger, string, Exception> _jsonResultExecuted;

        static JsonResultLoggerExtensions()
        {
            _jsonResultExecuted = LoggerMessage.Define<string>(LogLevel.Information, 1, "JsonResult for action {ActionName} executed.");
        }

        public static void JsonResultExecuted(this ILogger logger, ActionContext context)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            _jsonResultExecuted(logger, actionName, null);
        }
    }
}
