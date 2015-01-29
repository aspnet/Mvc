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
                            return script.Replace("{", "{{")
                                         .Replace("}", "}}")
                                         .Replace("[[[", "{")
                                         .Replace("]]]", "}");
                        }
                        return script;
                    }
                }
            });
        }
        
        /// <summary>
        /// Encodes a .NET string for safe use as a JavaScript string literal, including inline in an HTML file.
        /// </summary>
        public static string JavaScriptStringEncode(string value)
        {
            var map = new Dictionary<char, string>
            {
                { '<', @"\u003c" },
                { '>', @"\u003e" },
                { '\'', @"\u0027" },
                { '"', @"\u0022" },
                { '\\', @"\\" },
                { '\r', "\\r" },
                { '\n', "\\n" }
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