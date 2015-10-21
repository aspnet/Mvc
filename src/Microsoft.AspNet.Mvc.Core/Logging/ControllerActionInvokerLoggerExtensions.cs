// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.Mvc.Core.Logging
{
    public static class ControllerActionInvokerLoggerExtensions
    {
        private static Action<ILogger, string, ModelValidationState?, Exception> _actionStarting;
        private static Action<ILogger, string, Exception> _actionFinishing;

        static ControllerActionInvokerLoggerExtensions()
        {
            _actionStarting = LoggerMessage.Define<string, ModelValidationState?>(
                LogLevel.Information,
                1,
                "Starting Action {ActionName}. Model state is {ModelValidationState}'");

            _actionFinishing = LoggerMessage.Define<string>(
                LogLevel.Information,
                2,
                "Stopping Action {ActionName}'");
        }

        public static void ActionStarting(this ILogger logger, ActionExecutingContext actionContext)
        {
            var actionName = actionContext.ActionDescriptor.DisplayName;
            var modelValidationState = actionContext.ModelState?.ValidationState;
            _actionStarting(logger, actionName, modelValidationState, null);
        }

        public static void ActionFinishing(this ILogger logger, ActionExecutingContext actionContext)
        {
            _actionFinishing(logger, actionContext.ActionDescriptor.DisplayName, null);
        }
    }
}
