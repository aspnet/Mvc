// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml.Internal
{
    internal static class MvcXmlLoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception> _noExecutor;
        private static readonly Action<ILogger, IOutputFormatter, string, Exception> _formatterSelected;
        private static readonly Action<ILogger, string, Exception> _xmlResultExecuting;

        static MvcXmlLoggerExtensions()
        {
            _noExecutor = LoggerMessage.Define<string>
                                     (LogLevel.Warning, 1, "No output formatter was found for content type '{ContentType}' to write the response.");

            _formatterSelected = LoggerMessage.Define<IOutputFormatter, string>
                                    (LogLevel.Debug, 2, "Selected output formatter '{OutputFormatter}' and content type '{ContentType}' to write the response.");

            _xmlResultExecuting = LoggerMessage.Define<string>
                                    (LogLevel.Information, 3, "Executing XmlResult, writing value {Value}.");
        }


        public static void NoExecutor(this ILogger logger, OutputFormatterWriteContext formatterContext)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                _noExecutor(logger, Convert.ToString(formatterContext.ContentType), null);
            }
        }


        public static void FormatterSelected(this ILogger logger, IOutputFormatter outputFormatter, OutputFormatterWriteContext context)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                var contentType = Convert.ToString(context.ContentType);
                _formatterSelected(logger, outputFormatter, contentType, null);
            }
        }

        public static void XmlResultExecuting(this ILogger logger, object value)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _xmlResultExecuting(logger, Convert.ToString(value), null);
            }
        }
    }
}
