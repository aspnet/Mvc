﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
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

        public override Task FlushAsync() => FlushAsyncCore();

        // private non-virtual for internal calling.
        // It first does a fast check to see if async is necessary, we inline this check.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task FlushAsyncCore()
        {
            var length = _charBuffer.Length;
            if (length == 0)
            {
                // If nothing sync buffered return CompletedTask,
                // so we can fast-path skip async state-machine creation
                return Task.CompletedTask;
            }

            return FlushAsyncAwaited();
        }

        private async Task FlushAsyncAwaited()
        {
            var length = _charBuffer.Length;
            Debug.Assert(length > 0);

            var pages = _charBuffer.Pages;
            var count = pages.Count;
            for (var i = 0; i < count; i++)
            {
                var page = pages[i];
                var pageLength = Math.Min(length, page.Length);
                if (pageLength != 0)
                {
                    await _inner.WriteAsync(page, index: 0, count: pageLength);
                }

                length -= pageLength;
            }

            Debug.Assert(length == 0);
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
            var flushTask = FlushAsyncCore();

            // FlushAsyncCore will return CompletedTask if nothing sync buffered
            // Fast-path and skip async state-machine if only a single async operation
            return ReferenceEquals(flushTask, Task.CompletedTask) ? 
                _inner.WriteAsync(value) :
                WriteAsyncAwaited(flushTask, value);
        }

        private async Task WriteAsyncAwaited(Task flushTask, char value)
        {
            await flushTask;
            await _inner.WriteAsync(value);
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            var flushTask = FlushAsyncCore();

            // FlushAsyncCore will return CompletedTask if nothing sync buffered
            // Fast-path and skip async state-machine if only a single async operation
            return ReferenceEquals(flushTask, Task.CompletedTask) ?
                _inner.WriteAsync(buffer, index, count) :
                WriteAsyncAwaited(flushTask, buffer, index, count);
        }

        private async Task WriteAsyncAwaited(Task flushTask, char[] buffer, int index, int count)
        {
            await flushTask;
            await _inner.WriteAsync(buffer, index, count);
        }

        public override Task WriteAsync(string value)
        {
            var flushTask = FlushAsyncCore();

            // FlushAsyncCore will return CompletedTask if nothing sync buffered
            // Fast-path and skip async state-machine if only a single async operation
            return ReferenceEquals(flushTask, Task.CompletedTask) ?
                _inner.WriteAsync(value) :
                WriteAsyncAwaited(flushTask, value);
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
