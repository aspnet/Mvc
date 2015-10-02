﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Framework.MemoryPool;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class HttpResponseStreamWriterTest
    {
        [Fact]
        public async Task DoesNotWriteBOM()
        {
            // Arrange
            var memoryStream = new MemoryStream();
            var encodingWithBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            var writer = new HttpResponseStreamWriter(memoryStream, encodingWithBOM);
            var expectedData = new byte[] { 97, 98, 99, 100 }; // without BOM

            // Act
            using (writer)
            {
                await writer.WriteAsync("abcd");
            }

            // Assert
            Assert.Equal(expectedData, memoryStream.ToArray());
        }

        [Fact]
        public async Task DoesNotFlush_UnderlyingStream_OnClosingWriter()
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new HttpResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            await writer.WriteAsync("Hello");
            writer.Close();

            // Assert
            Assert.Equal(0, stream.FlushCallCount);
            Assert.Equal(0, stream.FlushAsyncCallCount);
        }

        [Fact]
        public async Task DoesNotFlush_UnderlyingStream_OnDisposingWriter()
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new HttpResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            await writer.WriteAsync("Hello");
            writer.Dispose();

            // Assert
            Assert.Equal(0, stream.FlushCallCount);
            Assert.Equal(0, stream.FlushAsyncCallCount);
        }

        [Fact]
        public async Task DoesNotClose_UnderlyingStream_OnDisposingWriter()
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new HttpResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            await writer.WriteAsync("Hello");
            writer.Close();

            // Assert
            Assert.Equal(0, stream.CloseCallCount);
        }

        [Fact]
        public async Task DoesNotDispose_UnderlyingStream_OnDisposingWriter()
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new HttpResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            await writer.WriteAsync("Hello world");
            writer.Dispose();

            // Assert
            Assert.Equal(0, stream.DisposeCallCount);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public async Task FlushesBuffer_OnClose(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new HttpResponseStreamWriter(stream, Encoding.UTF8);
            await writer.WriteAsync(new string('a', byteLength));

            // Act
            writer.Close();

            // Assert
            Assert.Equal(0, stream.FlushCallCount);
            Assert.Equal(0, stream.FlushAsyncCallCount);
            Assert.Equal(byteLength, stream.Length);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public async Task FlushesBuffer_OnDispose(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new HttpResponseStreamWriter(stream, Encoding.UTF8);
            await writer.WriteAsync(new string('a', byteLength));

            // Act
            writer.Dispose();

            // Assert
            Assert.Equal(0, stream.FlushCallCount);
            Assert.Equal(0, stream.FlushAsyncCallCount);
            Assert.Equal(byteLength, stream.Length);
        }

        [Fact]
        public void NoDataWritten_Flush_DoesNotFlushUnderlyingStream()
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new HttpResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            writer.Flush();

            // Assert
            Assert.Equal(0, stream.FlushCallCount);
            Assert.Equal(0, stream.Length);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public void FlushesBuffer_OnFlush(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new HttpResponseStreamWriter(stream, Encoding.UTF8);
            writer.Write(new string('a', byteLength));

            // Act
            writer.Flush();

            // Assert
            Assert.Equal(1, stream.FlushCallCount);
            Assert.Equal(byteLength, stream.Length);
        }

        [Fact]
        public async Task NoDataWritten_FlushAsync_DoesNotFlushUnderlyingStream()
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new HttpResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            await writer.FlushAsync();

            // Assert
            Assert.Equal(0, stream.FlushAsyncCallCount);
            Assert.Equal(0, stream.Length);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public async Task FlushesBuffer_OnFlushAsync(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new HttpResponseStreamWriter(stream, Encoding.UTF8);
            await writer.WriteAsync(new string('a', byteLength));

            // Act
            await writer.FlushAsync();

            // Assert
            Assert.Equal(1, stream.FlushAsyncCallCount);
            Assert.Equal(byteLength, stream.Length);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public void WriteChar_WritesToStream(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new HttpResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            using (writer)
            {
                for (var i = 0; i < byteLength; i++)
                {
                    writer.Write('a');
                }
            }

            // Assert
            Assert.Equal(byteLength, stream.Length);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public void WriteCharArray_WritesToStream(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new HttpResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            using (writer)
            {
                writer.Write((new string('a', byteLength)).ToCharArray());
            }

            // Assert
            Assert.Equal(byteLength, stream.Length);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public async Task WriteCharAsync_WritesToStream(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new HttpResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            using (writer)
            {
                for (var i = 0; i < byteLength; i++)
                {
                    await writer.WriteAsync('a');
                }
            }

            // Assert
            Assert.Equal(byteLength, stream.Length);
        }

        [Theory]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1050)]
        [InlineData(2048)]
        public async Task WriteCharArrayAsync_WritesToStream(int byteLength)
        {
            // Arrange
            var stream = new TestMemoryStream();
            var writer = new HttpResponseStreamWriter(stream, Encoding.UTF8);

            // Act
            using (writer)
            {
                await writer.WriteAsync((new string('a', byteLength)).ToCharArray());
            }

            // Assert
            Assert.Equal(byteLength, stream.Length);
        }

        [Theory]
        [InlineData("你好世界", "utf-16")]
        [InlineData("こんにちは世界", "shift_jis")]
        [InlineData("హలో ప్రపంచ", "iso-8859-1")]
        [InlineData("வணக்கம் உலக", "utf-32")]
        public async Task WritesData_InExpectedEncoding(string data, string encodingName)
        {
            // Arrange
            var encoding = Encoding.GetEncoding(encodingName);
            var expectedBytes = encoding.GetBytes(data);
            var stream = new MemoryStream();
            var writer = new HttpResponseStreamWriter(stream, encoding);

            // Act
            using (writer)
            {
                await writer.WriteAsync(data);
            }

            // Assert
            Assert.Equal(expectedBytes, stream.ToArray());
        }

        [Theory]
        [InlineData('ん', 1023, "utf-8")]
        [InlineData('ん', 1024, "utf-8")]
        [InlineData('ん', 1050, "utf-8")]
        [InlineData('你', 1023, "utf-16")]
        [InlineData('你', 1024, "utf-16")]
        [InlineData('你', 1050, "utf-16")]
        [InlineData('こ', 1023, "shift_jis")]
        [InlineData('こ', 1024, "shift_jis")]
        [InlineData('こ', 1050, "shift_jis")]
        [InlineData('హ', 1023, "iso-8859-1")]
        [InlineData('హ', 1024, "iso-8859-1")]
        [InlineData('హ', 1050, "iso-8859-1")]
        [InlineData('வ', 1023, "utf-32")]
        [InlineData('வ', 1024, "utf-32")]
        [InlineData('வ', 1050, "utf-32")]
        public async Task WritesData_OfDifferentLength_InExpectedEncoding(
            char character,
            int charCount,
            string encodingName)
        {
            // Arrange
            var encoding = Encoding.GetEncoding(encodingName);
            string data = new string(character, charCount);
            var expectedBytes = encoding.GetBytes(data);
            var stream = new MemoryStream();
            var writer = new HttpResponseStreamWriter(stream, encoding);

            // Act
            using (writer)
            {
                await writer.WriteAsync(data);
            }

            // Assert
            Assert.Equal(expectedBytes, stream.ToArray());
        }

        // None of the code in HttpResponseStreamWriter differs significantly when using pooled buffers.
        //
        // This test effectively verifies that things are correctly constructed and disposed. Pooled buffers
        // throw on the finalizer thread if not disposed, so that's why it's complicated.
        [Fact]
        public void HttpResponseStreamWriter_UsingPooledBuffers()
        {
            // Arrange
            var encoding = Encoding.UTF8;
            var stream = new MemoryStream();

            var expectedBytes = encoding.GetBytes("Hello, World!");

            using (var bytePool = new DefaultArraySegmentPool<byte>())
            {
                using (var charPool = new DefaultArraySegmentPool<char>())
                {
                    LeasedArraySegment<byte> bytes = null;
                    LeasedArraySegment<char> chars = null;
                    HttpResponseStreamWriter writer;

                    try
                    {
                        bytes = bytePool.Lease(4096);
                        chars = charPool.Lease(4096);

                        writer = new HttpResponseStreamWriter(stream, encoding, bytes, chars);
                    }
                    catch
                    {
                        if (bytes != null)
                        {
                            bytes.Owner.Return(bytes);
                        }

                        if (chars != null)
                        {
                            chars.Owner.Return(chars);
                        }

                        throw;
                    }

                    // Act
                    using (writer)
                    {
                        writer.Write("Hello, World!");
                    }
                }
            }

            // Assert
            Assert.Equal(expectedBytes, stream.ToArray());
        }

        // This covers the case where we need to limit the usable region of the char buffer
        // based on the size of the byte buffer. See comments in the constructor.
        [Fact]
        public void HttpResponseStreamWriter_UsingPooledBuffers_SmallByteBuffer()
        {
            // Arrange
            var encoding = Encoding.UTF8;
            var stream = new MemoryStream();

            var charBufferSize = encoding.GetMaxCharCount(1024);

            // This content is bigger than the byte buffer can hold, so it will need to be split
            // into two separate encoding operations.
            var content = new string('a', charBufferSize + 1);
            var expectedBytes = encoding.GetBytes(content);

            using (var bytePool = new DefaultArraySegmentPool<byte>())
            {
                using (var charPool = new DefaultArraySegmentPool<char>())
                {
                    LeasedArraySegment<byte> bytes = null;
                    LeasedArraySegment<char> chars = null;
                    HttpResponseStreamWriter writer;

                    try
                    {
                        bytes = bytePool.Lease(1024);
                        chars = charPool.Lease(4096);

                        writer = new HttpResponseStreamWriter(stream, encoding, bytes, chars);
                    }
                    catch
                    {
                        if (bytes != null)
                        {
                            bytes.Owner.Return(bytes);
                        }

                        if (chars != null)
                        {
                            chars.Owner.Return(chars);
                        }

                        throw;
                    }

                    // Zero the byte buffer because we're going to examine it.
                    Array.Clear(bytes.Data.Array, 0, bytes.Data.Array.Length);

                    // Act
                    using (writer)
                    {
                        writer.Write(content);
                    }

                    // Verify that we didn't buffer overflow 'our' region of the underlying array.
                    if (bytes.Data.Array.Length > bytes.Data.Count)
                    {
                        Assert.Equal((byte)0, bytes.Data.Array[bytes.Data.Count]);
                    }
                }
            }

            // Assert
            Assert.Equal(expectedBytes, stream.ToArray());
        }

        private class TestMemoryStream : MemoryStream
        {
            private int _flushCallCount;
            private int _flushAsyncCallCount;
            private int _closeCallCount;
            private int _disposeCallCount;

            public int FlushCallCount { get { return _flushCallCount; } }

            public int FlushAsyncCallCount { get { return _flushAsyncCallCount; } }

            public int CloseCallCount { get { return _closeCallCount; } }

            public int DisposeCallCount { get { return _disposeCallCount; } }

            public override void Flush()
            {
                _flushCallCount++;
                base.Flush();
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                _flushAsyncCallCount++;
                return base.FlushAsync(cancellationToken);
            }

            public override void Close()
            {
                _closeCallCount++;
                base.Close();
            }

            protected override void Dispose(bool disposing)
            {
                _disposeCallCount++;
                base.Dispose(disposing);
            }
        }
    }
}
