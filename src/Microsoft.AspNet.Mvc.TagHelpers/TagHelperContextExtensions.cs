// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Logging;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.TagHelpers;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Utility related extensions for <see cref="TagHelperContext"/>.
    /// </summary>
    public static class TagHelperContextExtensions
    {
        /// <summary>
        /// Determines whether a <see cref="ITagHelper" />'s required attributes are present, non null, non empty, and
        /// non whitepsace.
        /// </summary>
        /// <param name="context">The <see cref="TagHelperContext"/>.</param>
        /// <param name="requiredAttributes">The attributes the <see cref="ITagHelper" /> requires in order to run.</param>
        /// <param name="logger">An optional <see cref="ILogger"/> to log warning details to.</param>
        /// <returns>A <see cref="bool"/> indicating whether the <see cref="ITagHelper" /> should run.</returns> 
        public static bool AllRequiredAttributesArePresent(
            [NotNull]this TagHelperContext context,
            [NotNull]IEnumerable<string> requiredAttributes,
            ILogger logger = null)
        {
            // Check for all attribute values & log a warning if any required are missing
            var atLeastOnePresent = false;
            var missingAttrNames = new List<string>();

            foreach (var attr in requiredAttributes)
            {
                if (!context.AllAttributes.ContainsKey(attr)
                    || context.AllAttributes[attr] == null
                    || string.IsNullOrWhiteSpace(context.AllAttributes[attr] as string))
                {
                    // Missing attribute!
                    missingAttrNames.Add(attr);
                }
                else
                {
                    atLeastOnePresent = true;
                }
            }

            if (missingAttrNames.Any())
            {
                if (atLeastOnePresent && logger != null && logger.IsEnabled(LogLevel.Warning))
                {
                    // At least 1 attribute was present indicating the user intended to use the tag helper,
                    // but at least 1 was missing too, so log a warning with the details.
                    logger.WriteWarning(new MissingAttributeLoggerStructure(context.UniqueId, missingAttrNames));
                }

                return false;
            }

            // All required attributes present
            return true;
        }

        public static ModeResult<TMode> DetermineMode<TMode, TSet>(
            this TagHelperContext context,
            IEnumerable<Tuple<TMode, TSet>> attributeSets,
            ILogger logger = null)
            where TSet : IEnumerable<string>
        {
            var bufferedLogger = logger != null ? new BufferedLogger(logger) : null;

            foreach (var set in attributeSets)
            {
                if (AllRequiredAttributesArePresent(context, set.Item2, bufferedLogger))
                {
                    return ModeResult.Matched(set.Item1);
                }
            }

            // TODO: This might need some more work, possibly just refactor the AllRequiredAttributesArePresent to
            //       support getting the missed attributes back and then write this out more cleanly here.
            // No match found, flush any log messages that were written while checking
            if (bufferedLogger != null)
            {
                logger.WriteWarning("Partial mode matches for {0} with ID {1} were found:", nameof(LinkTagHelper), context.UniqueId);
                bufferedLogger.Flush();
            }

            return ModeResult<TMode>.Unmatched;
        }

        private class BufferedLogger : ILogger
        {
            private readonly ILogger _logger;
            private readonly List<LogCall> _logs = new List<LogCall>();

            public BufferedLogger(ILogger logger)
            {
                _logger = logger;
            }

            public IDisposable BeginScope(object state)
            {
                return _logger.BeginScope(state);
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return _logger.IsEnabled(logLevel);
            }

            public void Write(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
            {
                _logs.Add(new LogCall(logLevel, eventId, state, exception, formatter));
            }

            public void Flush()
            {
                foreach (var log in _logs)
                {
                    _logger.Write(log.LogLevel, log.EventId, log.State, log.Exception, log.Formatter);
                }
            }

            private class LogCall
            {
                public LogCall(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
                {
                    LogLevel = logLevel;
                    EventId = eventId;
                    State = state;
                    Exception = exception;
                    Formatter = formatter;
                }

                public LogLevel LogLevel { get; set; }
                public int EventId { get; set; }
                public object State { get; set; }
                public Exception Exception { get; set; }
                public Func<object, Exception, string> Formatter { get; set; }
            }
        }
    }

    public static class ModeResult
    {
        public static ModeResult<TMode> Matched<TMode>(TMode mode)
        {
            return new ModeResult<TMode> { Matched = true, Mode = mode };
        }
    }

    public class ModeResult<TMode>
    {
        private static readonly ModeResult<TMode> _unmatched = new ModeResult<TMode> { Matched = false };

        public TMode Mode { get; set; }

        public bool Matched { get; set; }

        public static ModeResult<TMode> Unmatched
        {
            get { return _unmatched; }
        }
    }
}