using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Mvc.Logging
{
    public static class ContentResultLoggerExtensions
    {
        private static Action<ILogger, string, string, Exception> _contentResultExecuted;

        static ContentResultLoggerExtensions()
        {
            _contentResultExecuted = LoggerMessage.Define<string, string>(LogLevel.Information, 1, "ContentResult for action {ActionName} executed, had ContentType of {ContentType}");
        }

        public static void ContentResultExecuted(this ILogger logger, ActionContext context,
            MediaTypeHeaderValue contentType)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            _contentResultExecuted(logger, actionName, contentType.MediaType, null);
        }
    }
}
