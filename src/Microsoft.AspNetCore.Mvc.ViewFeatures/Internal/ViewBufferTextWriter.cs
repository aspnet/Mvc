// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    /// <summary>
    /// <para>
    /// A <see cref="TextWriter"/> that is backed by a unbuffered writer (over the Response stream) and/or a 
    /// <see cref="ViewBuffer"/>
    /// </para>
    /// <para>
    /// When <c>Flush</c> or <c>FlushAsync</c> is invoked, the writer copies all content from the buffer to
    /// the writer and switches to writing to the unbuffered writer for all further write operations.
    /// </para>
    /// </summary>
    public class ViewBufferTextWriter : TextWriter
    {
        private readonly TextWriter _inner;
        private readonly HtmlEncoder _htmlEncoder;

        /// <summary>
        /// Creates a new instance of <see cref="ViewBufferTextWriter"/>.
        /// </summary>
        /// <param name="buffer">The <see cref="ViewBuffer"/> for buffered output.</param>
        /// <param name="encoding">The <see cref="System.Text.Encoding"/>.</param>
        public ViewBufferTextWriter(ViewBuffer buffer, Encoding encoding)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            Buffer = buffer;
            Encoding = encoding;
        }

        /// <summary>
        /// Creates a new instance of <see cref="ViewBufferTextWriter"/>.
        /// </summary>
        /// <param name="buffer">The <see cref="ViewBuffer"/> for buffered output.</param>
        /// <param name="encoding">The <see cref="System.Text.Encoding"/>.</param>
        /// <param name="htmlEncoder">The HTML encoder.</param>
        /// <param name="inner">
        /// The inner <see cref="TextWriter"/> to write output to when this instance is no longer buffering.
        /// </param>
        public ViewBufferTextWriter(ViewBuffer buffer, Encoding encoding, HtmlEncoder htmlEncoder, TextWriter inner)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            if (htmlEncoder == null)
            {
                throw new ArgumentNullException(nameof(htmlEncoder));
            }

            if (inner == null)
            {
                throw new ArgumentNullException(nameof(inner));
            }

            Buffer = buffer;
            Encoding = encoding;
            _htmlEncoder = htmlEncoder;
            _inner = inner;
        }

        /// <inheritdoc />
        public override Encoding Encoding { get; }

        /// <inheritdoc />
        public bool IsBuffering { get; private set; } = true;

        /// <summary>
        /// Gets the <see cref="ViewBuffer"/>.
        /// </summary>
        public ViewBuffer Buffer { get; }

        /// <inheritdoc />
        public override void Write(char value)
        {
            if (IsBuffering)
            {
                Buffer.AppendHtml(value.ToString());
            }
            else
            {
                _inner.Write(value);
            }
        }

        /// <inheritdoc />
        public override void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0 || index >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0 || (buffer.Length - index < count))
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (IsBuffering)
            {
                Buffer.AppendHtml(new string(buffer, index, count));
            }
            else
            {
                _inner.Write(buffer, index, count);
            }
        }

        /// <inheritdoc />
        public override void Write(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (IsBuffering)
            {
                Buffer.AppendHtml(value);
            }
            else
            {
                _inner.Write(value);
            }
        }

        /// <inheritdoc />
        public override void Write(object value)
        {
            if (value == null)
            {
                return;
            }

            IHtmlContentContainer container;
            IHtmlContent content;
            if ((container = value as IHtmlContentContainer) != null)
            {
                Write(container);
            }
            else if ((content = value as IHtmlContent) != null)
            {
                Write(content);
            }
            else
            {
                Write(value.ToString());
            }
        }

        /// <summary>
        /// Writes an <see cref="IHtmlContent"/> value.
        /// </summary>
        /// <param name="value">The <see cref="IHtmlContent"/> value.</param>
        public void Write(IHtmlContent value)
        {
            if (value == null)
            {
                return;
            }

            if (IsBuffering)
            {
                Buffer.AppendHtml(value);
            }
            else
            {
                value.WriteTo(_inner, _htmlEncoder);
            }
        }

        /// <summary>
        /// Writes an <see cref="IHtmlContentContainer"/> value.
        /// </summary>
        /// <param name="value">The <see cref="IHtmlContentContainer"/> value.</param>
        public void Write(IHtmlContentContainer value)
        {
            if (value == null)
            {
                return;
            }

            if (IsBuffering)
            {
                value.MoveTo(Buffer);
            }
            else
            {
                value.WriteTo(_inner, _htmlEncoder);
            }
        }

        /// <inheritdoc />
        public override void WriteLine(object value)
        {
            if (value == null)
            {
                return;
            }

            IHtmlContentContainer container;
            IHtmlContent content;
            if ((container = value as IHtmlContentContainer) != null)
            {
                Write(container);
                Write(NewLine);
            }
            else if ((content = value as IHtmlContent) != null)
            {
                Write(content);
                Write(NewLine);
            }
            else
            {
                Write(value.ToString());
                Write(NewLine);
            }
        }

        /// <inheritdoc />
        public override Task WriteAsync(char value)
        {
            if (IsBuffering)
            {
                Buffer.AppendHtml(value.ToString());
                return TaskCache.CompletedTask;
            }
            else
            {
                return _inner.WriteAsync(value);
            }
        }

        /// <inheritdoc />
        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0 || (buffer.Length - index < count))
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (IsBuffering)
            {
                Buffer.AppendHtml(new string(buffer, index, count));
                return TaskCache.CompletedTask;
            }
            else
            {
                return _inner.WriteAsync(buffer, index, count);
            }
        }

        /// <inheritdoc />
        public override Task WriteAsync(string value)
        {
            if (IsBuffering)
            {
                Buffer.AppendHtml(value);
                return TaskCache.CompletedTask;
            }
            else
            {
                return _inner.WriteAsync(value);
            }
        }

        /// <inheritdoc />
        public override void WriteLine()
        {
            if (IsBuffering)
            {
                Buffer.AppendHtml(NewLine);
            }
            else
            {
                _inner.WriteLine();
            }
        }

        /// <inheritdoc />
        public override void WriteLine(string value)
        {
            if (IsBuffering)
            {
                Buffer.AppendHtml(value);
                Buffer.AppendHtml(NewLine);
            }
            else
            {
                _inner.WriteLine(value);
            }
        }

        /// <inheritdoc />
        public override Task WriteLineAsync(char value)
        {
            if (IsBuffering)
            {
                Buffer.AppendHtml(value.ToString());
                Buffer.AppendHtml(NewLine);
                return TaskCache.CompletedTask;
            }
            else
            {
                return _inner.WriteLineAsync(value);
            }
        }

        /// <inheritdoc />
        public override Task WriteLineAsync(char[] value, int start, int offset)
        {
            if (IsBuffering)
            {
                Buffer.AppendHtml(new string(value, start, offset));
                Buffer.AppendHtml(NewLine);
                return TaskCache.CompletedTask;
            }
            else
            {
                return _inner.WriteLineAsync(value, start, offset);
            }
        }

        /// <inheritdoc />
        public override Task WriteLineAsync(string value)
        {
            if (IsBuffering)
            {
                Buffer.AppendHtml(value);
                Buffer.AppendHtml(NewLine);
                return TaskCache.CompletedTask;
            }
            else
            {
                return _inner.WriteLineAsync(value);
            }
        }

        /// <inheritdoc />
        public override Task WriteLineAsync()
        {
            if (IsBuffering)
            {
                Buffer.AppendHtml(NewLine);
                return TaskCache.CompletedTask;
            }
            else
            {
                return _inner.WriteLineAsync();
            }
        }

        /// <summary>
        /// Copies the buffered content to the unbuffered writer and invokes flush on it.
        /// Additionally causes this instance to no longer buffer and direct all write operations
        /// to the unbuffered writer.
        /// </summary>
        public override void Flush()
        {
            if (_inner == null)
            {
                return;
            }

            if (IsBuffering)
            {
                IsBuffering = false;
                Buffer.WriteTo(_inner, _htmlEncoder);
            }

            _inner.Flush();
        }

        /// <summary>
        /// Copies the buffered content to the unbuffered writer and invokes flush on it.
        /// Additionally causes this instance to no longer buffer and direct all write operations
        /// to the unbuffered writer.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous copy and flush operations.</returns>
        public override async Task FlushAsync()
        {
            if (_inner == null)
            {
                return;
            }

            if (IsBuffering)
            {
                IsBuffering = false;
                await Buffer.WriteToAsync(_inner, _htmlEncoder);
            }

            await _inner.FlushAsync();
        }
    }
}