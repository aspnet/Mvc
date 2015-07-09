// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Testing;
using Microsoft.Framework.WebEncoders;
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
            var expected = new[] { "True", "3", "18446744073709551615", "Hello world", "3.14", "2.718", "m" };
            var writer = new RazorTextWriter(TextWriter.Null, Encoding.UTF8);

            // Act
            writer.Write(true);
            writer.Write(3);
            writer.Write(ulong.MaxValue);
            writer.Write(new TestClass());
            writer.Write(3.14);
            writer.Write(2.718m);
            writer.Write('m');

            // Assert
            Assert.Equal(expected, writer.BufferedWriter.Content.Entries);
        }

        [Fact]
        [ReplaceCulture]
        public void Write_WritesDataTypes_ToUnderlyingStream_WhenNotBuffering()
        {
            // Arrange
            var expected = new[] { "True", "3", "18446744073709551615", "Hello world", "3.14", "2.718" };
            var unbufferedWriter = new Mock<TextWriter>();
            var writer = new RazorTextWriter(unbufferedWriter.Object, Encoding.UTF8);
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
            Assert.Empty(writer.BufferedWriter.Content.Entries);
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
            var writer = new RazorTextWriter(unbufferedWriter.Object, Encoding.UTF8);
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
            Assert.Empty(writer.BufferedWriter.Content.Entries);
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
            var writer = new RazorTextWriter(unbufferedWriter.Object, Encoding.UTF8);

            // Act
            await writer.FlushAsync();
            writer.Write("a");
            writer.WriteLine("ab");
            await writer.WriteAsync("ef");
            await writer.WriteLineAsync("gh");

            // Assert
            Assert.Empty(writer.BufferedWriter.Content.Entries);
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
            var writer = new RazorTextWriter(TextWriter.Null, Encoding.UTF8);

            // Act
            writer.WriteLine(false);
            writer.WriteLine(1.1f);
            writer.WriteLine(3L);

            // Assert
            Assert.Equal(expected, writer.BufferedWriter.Content.Entries);
        }

        [Fact]
        [ReplaceCulture]
        public void WriteLine_WritesDataTypes_ToUnbufferedStream_WhenNotBuffering()
        {
            // Arrange
            var unbufferedWriter = new Mock<TextWriter>();
            var writer = new RazorTextWriter(unbufferedWriter.Object, Encoding.UTF8);

            // Act
            writer.Flush();
            writer.WriteLine(false);
            writer.WriteLine(1.1f);
            writer.WriteLine(3L);

            // Assert
            Assert.Empty(writer.BufferedWriter.Content.ToString());
            unbufferedWriter.Verify(v => v.Write("False"), Times.Once());
            unbufferedWriter.Verify(v => v.Write("1.1"), Times.Once());
            unbufferedWriter.Verify(v => v.Write("3"), Times.Once());
            unbufferedWriter.Verify(v => v.WriteLine(), Times.Exactly(3));
        }

        [Fact]
        public async Task Write_WritesCharBuffer()
        {
            // Arrange
            var input1 = new ArraySegment<char>(new char[] { 'a', 'b', 'c', 'd' }, 1, 3);
            var input2 = new ArraySegment<char>(new char[] { 'e', 'f' }, 0, 2);
            var input3 = new ArraySegment<char>(new char[] { 'g', 'h', 'i', 'j' }, 3, 1);
            var writer = new RazorTextWriter(TextWriter.Null, Encoding.UTF8);

            // Act
            writer.Write(input1.Array, input1.Offset, input1.Count);
            await writer.WriteAsync(input2.Array, input2.Offset, input2.Count);
            await writer.WriteLineAsync(input3.Array, input3.Offset, input3.Count);

            // Assert
            var buffer = writer.BufferedWriter.Content.Entries;
            Assert.Equal(4, buffer.Count);
            Assert.Equal("bcd", buffer[0]);
            Assert.Equal("ef", buffer[1]);
            Assert.Equal("j", buffer[2]);
            Assert.Equal(Environment.NewLine, buffer[3]);
        }

        [Fact]
        public async Task WriteLines_WritesCharBuffer()
        {
            // Arrange
            var newLine = Environment.NewLine;
            var writer = new RazorTextWriter(TextWriter.Null, Encoding.UTF8);

            // Act
            writer.WriteLine();
            await writer.WriteLineAsync();

            // Assert
            var actual = writer.BufferedWriter.Content.Entries;
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
            var writer = new RazorTextWriter(TextWriter.Null, Encoding.UTF8);

            // Act
            writer.Write(input1);
            writer.WriteLine(input2);
            await writer.WriteAsync(input3);
            await writer.WriteLineAsync(input4);

            // Assert
            var actual = writer.BufferedWriter.Content.Entries;
            Assert.Equal<object>(new[] { input1, input2, newLine, input3, input4, newLine }, actual);
        }

        [Fact]
        public void Copy_CopiesContent_IfTargetTextWriterIsARazorTextWriterAndBuffering()
        {
            // Arrange
            var source = new RazorTextWriter(TextWriter.Null, Encoding.UTF8);
            var target = new RazorTextWriter(TextWriter.Null, Encoding.UTF8);

            // Act
            source.Write("Hello world");
            source.Write(new char[1], 0, 1);
            source.CopyTo(target, new HtmlEncoder());

            // Assert
            // Make sure content was written to the source.
            Assert.Equal(2, source.BufferedWriter.Content.Count());
            Assert.Equal(1, target.BufferedWriter.Content.Count());
            Assert.Same(source.BufferedWriter.Content, Assert.Single(target.BufferedWriter.Content));
        }

        [Fact]
        public void Copy_CopiesContent_IfTargetTextWriterIsARazorTextWriterAndNotBuffering()
        {
            // Arrange
            var unbufferedWriter = new Mock<TextWriter>();
            var source = new RazorTextWriter(TextWriter.Null, Encoding.UTF8);
            var target = new RazorTextWriter(unbufferedWriter.Object, Encoding.UTF8);

            // Act
            target.Flush();
            source.Write("Hello world");
            source.Write(new[] { 'a', 'b', 'c', 'd' }, 1, 2);
            source.CopyTo(target, new HtmlEncoder());

            // Assert
            // Make sure content was written to the source.
            Assert.Equal(2, source.BufferedWriter.Content.Count());
            Assert.Empty(target.BufferedWriter.Content.ToString());
            unbufferedWriter.Verify(v => v.Write("Hello world"), Times.Once());
            unbufferedWriter.Verify(v => v.Write("bc"), Times.Once());
        }

        [Fact]
        public void Copy_WritesContent_IfTargetTextWriterIsNotARazorTextWriter()
        {
            // Arrange
            var source = new RazorTextWriter(TextWriter.Null, Encoding.UTF8);
            var target = new StringWriter();
            var expected = @"Hello world
abc";

            // Act
            source.WriteLine("Hello world");
            source.Write(new[] { 'x', 'a', 'b', 'c' }, 1, 3);
            source.CopyTo(target, new HtmlEncoder());

            // Assert
            Assert.Equal(expected, target.ToString());
        }

        [Fact]
        public async Task CopyAsync_WritesContent_IfTargetTextWriterIsARazorTextWriterAndBuffering()
        {
            // Arrange
            var source = new RazorTextWriter(TextWriter.Null, Encoding.UTF8);
            var target = new RazorTextWriter(TextWriter.Null, Encoding.UTF8);

            // Act
            source.WriteLine("Hello world");
            source.Write(new[] { 'x', 'a', 'b', 'c' }, 1, 3);
            await source.CopyToAsync(target, new HtmlEncoder());

            // Assert
            Assert.Equal(3, source.BufferedWriter.Content.Count());
            Assert.Equal(1, target.BufferedWriter.Content.Count());
            Assert.Equal(source.BufferedWriter.Content, Assert.Single(target.BufferedWriter.Content));
        }

        //[Fact]
        // IHtmlContent currently does not support async writes. Hence disabling this test.
        public async Task CopyAsync_WritesContent_IfTargetTextWriterIsARazorTextWriterAndNotBuffering()
        {
            // Arrange
            var unbufferedWriter = new Mock<TextWriter>();
            var source = new RazorTextWriter(TextWriter.Null, Encoding.UTF8);
            var target = new RazorTextWriter(unbufferedWriter.Object, Encoding.UTF8);

            // Act
            await target.FlushAsync();
            source.WriteLine("Hello from Asp.Net");
            await source.WriteAsync(new[] { 'x', 'y', 'z', 'u' }, 0, 3);
            await source.CopyToAsync(target, new HtmlEncoder());

            // Assert
            // Make sure content was written to the source.
            Assert.Equal(3, source.BufferedWriter.Content.Count());
            Assert.Empty(target.BufferedWriter.Content.ToString());
            unbufferedWriter.Verify(v => v.WriteAsync("Hello from Asp.Net"), Times.Once());
            unbufferedWriter.Verify(v => v.WriteAsync(Environment.NewLine), Times.Once());
            unbufferedWriter.Verify(v => v.WriteAsync("xyz"), Times.Once());
        }

        [Fact]
        public async Task CopyAsync_WritesContent_IfTargetTextWriterIsNotARazorTextWriter()
        {
            // Arrange
            var source = new RazorTextWriter(TextWriter.Null, Encoding.UTF8);
            var target = new StringWriter();
            var expected = @"Hello world
";

            // Act
            source.Write("Hello ");
            await source.WriteLineAsync(new[] { 'w', 'o', 'r', 'l', 'd' });
            await source.CopyToAsync(target, new HtmlEncoder());

            // Assert
            Assert.Equal(expected, target.ToString());
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