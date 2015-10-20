using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ViewEngines;

namespace Microsoft.AspNet.Mvc.Logging
{
    public static class ViewResultLoggerExtensions
    {
        private static Action<ILogger, string, string, Exception> _resultCreated;

        static ViewResultLoggerExtensions()
        {
            _resultCreated = LoggerMessage.Define<string, string>(LogLevel.Information, 1, "ViewResult executed for action {ActionName} got view at path {ViewPath}");
        }

        public static void ViewResultExecuted(this ILogger logger, ActionContext actionContext, IView view)
        {
            var actionName = actionContext.ActionDescriptor.DisplayName;
            var viewPath = view.Path;
            _resultCreated(logger, actionName, viewPath, null);
        }
    }
}
