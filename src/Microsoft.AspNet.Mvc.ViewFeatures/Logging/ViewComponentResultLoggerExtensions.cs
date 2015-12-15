// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public static class ViewComponentResultLoggerExtensions
    {
        private static readonly Action<ILogger, string, string[], Exception> _viewComponentResultExecuting;

        static ViewComponentResultLoggerExtensions()
        {
            _viewComponentResultExecuting = LoggerMessage.Define<string, string[]>(
                LogLevel.Information,
                1,
                "Executing ViewComponentResult, running {ViewComponentName} with arguments ({Arguments}).");
        }

        public static void ViewComponentResultExecuting(this ILogger logger, string viewComponentName, object arguments)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                var formattedArguments = GetFormattedArguments(arguments);
                _viewComponentResultExecuting(logger, viewComponentName, formattedArguments, null);
            }
        }

        public static void ViewComponentResultExecuting(this ILogger logger, Type viewComponentType, object arguments)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                var formattedArguments = GetFormattedArguments(arguments);
                _viewComponentResultExecuting(logger, viewComponentType.Name, formattedArguments, null);
            }
        }

        private static string[] GetFormattedArguments(object arguments)
        {
            var argumentDictionary = PropertyHelper.ObjectToDictionary(arguments);
            var formattedArguments = new string[argumentDictionary.Count];
            var i = 0;
            foreach (var item in argumentDictionary)
            {
                formattedArguments[i++] = Convert.ToString(item.Value);
            }

            return formattedArguments;
        }
    }
}
