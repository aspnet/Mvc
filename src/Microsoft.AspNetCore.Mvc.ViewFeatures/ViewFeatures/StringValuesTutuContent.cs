// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    [DebuggerDisplay("{DebuggerToString()}")]
    public class StringValuesTutuContent : IHtmlContent
    {
        private readonly StringValuesTutu _stringValues;

        public StringValuesTutuContent(StringValuesTutu stringValues)
        {
            _stringValues = stringValues;
        }

        public virtual void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            foreach (var value in _stringValues)
            {
                encoder.Encode(writer, value);
            }
        }

        private string DebuggerToString()
        {
            return _stringValues.ToString();
        }
    }
}
