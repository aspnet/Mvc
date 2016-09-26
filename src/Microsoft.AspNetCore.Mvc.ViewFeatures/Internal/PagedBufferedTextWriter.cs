// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Internal;

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

        public override Task FlushAsync()
        {
            var length = _charBuffer.Length;
            if (length == 0)
            {
                return TaskCache.CompletedTask;
            }

            var pages = _charBuffer.Pages;
            for (var i = 0; i < pages.Count; i++)
            {
                var page = pages[i];

                var pageLength = Math.Min(length, PagedCharBuffer.PageSize);
                if (pageLength != 0)
                {
                    var writeTask = _inner.WriteAsync(page, 0, pageLength);
                    if (writeTask.Status != TaskStatus.RanToCompletion)
                    {
                        // Write did not complete sync, go async
                        return FlushAsyncAwaited(writeTask, i - 1, length - pageLength);
                    }
                }
                length -= pageLength;
            }

            _charBuffer.Clear();

            return TaskCache.CompletedTask;
        }

        private async Task FlushAsyncAwaited(Task writeTask, int i, int length)
        {
            // Await last write
            await writeTask;

            var pages = _charBuffer.Pages;
            for (; i < pages.Count; i++)
            {
                var page = pages[i];

                var pageLength = Math.Min(length, PagedCharBuffer.PageSize);
                if (pageLength != 0)
                {
                    await _inner.WriteAsync(page, 0, pageLength);
                }
                length -= pageLength;
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
            var flushTask = FlushAsync();
            if (flushTask.Status != TaskStatus.RanToCompletion)
            {
                // Flush did not complete sync, go async
                return WriteAsyncAwaited(flushTask, value);
            }

            return _inner.WriteAsync(value);
        }

        private async Task WriteAsyncAwaited(Task flushTask, char value)
        {
            await flushTask;
            await _inner.WriteAsync(value);
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            var flushTask = FlushAsync();
            if (flushTask.Status != TaskStatus.RanToCompletion)
            {
                // Flush did not complete sync, go async
                return WriteAsyncAwaited(flushTask, buffer, index, count);
            }

            return _inner.WriteAsync(buffer, index, count);
        }

        private async Task WriteAsyncAwaited(Task flushTask, char[] buffer, int index, int count)
        {
            await flushTask;
            await _inner.WriteAsync(buffer, index, count);
        }

        public override Task WriteAsync(string value)
        {
            var flushTask = FlushAsync();
            if (flushTask.Status != TaskStatus.RanToCompletion)
            {
                // Flush did not complete sync, go async
                return WriteAsyncAwaited(flushTask, value);
            }

            return _inner.WriteAsync(value);
        }

        private async Task WriteAsyncAwaited(Task flushTask, string value)
        {
            await flushTask;
            await _inner.WriteAsync(value);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _charBuffer.Dispose();
        }
    }
}
