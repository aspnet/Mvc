// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public static class LogFormatter
    {
        /// <summary>
        /// A formatter for use with <see cref="Microsoft.Framework.Logging.ILogger.Write(
        /// Framework.Logging.LogLevel,
        /// int,
        /// object,
        /// Exception, Func{object, Exception, string})"/>.
        /// </summary>
        public static string Formatter(object o, Exception e)
        {
            if (o != null && e != null)
            {
                return o + Environment.NewLine + e;
            }

            if (o != null)
            {
                return o.ToString();
            }

            if (e != null)
            {
                return e.ToString();
            }

            return "";
        }

        /// <summary>
        /// Formats an <see cref="ILoggerStructure"/>.
        /// </summary>
        /// <param name="structure">The <see cref="ILoggerStructure"/> to format.</param>
        /// <returns>A string representation of the given <see cref="ILoggerStructure"/>.</returns>
        public static string FormatStructure(ILoggerStructure structure)
        {
            var values = structure.GetValues();
            if (values == null)
            {
                return string.Empty;
            }
            var builder = new StringBuilder();
            foreach (var kvp in values)
            {
                builder.Append(kvp.Key);
                builder.Append(": ");
                if ((kvp.Value as IEnumerable<ILoggerStructure>) != null)
                {
                    var valArray = ((IEnumerable<ILoggerStructure>)kvp.Value).AsArray();
                    for (int j = 0; j < valArray.Length - 1; j++)
                    {
                        builder.Append(valArray[j].Format());
                        builder.Append(", ");
                    }
                    if (valArray.Length > 0)
                    {
                        builder.Append(valArray[valArray.Length - 1].Format());
                    }
                }
                else if ((kvp.Value as ILoggerStructure) != null)
                {
                    builder.Append(((ILoggerStructure)kvp.Value).Format());
                }
                else
                {
                    builder.Append(kvp.Value);
                }
                builder.Append("  ");
            }
            // get rid of the extra whitespace
            if (builder.Length > 0)
            {
                builder.Length -= 2;
            }
            return builder.ToString();
        }
    }
}