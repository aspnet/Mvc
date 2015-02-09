// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    /// <summary>
    /// Utility methods for dealing with JavaScript.
    /// </summary>
    public static class JavaScriptResources
    {
        private static readonly Assembly ResourcesAssembly = typeof(JavaScriptEncoding).GetTypeInfo().Assembly;

        private static readonly ConcurrentDictionary<string, string> Cache =
            new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

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
        /// Gets an embedded JavaScript file resource and decodes it for use as a .NET format string.
        /// </summary>
        public static string GetEmbeddedJavaScript(string resourceName)
        {
            return Cache.GetOrAdd(resourceName, key =>
            {
                // Load the JavaScript from embedded resource
                using (var resourceStream = ResourcesAssembly.GetManifestResourceStream(key))
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
    }
}