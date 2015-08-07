// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.AspNet.Html.Abstractions;
using Microsoft.Framework.Internal;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// String content which gets encoded when written.
    /// </summary>
    public class StringHtmlContent : IHtmlContent
    {
        private string _text;

        /// <summary>
        /// Creates a new instance of <see cref="StringHtmlContent"/>
        /// </summary>
        /// <param name="text"><see cref="string"/> to be HTML encoded when <see cref="WriteTo"/> is called.</param>
        public StringHtmlContent(string text)
        {
            _text = text;
        }

        /// <inheritdoc />
        public void WriteTo([NotNull] TextWriter writer, [NotNull] IHtmlEncoder encoder)
        {
            encoder.HtmlEncode(_text, writer);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _text;
        }
    }
}