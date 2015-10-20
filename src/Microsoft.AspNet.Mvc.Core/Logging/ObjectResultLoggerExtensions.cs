using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public static class ObjectResultLoggerExtensions
    {
        private static Action<ILogger, string, Exception> _objectResultExecuted;

        static ObjectResultLoggerExtensions()
        {
            _objectResultExecuted = LoggerMessage.Define<string>(LogLevel.Information, 1, "ObjectResult for action {ActionName} executed.");
        }

        public static void ObjectResultExecuted(this ILogger logger, ActionContext context)
        {
            var actionName = context.ActionDescriptor?.DisplayName;
            _objectResultExecuted(logger, actionName, null);
        }
    }
}
