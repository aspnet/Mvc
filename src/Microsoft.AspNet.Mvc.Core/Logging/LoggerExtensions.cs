﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    internal static class LoggerExtensions
    {
        public static void WriteValues([NotNull] this ILogger logger, object values)
        {
            logger.Write(
                logLevel: LogLevel.Verbose,
                eventId: 0,
                state: values,
                exception: null,
                formatter: LogFormatter.Formatter);
        }

        public static void WriteStructure([NotNull] this ILogger logger, ILoggerStructure structure)
        {
            logger.Write(
                logLevel: LogLevel.Verbose,
                eventId: 0,
                state: structure,
                exception: null,
                formatter: (state, error) => ((ILoggerStructure)state).Format());
        }
    }
}