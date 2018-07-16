// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.WebUtilities;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public class MemoryHtmlContent : IHtmlContent
    {
        private readonly ReadOnlyMemory<byte> _value;

        public MemoryHtmlContent(ReadOnlyMemory<byte> value)
        {
            _value = value;
        }

        public ReadOnlyMemory<byte> Value => _value;

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            if (writer is HttpResponseStreamWriter responseWriter && writer.Encoding.WebName =="utf-8")
            {
                writer.Write(_value);
                return;
            }

            // Slow path
            var chars = Encoding.UTF8.GetChars(_value.ToArray());
            writer.Write(chars);
        }
    }
}