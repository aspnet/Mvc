// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    public static class JavaScriptUtility
    {
        private static readonly Assembly ResourcesAssembly = typeof(JavaScriptUtility).GetTypeInfo().Assembly;

        private static readonly ConcurrentDictionary<string, string> Cache = new ConcurrentDictionary<string, string>();

        private static readonly IDictionary<char, string> EncodingMap = new Dictionary<char, string>
        {
            { '<', @"\u003c" },      // opening angle-bracket
            { '>', @"\u003e" },      // closing angle-bracket
            { '\'', @"\u0027" },     // single quote
            { '"', @"\u0022" },      // double quote
            { '\\', @"\\" },         // back slash
            { '\r', "\\r" },         // carriage return
            { '\n', "\\n" },         // new line
            { '\u0085', @"\u0085" }, // next line
            { '&', @"\u0026" },      // ampersand
        };

        /// <summary>
        /// Gets an embedded JavaScript file resource and optionally decodes it for use as a .NET format string.
        /// </summary>
        public static string GetEmbeddedJavaScript(string resourceName)
        {
            return Cache.GetOrAdd(resourceName, key =>
            {
                // Load the JavaScript from embedded resource
                using (var resourceStream = ResourcesAssembly.GetManifestResourceStream(resourceName))
                {
                    Debug.Assert(resourceStream != null, "Embedded resource missing. Ensure 'prebuild' script has run.");

                    using (var streamReader = new StreamReader(resourceStream))
                    {
                        var script = streamReader.ReadToEnd();

                        // Replace unescaped/escaped chars with their equivalent
                        return PrepareFormatString(script);
                    }
                }
            });
        }

        // Internal so we can test this separately
        internal static string PrepareFormatString(string input)
        {
            return input.Replace("{", "{{")
                        .Replace("}", "}}")
                        .Replace("[[[", "{")
                        .Replace("]]]", "}");
        }

        // TODO: Remove this when we get WebUtility.JavaScriptStringEncode https://github.com/aspnet/HttpAbstractions/issues/72
        /// <summary>
        /// Encodes a .NET string for safe use as a JavaScript string literal, including inline in an HTML file.
        /// </summary>
        internal static string JavaScriptStringEncode(string value)
        {
            var result = new StringBuilder();

            foreach (var c in value)
            {
                if (CharRequiresJavaScriptEncoding(c))
                {
                    EncodeAndAppendChar(result, c);
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        private static bool CharRequiresJavaScriptEncoding(char c)
        {
            return c < 0x20 // Control chars
                || EncodingMap.ContainsKey(c);
        }

        private static void EncodeAndAppendChar(StringBuilder builder, char c)
        {
            builder.Append(EncodingMap.ContainsKey(c)
                ? EncodingMap[c]
                : ((int)c).ToString("x4", CultureInfo.InvariantCulture));
        }
    }
}