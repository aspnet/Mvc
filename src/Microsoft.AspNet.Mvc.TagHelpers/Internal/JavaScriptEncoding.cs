// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Microsoft.AspNet.Mvc.TagHelpers.Internal
{
    /// <summary>
    /// Utility methods for dealing with JavaScript.
    /// </summary>
    public static class JavaScriptEncoding
    {
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
        /// Encodes a .NET string array for safe use as a JavaScript array literal, including inline in an HTML file.
        /// </summary>
        public static string JavaScriptArrayEncode(IEnumerable<string> values)
        {
            var builder = new StringBuilder();

            JavaScriptArrayEncode(values, builder);

            return builder.ToString();
        }

        /// <summary>
        /// Encodes a .NET string array for safe use as a JavaScript array literal, including inline in an HTML file.
        /// </summary>
        public static void JavaScriptArrayEncode(IEnumerable<string> values, StringBuilder builder)
        {
            builder.Append("[");

            var firstAdded = false;

            foreach (var value in values)
            {
                if (firstAdded)
                {
                    builder.Append(",");
                }
                builder.Append("\"");
                JavaScriptStringEncode(value, builder);
                builder.Append("\"");
                firstAdded = true;
            }

            builder.Append("]");
        }

        /// <summary>
        /// Encodes a .NET string for safe use as a JavaScript string literal, including inline in an HTML file.
        /// </summary>
        public static string JavaScriptStringEncode(string value)
        {
            var builder = new StringBuilder();

            JavaScriptStringEncode(value, builder);

            return builder.ToString();
        }

        /// <summary>
        /// Encodes a .NET string for safe use as a JavaScript string literal, including inline in an HTML file.
        /// </summary>
        public static void JavaScriptStringEncode(string value, StringBuilder builder)
        {
            foreach (var character in value)
            {
                if (CharRequiresJavaScriptEncoding(character))
                {
                    EncodeAndAppendChar(builder, character);
                }
                else
                {
                    builder.Append(character);
                }
            }
        }

        private static bool CharRequiresJavaScriptEncoding(char character)
        {
            return character < 0x20 // Control chars
                || EncodingMap.ContainsKey(character);
        }

        private static void EncodeAndAppendChar(StringBuilder builder, char character)
        {
            string mapped;

            if (!EncodingMap.TryGetValue(character, out mapped))
            {
                mapped = "\\u" + ((int)character).ToString("x4", CultureInfo.InvariantCulture);
            }

            builder.Append(mapped);
        }
    }
}