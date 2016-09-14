﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Internal
{
    public class PagedBufferedTextWriter : TextWriter
    {
        private readonly TextWriter _inner;
        private readonly PagedCharBuffer _charBuffer;

        public PagedBufferedTextWriter(ArrayPool<char> pool, TextWriter inner)
        {
            _charBuffer = new PagedCharBuffer(new ArrayPoolBufferSource(pool));
            _inner = inner;
        }

        public override Encoding Encoding => _inner.Encoding;

        public override void Flush()
        {
            // Don't do anything. We'll call FlushAsync.
        }

        public override async Task FlushAsync()
        {
            var pages = _charBuffer.Pages;
            if (_charBuffer.Length == 0)
            {
                return;
            }

            var length = _charBuffer.Length;
            for (var i = 0; i < pages.Count; i++)
            {
                var page = pages[i];

                var count = Math.Min(length, PagedCharBuffer.PageSize);
                if (count > 0)
                {
                    await _inner.WriteAsync(page, 0, count);
                }

                length -= count;
            }

            _charBuffer.Clear();
        }

        public override void Write(char value)
        {
            _charBuffer.Append(value);
        }

        public override void Write(char[] buffer)
        {
            if (buffer == null)
            {
                return;
            }

            _charBuffer.Append(buffer, 0, buffer.Length);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            _charBuffer.Append(buffer, index, count);
        }

        public override void Write(string value)
        {
            if (value == null)
            {
                return;
            }

            _charBuffer.Append(value);
        }

        public override Task WriteAsync(char value)
        {
            return _inner.WriteAsync(value);
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            return _inner.WriteAsync(buffer, index, count);
        }

        public override Task WriteAsync(string value)
        {
            return _inner.WriteAsync(value);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _charBuffer.Dispose();
        }
    }
}
