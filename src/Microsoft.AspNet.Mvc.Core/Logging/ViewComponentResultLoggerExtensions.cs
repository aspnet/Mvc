using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc.Core.Logging
{
    public static class ViewComponentResultLoggerExtensions
    {
        private static Action<ILogger, string, string, Exception> _viewComponentResult;

        static ViewComponentResultLoggerExtensions()
        {
            _viewComponentResult = LoggerMessage.Define<string, string>(LogLevel.Information, 1, "ViewComponentResult for action {ActionName} executed, resulting in ViewComponent with the name {ViewComponentName}");
        }

        public static void ViewComponentResultExecuted(this ILogger logger, ActionContext context, string viewComponentName)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            _viewComponentResult(logger, actionName, viewComponentName, null);
        }
    }
}
