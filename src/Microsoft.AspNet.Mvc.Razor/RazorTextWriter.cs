// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.Html.Abstractions;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// An <see cref="HtmlTextWriter"/> that is backed by a unbuffered writer (over the Response stream) and a buffered
    /// <see cref="StringCollectionTextWriter"/>. When <c>Flush</c> or <c>FlushAsync</c> is invoked, the writer
    /// copies all content from the buffered writer to the unbuffered one and switches to writing to the unbuffered
    /// writer for all further write operations.
    /// </summary>
    /// <remarks>
    /// This type is designed to avoid creating large in-memory strings when buffering and supporting the contract that
    /// <see cref="RazorPage.FlushAsync"/> expects.
    /// </remarks>
    public class RazorTextWriter : HtmlTextWriter
    {
        /// <summary>
        /// Creates a new instance of <see cref="RazorTextWriter"/>.
        /// </summary>
        /// <param name="unbufferedWriter">The <see cref="TextWriter"/> to write output to when this instance
        /// is no longer buffering.</param>
        /// <param name="encoding">The character <see cref="Encoding"/> in which the output is written.</param>
        /// <param name="encoder">The HTML encoder.</param>
        public RazorTextWriter(TextWriter unbufferedWriter, HtmlEncoder encoder)
        {
            UnbufferedWriter = unbufferedWriter;
            HtmlEncoder = encoder;
        }

        /// <inheritdoc />
        public override Encoding Encoding => UnbufferedWriter.Encoding;

        /// <inheritdoc />
        public bool IsBuffering { get; private set; } = true;

        // Internal for unit testing
        internal TextWriter UnbufferedWriter { get; }

        private HtmlEncoder HtmlEncoder { get; }

        public IHtmlContentBuilder Content { get; set; }

        /// <summary>
        /// Copies the buffered content to the unbuffered writer and invokes flush on it.
        /// Additionally causes this instance to no longer buffer and direct all write operations
        /// to the unbuffered writer.
        /// </summary>
        public override void Flush()
        {
            IsBuffering = false;
            Content.WriteTo(UnbufferedWriter, HtmlEncoder);
            Content.Clear();
            UnbufferedWriter.Flush();
        }

        /// <summary>
        /// Copies the buffered content to the unbuffered writer and invokes flush on it.
        /// Additionally causes this instance to no longer buffer and direct all write operations
        /// to the unbuffered writer.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous copy and flush operations.</returns>
        public override Task FlushAsync()
        {
            IsBuffering = false;
            Content.WriteTo(UnbufferedWriter, HtmlEncoder);
            Content.Clear();

            return UnbufferedWriter.FlushAsync();
        }

        /// <inheritdoc />
        public override void Write(string value)
        {
            Content.AppendHtml(value);
        }

        /// <inheritdoc />
        public override void Write(char value)
        {
            Content.AppendHtml(value.ToString());
        }

        /// <inheritdoc />
        public override void Write(IHtmlContent value)
        {
            Content.Append(value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Content.ToString();
        }
    }
}