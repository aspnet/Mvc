// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Razor.Buffer;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Testing;
using Microsoft.Extensions.WebEncoders.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.Test
{
    public class RazorTextWriterTest
    {
        [Fact]
        [ReplaceCulture]
        public void Write_WritesDataTypes_ToBuffer()
        {
            // Arrange
            var expected = new object[] { "True", "3", "18446744073709551615", "Hello world", "3.14", "2.718", "m" };
            var buffer = new RazorBuffer(new TestRazorBufferSource(), "some-name");
            var writer = new RazorTextWriter(TextWriter.Null, buffer, new HtmlTestEncoder());

            // Act
            writer.Write(true);
            writer.Write(3);
            writer.Write(ulong.MaxValue);
            writer.Write(new TestClass());
            writer.Write(3.14);
            writer.Write(2.718m);
            writer.Write('m');

            // Assert
            Assert.Equal(expected, GetValues(buffer));
        }

        [Fact]
        [ReplaceCulture]
        public void Write_WritesDataTypes_ToUnderlyingStream_WhenNotBuffering()
        {
            // Arrange
            var expected = new[] { "True", "3", "18446744073709551615", "Hello world", "3.14", "2.718" };
            var unbufferedWriter = new Mock<TextWriter>();
            unbufferedWriter.SetupGet(w => w.Encoding).Returns(Encoding.UTF8);
            var buffer = new RazorBuffer(new TestRazorBufferSource(), "some-name");
            var writer = new RazorTextWriter(unbufferedWriter.Object, buffer, new HtmlTestEncoder());
            var testClass = new TestClass();

            // Act
            writer.Flush();
            writer.Write(true);
            writer.Write(3);
            writer.Write(ulong.MaxValue);
            writer.Write(testClass);
            writer.Write(3.14);
            writer.Write(2.718m);

            // Assert
            Assert.Null(buffer.BufferChunks);
            foreach (var item in expected)
            {
                unbufferedWriter.Verify(v => v.Write(item), Times.Once());
            }
        }

        [Fact]
        [ReplaceCulture]
        public async Task Write_WritesCharValues_ToUnderlyingStream_WhenNotBuffering()
        {
            // Arrange
            var unbufferedWriter = new Mock<TextWriter> { CallBase = true };
            unbufferedWriter.SetupGet(w => w.Encoding).Returns(Encoding.UTF8);
            var buffer = new RazorBuffer(new TestRazorBufferSource(), "some-name");
            var writer = new RazorTextWriter(unbufferedWriter.Object, buffer, new HtmlTestEncoder());
            var buffer1 = new[] { 'a', 'b', 'c', 'd' };
            var buffer2 = new[] { 'd', 'e', 'f' };

            // Act
            writer.Flush();
            writer.Write('x');
            writer.Write(buffer1, 1, 2);
            writer.Write(buffer2);
            await writer.WriteAsync(buffer2, 1, 1);
            await writer.WriteLineAsync(buffer1);

            // Assert
            Assert.Null(buffer.BufferChunks);
            unbufferedWriter.Verify(v => v.Write('x'), Times.Once());
            unbufferedWriter.Verify(v => v.Write(buffer1, 1, 2), Times.Once());
            unbufferedWriter.Verify(v => v.Write(buffer1, 0, 4), Times.Once());
            unbufferedWriter.Verify(v => v.Write(buffer2, 0, 3), Times.Once());
            unbufferedWriter.Verify(v => v.WriteAsync(buffer2, 1, 1), Times.Once());
            unbufferedWriter.Verify(v => v.WriteLine(), Times.Once());
        }

        [Fact]
        [ReplaceCulture]
        public async Task Write_WritesStringValues_ToUnbufferedStream_WhenNotBuffering()
        {
            // Arrange
            var unbufferedWriter = new Mock<TextWriter>();
            unbufferedWriter.SetupGet(w => w.Encoding).Returns(Encoding.UTF8);
            var buffer = new RazorBuffer(new TestRazorBufferSource(), "some-name");
            var writer = new RazorTextWriter(unbufferedWriter.Object, buffer, new HtmlTestEncoder());

            // Act
            await writer.FlushAsync();
            writer.Write("a");
            writer.WriteLine("ab");
            await writer.WriteAsync("ef");
            await writer.WriteLineAsync("gh");

            // Assert
            Assert.Null(buffer.BufferChunks);
            unbufferedWriter.Verify(v => v.Write("a"), Times.Once());
            unbufferedWriter.Verify(v => v.WriteLine("ab"), Times.Once());
            unbufferedWriter.Verify(v => v.WriteAsync("ef"), Times.Once());
            unbufferedWriter.Verify(v => v.WriteLineAsync("gh"), Times.Once());
        }

        [Fact]
        [ReplaceCulture]
        public void WriteLine_WritesDataTypes_ToBuffer()
        {
            // Arrange
            var newLine = Environment.NewLine;
            var expected = new List<object> { "False", newLine, "1.1", newLine, "3", newLine };
            var buffer = new RazorBuffer(new TestRazorBufferSource(), "some-name");
            var writer = new RazorTextWriter(TextWriter.Null, buffer, new HtmlTestEncoder());

            // Act
            writer.WriteLine(false);
            writer.WriteLine(1.1f);
            writer.WriteLine(3L);

            // Assert
            Assert.Equal(expected, GetValues(buffer));
        }

        [Fact]
        [ReplaceCulture]
        public void WriteLine_WritesDataTypes_ToUnbufferedStream_WhenNotBuffering()
        {
            // Arrange
            var unbufferedWriter = new Mock<TextWriter>();
            unbufferedWriter.SetupGet(w => w.Encoding).Returns(Encoding.UTF8);
            var buffer = new RazorBuffer(new TestRazorBufferSource(), "some-name");
            var writer = new RazorTextWriter(unbufferedWriter.Object, buffer, new HtmlTestEncoder());

            // Act
            writer.Flush();
            writer.WriteLine(false);
            writer.WriteLine(1.1f);
            writer.WriteLine(3L);

            // Assert
            Assert.Null(buffer.BufferChunks);
            unbufferedWriter.Verify(v => v.Write("False"), Times.Once());
            unbufferedWriter.Verify(v => v.Write("1.1"), Times.Once());
            unbufferedWriter.Verify(v => v.Write("3"), Times.Once());
            unbufferedWriter.Verify(v => v.WriteLine(), Times.Exactly(3));
        }

        [Fact]
        public async Task WriteLines_WritesCharBuffer()
        {
            // Arrange
            var newLine = Environment.NewLine;
            var buffer = new RazorBuffer(new TestRazorBufferSource(), "some-name");
            var writer = new RazorTextWriter(TextWriter.Null, buffer, new HtmlTestEncoder());

            // Act
            writer.WriteLine();
            await writer.WriteLineAsync();

            // Assert
            var actual = GetValues(buffer);
            Assert.Equal<object>(new[] { newLine, newLine }, actual);
        }

        [Fact]
        public async Task Write_WritesStringBuffer()
        {
            // Arrange
            var newLine = Environment.NewLine;
            var input1 = "Hello";
            var input2 = "from";
            var input3 = "ASP";
            var input4 = ".Net";
            var buffer = new RazorBuffer(new TestRazorBufferSource(), "some-name");
            var writer = new RazorTextWriter(TextWriter.Null, buffer, new HtmlTestEncoder());

            // Act
            writer.Write(input1);
            writer.WriteLine(input2);
            await writer.WriteAsync(input3);
            await writer.WriteLineAsync(input4);

            // Assert
            var actual = GetValues(buffer);
            Assert.Equal<object>(new[] { input1, input2, newLine, input3, input4, newLine }, actual);
        }

        [Fact]
        public void Write_HtmlContent_AfterFlush_GoesToStream()
        {
            // Arrange
            var stringWriter = new StringWriter();
            var buffer = new RazorBuffer(new TestRazorBufferSource(), "some-name");
            var writer = new RazorTextWriter(stringWriter, buffer, new HtmlTestEncoder());
            writer.Flush();

            var content = new HtmlString("Hello, world!");

            // Act
            writer.Write(content);

            // Assert
            Assert.Equal("Hello, world!", stringWriter.ToString());
        }

        private static object[] GetValues(RazorBuffer buffer)
        {
            return buffer.BufferChunks
                .SelectMany(c => c.Data)
                .Select(d => d.Value)
                .TakeWhile(d => d != null)
                .ToArray();
        }

        private class TestClass
        {
            public override string ToString()
            {
                return "Hello world";
            }
        }
    }
}