﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    internal static class MvcViewFeaturesLoggerExtensions
    {
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        private static readonly Action<ILogger, string, string[], Exception> _viewComponentExecuting;
        private static readonly Action<ILogger, string, double, string, Exception> _viewComponentExecuted;

        private static readonly Action<ILogger, string, double, Exception> _partialViewFound;
        private static readonly Action<ILogger, string, IEnumerable<string>, Exception> _partialViewNotFound;
        private static readonly Action<ILogger, string, Exception> _partialViewResultExecuting;
        private static readonly Action<ILogger, string, double, Exception> _partialViewResultExecuted;

        private static readonly Action<ILogger, string, Exception> _antiforgeryTokenInvalid;

        private static readonly Action<ILogger, string, Exception> _viewComponentResultExecuting;

        private static readonly Action<ILogger, string, Exception> _viewResultExecuting;
        private static readonly Action<ILogger, string, double, Exception> _viewResultExecuted;
        private static readonly Action<ILogger, string, double, Exception> _viewFound;
        private static readonly Action<ILogger, string, IEnumerable<string>, Exception> _viewNotFound;

        private static readonly Action<ILogger, string, Exception> _tempDataCookieNotFound;
        private static readonly Action<ILogger, string, Exception> _tempDataCookieLoadSuccess;
        private static readonly Action<ILogger, string, Exception> _tempDataCookieLoadFailure;

        private static readonly Action<ILogger, Type, Exception> _notMostEffectiveFilter;

        static MvcViewFeaturesLoggerExtensions()
        {
            _viewComponentExecuting = LoggerMessage.Define<string, string[]>(
                LogLevel.Debug,
                1,
                "Executing view component {ViewComponentName} with arguments ({Arguments}).");

            _viewComponentExecuted = LoggerMessage.Define<string, double, string>(
                LogLevel.Debug,
                2,
                "Executed view component {ViewComponentName} in {ElapsedMilliseconds}ms and returned " +
                "{ViewComponentResult}");

            _partialViewResultExecuting = LoggerMessage.Define<string>(
                LogLevel.Information,
                1,
                "Executing PartialViewResult, running view {PartialViewName}.");

            _partialViewFound = LoggerMessage.Define<string, double>(
                LogLevel.Debug,
                2,
                "The partial view path '{PartialViewFilePath}' was found in {ElapsedMilliseconds}ms.");

            _partialViewNotFound = LoggerMessage.Define<string, IEnumerable<string>>(
                LogLevel.Error,
                3,
                "The partial view '{PartialViewName}' was not found. Searched locations: {SearchedViewLocations}");

            _partialViewResultExecuted = LoggerMessage.Define<string, double>(
                LogLevel.Information,
                4,
                "Executed PartialViewResult - view {PartialViewName} executed in {ElapsedMilliseconds}ms.");

            _antiforgeryTokenInvalid = LoggerMessage.Define<string>(
                LogLevel.Information,
                1,
                "Antiforgery token validation failed. {Message}");

            _viewComponentResultExecuting = LoggerMessage.Define<string>(
                LogLevel.Information,
                1,
                "Executing ViewComponentResult, running {ViewComponentName}.");

            _viewResultExecuting = LoggerMessage.Define<string>(
                LogLevel.Information,
                1,
                "Executing ViewResult, running view {ViewName}.");

            _viewFound = LoggerMessage.Define<string, double>(
                LogLevel.Debug,
                2,
                "The view path '{ViewFilePath}' was found in {ElapsedMilliseconds}ms.");

            _viewNotFound = LoggerMessage.Define<string, IEnumerable<string>>(
                LogLevel.Error,
                3,
                "The view '{ViewName}' was not found. Searched locations: {SearchedViewLocations}");

            _viewResultExecuted = LoggerMessage.Define<string, double>(
                LogLevel.Information,
                4,
                "Executed ViewResult - view {ViewName} executed in {ElapsedMilliseconds}ms.");

            _tempDataCookieNotFound = LoggerMessage.Define<string>(
                LogLevel.Debug,
                1,
                "The temp data cookie {CookieName} was not found.");

            _tempDataCookieLoadSuccess = LoggerMessage.Define<string>(
                LogLevel.Debug,
                2,
                "The temp data cookie {CookieName} was used to successfully load temp data.");

            _tempDataCookieLoadFailure = LoggerMessage.Define<string>(
                LogLevel.Warning,
                3,
                "The temp data cookie {CookieName} could not be loaded.");

            _notMostEffectiveFilter = LoggerMessage.Define<Type>(
                LogLevel.Trace,
                1,
                "Skipping the execution of current filter as its not the most effective filter implementing the policy {FilterPolicy}.");
        }

        public static IDisposable ViewComponentScope(this ILogger logger, ViewComponentContext context)
        {
            return logger.BeginScope(new ViewComponentLogScope(context.ViewComponentDescriptor));
        }

        public static void ViewComponentExecuting(
            this ILogger logger,
            ViewComponentContext context,
            object[] arguments)
        {
            var formattedArguments = GetFormattedArguments(arguments);
            _viewComponentExecuting(logger, context.ViewComponentDescriptor.DisplayName, formattedArguments, null);
        }

        private static string[] GetFormattedArguments(object[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
            {
                return Array.Empty<string>();
            }

            var formattedArguments = new string[arguments.Length];
            for (var i = 0; i < formattedArguments.Length; i++)
            {
                formattedArguments[i] = Convert.ToString(arguments[i]);
            }

            return formattedArguments;
        }

        public static void ViewComponentExecuted(
            this ILogger logger,
            ViewComponentContext context,
            TimeSpan timespan,
            object result)
        {
            // Don't log if logging wasn't enabled at start of request as time will be wildly wrong.
            _viewComponentExecuted(
                logger,
                context.ViewComponentDescriptor.DisplayName,
                timespan.TotalMilliseconds,
                Convert.ToString(result),
                null);
        }

        public static void PartialViewFound(
            this ILogger logger,
            IView view,
            TimeSpan timespan)
        {
            _partialViewFound(logger, view.Path, timespan.TotalMilliseconds, null);
        }

        public static void PartialViewNotFound(
            this ILogger logger,
            string partialViewName,
            IEnumerable<string> searchedLocations)
        {
            _partialViewNotFound(logger, partialViewName, searchedLocations, null);
        }

        public static void PartialViewResultExecuting(this ILogger logger, string partialViewName)
        {
            _partialViewResultExecuting(logger, partialViewName, null);
        }

        public static void PartialViewResultExecuted(this ILogger logger, string partialViewName, TimeSpan timespan)
        {
            _partialViewResultExecuted(logger, partialViewName, timespan.TotalMilliseconds, null);
        }

        public static void AntiforgeryTokenInvalid(this ILogger logger, string message, Exception exception)
        {
            _antiforgeryTokenInvalid(logger, message, exception);
        }

        public static void ViewComponentResultExecuting(this ILogger logger, string viewComponentName)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _viewComponentResultExecuting(logger, viewComponentName, null);
            }
        }

        public static void ViewComponentResultExecuting(this ILogger logger, Type viewComponentType)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _viewComponentResultExecuting(logger, viewComponentType.Name, null);
            }
        }

        public static void ViewResultExecuting(this ILogger logger, string viewName)
        {
            _viewResultExecuting(logger, viewName, null);
        }

        public static void ViewResultExecuted(this ILogger logger, string viewName, TimeSpan timespan)
        {
            _viewResultExecuted(logger, viewName, timespan.TotalMilliseconds, null);
        }

        public static void ViewFound(this ILogger logger, IView view, TimeSpan timespan)
        {
            _viewFound(logger, view.Path, timespan.TotalMilliseconds, null);
        }

        public static void ViewNotFound(this ILogger logger, string viewName,
            IEnumerable<string> searchedLocations)
        {
            _viewNotFound(logger, viewName, searchedLocations, null);
        }

        public static void TempDataCookieNotFound(this ILogger logger, string cookieName)
        {
            _tempDataCookieNotFound(logger, cookieName, null);
        }

        public static void TempDataCookieLoadSuccess(this ILogger logger, string cookieName)
        {
            _tempDataCookieLoadSuccess(logger, cookieName, null);
        }

        public static void TempDataCookieLoadFailure(this ILogger logger, string cookieName, Exception exception)
        {
            _tempDataCookieLoadFailure(logger, cookieName, exception);
        }

        public static void NotMostEffectiveFilter(this ILogger logger, Type policyType)
        {
            _notMostEffectiveFilter(logger, policyType, null);
        }

        private class ViewComponentLogScope : IReadOnlyList<KeyValuePair<string, object>>
        {
            private readonly ViewComponentDescriptor _descriptor;

            public ViewComponentLogScope(ViewComponentDescriptor descriptor)
            {
                _descriptor = descriptor;
            }

            public KeyValuePair<string, object> this[int index]
            {
                get
                {
                    if (index == 0)
                    {
                        return new KeyValuePair<string, object>("ViewComponentName", _descriptor.DisplayName);
                    }
                    else if (index == 1)
                    {
                        return new KeyValuePair<string, object>("ViewComponentId", _descriptor.Id);
                    }
                    throw new IndexOutOfRangeException(nameof(index));
                }
            }

            public int Count => 2;

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                for (var i = 0; i < Count; ++i)
                {
                    yield return this[i];
                }
            }

            public override string ToString()
            {
                return _descriptor.DisplayName;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
