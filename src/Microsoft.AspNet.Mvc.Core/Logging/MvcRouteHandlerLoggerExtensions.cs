﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    internal static class MvcRouteHandlerLoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception> _actionExecuting;
        private static readonly Action<ILogger, string, double, Exception> _actionExecuted;

        static MvcRouteHandlerLoggerExtensions()
        {
            _actionExecuting = LoggerMessage.Define<string>(
                LogLevel.Verbose,
                1,
                "Executing action {ActionName}");

            _actionExecuted = LoggerMessage.Define<string, double>(
                LogLevel.Information,
                2,
                "Executed action {ActionName} in {ElapsedMilliseconds}ms");
        }

        public static IDisposable ActionScope(this ILogger logger, ActionDescriptor action)
        {
            return logger.BeginScopeImpl(new ActionLogScope(action));
        }

        public static void ExecutingAction(this ILogger logger, ActionDescriptor action)
        {
            _actionExecuting(logger, action.DisplayName, null);
        }

        public static void ExecutedAction(this ILogger logger, ActionDescriptor action, int startTicks)
        {
            var elapsed = new TimeSpan(Environment.TickCount - startTicks);
            _actionExecuted(logger, action.DisplayName, elapsed.TotalMilliseconds, null);
        }

        public static void NoActionsMatched(this ILogger logger)
        {
            logger.LogVerbose(3, "No actions matched the current request");
        }

        private class ActionLogScope : ILogValues
        {
            private readonly ActionDescriptor _action;

            public ActionLogScope(ActionDescriptor action)
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action));
                }

                _action = action;
            }

            public IEnumerable<KeyValuePair<string, object>> GetValues()
            {
                return new KeyValuePair<string, object>[]
                {
                    new KeyValuePair<string, object>("ActionId", _action.Id),
                    new KeyValuePair<string, object>("ActionName", _action.DisplayName),
                };
            }

            public override string ToString()
            {
                // We don't include the _action.Id here because it's just an opaque guid, and if
                // you have text logging, you can already use the requestId for correlation.
                return _action.DisplayName;
            }
        }
    }
}
