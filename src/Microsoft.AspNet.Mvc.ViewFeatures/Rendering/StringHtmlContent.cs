// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNet.Html.Abstractions;
using Microsoft.Framework.Internal;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// String content which gets enoded when written.
    /// </summary>
    public class StringHtmlContent : IHtmlContent
    {
        private bool _encodeOnWrite;
        private string _text;

        /// <summary>
        /// Creates a new instance of <see cref="StringHtmlContent"/>
        /// </summary>
        /// <param name="text"><c>string</c>to initialize <see cref="StringHtmlContent"/>.</param>
        public StringHtmlContent(string text)
            : this(text, encodeOnWrite: true)
        {
        }

        internal StringHtmlContent(string text, bool encodeOnWrite)
        {
            _text = text;
            _encodeOnWrite = encodeOnWrite;
        }

        /// <inheritdoc />
        public void WriteTo([NotNull] TextWriter writer, [NotNull] IHtmlEncoder encoder)
        {
            if (_encodeOnWrite)
            {
                encoder.HtmlEncode(_text, writer);
            }
            else
            {
                writer.Write(_text);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (_text == null)
            {
                return null;
            }

            using (var writer = new StringWriter())
            {
                WriteTo(writer, new HtmlEncoder());
                return writer.ToString();
            }
        }
    }
}