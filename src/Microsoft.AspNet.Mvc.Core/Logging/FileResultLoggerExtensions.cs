using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public static class FileResultLoggerExtensions
    {
        private static Action<ILogger, string, string, Exception> _fileResultExecuted;

        static FileResultLoggerExtensions()
        {
            _fileResultExecuted = LoggerMessage.Define<string, string>(LogLevel.Information, 1, "FileResult for action {ActionName} executed. File written was named {FileName}");
        }

        public static void FileResultExecuted(this ILogger logger, ActionContext context,
            string fileName)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            _fileResultExecuted(logger, actionName, fileName, null);
        }
    }
}
