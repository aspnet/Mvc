﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc
{
    public class NullLoggerFactory : ILoggerFactory
    {
        public static NullLoggerFactory Instance = new NullLoggerFactory();

        public ILogger Create(string name)
        {
            return NullLogger.Instance;
        }

        public void AddProvider(ILoggerProvider provider)
        {

        }
    }

    public class NullLogger : ILogger
    {
        public static NullLogger Instance = new NullLogger();

        public IDisposable BeginScope(object state)
        {
            return NullDisposable.Instance;
        }

        public void Write(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }
    }

    public class NullDisposable : IDisposable
    {
        public static NullDisposable Instance = new NullDisposable();

        public void Dispose()
        {
            // intentionally does nothing
        }
    }
}