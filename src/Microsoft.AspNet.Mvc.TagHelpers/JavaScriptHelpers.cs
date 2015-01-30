// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
	internal static class JavaScriptHelpers
    {
        private static readonly Assembly ResourcesAssembly = typeof(JavaScriptHelpers).GetTypeInfo().Assembly;
        private static readonly ConcurrentDictionary<Tuple<string, bool>, string> Cache = new ConcurrentDictionary<Tuple<string, bool>, string>();
        
        /// <summary>
        /// Gets an embedded JavaScript file resource and optionally decodes it for use as a .NET format string.
        /// </summary>
    	public static string GetEmbeddedJavaScript(string resourceName, bool isFormatString = false)
        {
            return Cache.GetOrAdd(Tuple.Create(resourceName, isFormatString), key =>
            {
                // Load the JavaScript from embedded resource
                using (var resourceStream = ResourcesAssembly.GetManifestResourceStream(resourceName))
                {
                    Debug.Assert(resourceStream != null, "Embedded resource missing. Ensure 'prebuild' script has run.");
        
                    using (var streamReader = new StreamReader(resourceStream))
                    {
                        var script = streamReader.ReadToEndAsync().Result;
                        if (isFormatString)
                        {
                            // Replace unescaped/escaped chars with their equivalent
                            return PrepareFormatString(script);
                        }
                        return script;
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
        
        // TODO: Remove this when we get WebUtility.JavaScriptStringEncode
        /// <summary>
        /// Encodes a .NET string for safe use as a JavaScript string literal, including inline in an HTML file.
        /// </summary>
        public static string JavaScriptStringEncode(string value)
        {
            var map = new Dictionary<char, string>
            {
                { '<', @"\u003c" },  // opening angle-bracket
                { '>', @"\u003e" },  // closing angle-bracket
                { '\'', @"\u0027" }, // single quote
                { '"', @"\u0022" },  // double quote
                { '\\', @"\\" },     // back slash
                { '\r', "\\r" },     // carriage return
                { '\n', "\\n" }      // new line
            };
            var result = new StringBuilder();

            foreach (var c in value)
            {
                if (map.ContainsKey(c))
                {
                    result.Append(map[c]);
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
    }
}