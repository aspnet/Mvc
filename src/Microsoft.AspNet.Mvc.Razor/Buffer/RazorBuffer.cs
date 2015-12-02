// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNet.Html;
using Microsoft.AspNet.Mvc.Rendering;

namespace Microsoft.AspNet.Mvc.Razor.Buffer
{
    /// <summary>
    /// An <see cref="IHtmlContentBuilder"/> that is backed by a buffer provided by <see cref="IRazorBufferScope"/>.
    /// </summary>
    [DebuggerDisplay("{DebuggerToString()}")]
    public class RazorBuffer : IHtmlContentBuilder
    {
        private readonly IRazorBufferScope _bufferScope;
        private readonly string _name;
        private int _currentIndex;

        /// <summary>
        /// Initializes a new instance of <see cref="RazorBuffer"/>.
        /// </summary>
        /// <param name="bufferScope">The <see cref="IRazorBufferScope"/>.</param>
        /// <param name="name">A name to identify this instance.</param>
        public RazorBuffer(IRazorBufferScope bufferScope, string name)
        {
            if (bufferScope == null)
            {
                throw new ArgumentNullException(nameof(bufferScope));
            }

            _bufferScope = bufferScope;
            _name = name;
        }

        /// <summary>
        /// Gets the backing buffer.
        /// </summary>
        public IList<RazorBufferSegment> BufferChunks { get; private set; }

        /// <inheritdoc />
        public IHtmlContentBuilder Append(string unencoded)
        {
            if (unencoded == null)
            {
                return this;
            }

            EnsureCapacity();

            var chunk = BufferChunks[BufferChunks.Count - 1];
            chunk.Data.Array[chunk.Data.Offset + _currentIndex] = new RazorValue(unencoded);
            _currentIndex = (_currentIndex + 1) % chunk.Data.Count;
            return this;
        }

        /// <inheritdoc />
        public IHtmlContentBuilder Append(IHtmlContent content)
        {
            if (content == null)
            {
                return this;
            }

            EnsureCapacity();

            var chunk = BufferChunks[BufferChunks.Count - 1];
            chunk.Data.Array[chunk.Data.Offset + _currentIndex] = new RazorValue(content);
            _currentIndex = (_currentIndex + 1) % chunk.Data.Count;
            return this;
        }

        /// <inheritdoc />
        public IHtmlContentBuilder AppendHtml(string encoded)
        {
            if (encoded == null)
            {
                return this;
            }

            EnsureCapacity();

            var chunk = BufferChunks[BufferChunks.Count - 1];
            chunk.Data.Array[chunk.Data.Offset + _currentIndex] = new RazorValue(new HtmlString(encoded));
            _currentIndex = (_currentIndex + 1) % chunk.Data.Count;
            return this;
        }

        /// <inheritdoc />
        public IHtmlContentBuilder Clear()
        {
            if (BufferChunks != null)
            {
                _currentIndex = 0;
                BufferChunks.Clear();
            }

            return this;
        }

        /// <inheritdoc />
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            if (BufferChunks == null)
            {
                return;
            }

            var htmlTextWriter = writer as HtmlTextWriter;

            for (var i = 0; i < BufferChunks.Count; i++)
            {
                var chunk = BufferChunks[i];
                var count = i + 1 < BufferChunks.Count ? chunk.Data.Count : _currentIndex;

                for (var j = 0; j < count; j++)
                {
                    var value = chunk.Data.Array[chunk.Data.Offset + j];
                    if (htmlTextWriter != null)
                    {
                        htmlTextWriter.Write(value.Value);
                    }
                    else
                    {
                        value.WriteTo(writer, encoder);
                    }
                }
            }
        }

        private RazorBufferSegment EnsureCapacity()
        {
            if (BufferChunks == null)
            {
                BufferChunks = new List<RazorBufferSegment>(1);
                BufferChunks.Add(_bufferScope.GetSegment());
            };

            var chunk = BufferChunks[BufferChunks.Count - 1];
            if (_currentIndex == chunk.Data.Count - 1)
            {
                chunk = _bufferScope.GetSegment();
                BufferChunks.Add(chunk);
                return chunk;
            }
            else
            {
                return chunk;
            }
        }

        private string DebuggerToString() => _name;
    }
}
