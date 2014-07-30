﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Testing;
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
            var writer = new RazorTextWriter(Encoding.UTF8);

            // Act
            writer.Write(true);
            writer.Write(3);
            writer.Write(ulong.MaxValue);
            writer.Write(new TestClass());
            writer.Write(3.14);
            writer.Write(2.718m);
            writer.Write('m');

            // Assert
            Assert.Equal(expected, writer.Buffer.BufferEntries);
        }

        [Fact]
        [ReplaceCulture]
        public void WriteLine_WritesDataTypes_ToBuffer()
        {
            // Arrange 
            var newLine = Environment.NewLine;
            var expected = new[] { "False", newLine, "1.1", newLine, "3", newLine };
            var writer = new RazorTextWriter(Encoding.UTF8);

            // Act
            writer.WriteLine(false);
            writer.WriteLine(1.1f);
            writer.WriteLine(3L);

            // Assert
            Assert.Equal(expected, writer.Buffer.BufferEntries);
        }

        [Fact]
        public async Task Write_WritesCharBuffer()
        {
            // Arrange
            var input1 = new ArraySegment<char>(new char[] { 'a', 'b', 'c', 'd' }, 1, 3);
            var input2 = new ArraySegment<char>(new char[] { 'e', 'f' }, 0, 2);
            var input3 = new ArraySegment<char>(new char[] { 'g', 'h', 'i', 'j' }, 3, 1);
            var writer = new RazorTextWriter(Encoding.UTF8);

            // Act
            writer.Write(input1.Array, input1.Offset, input1.Count);
            await writer.WriteAsync(input2.Array, input2.Offset, input2.Count);
            await writer.WriteLineAsync(input3.Array, input3.Offset, input3.Count);

            // Assert
            var buffer = writer.Buffer.BufferEntries;
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
            var writer = new RazorTextWriter(Encoding.UTF8);

            // Act
            writer.WriteLine();
            await writer.WriteLineAsync();

            // Assert
            var actual = writer.Buffer.BufferEntries;
            Assert.Equal(new[] { newLine, newLine }, actual);
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
            var writer = new RazorTextWriter(Encoding.UTF8);

            // Act
            writer.Write(input1);
            writer.WriteLine(input2);
            await writer.WriteAsync(input3);
            await writer.WriteLineAsync(input4);

            // Assert
            var actual = writer.Buffer.BufferEntries;
            Assert.Equal(new[] { input1, input2, newLine, input3, input4, newLine }, actual);
        }

        [Fact]
        public void Copy_CopiesContent_IfTargetTextWriterIsARazorTextWriter()
        {
            // Arrange
            var source = new RazorTextWriter(Encoding.UTF8);
            var target = new RazorTextWriter(Encoding.UTF8);

            // Act
            source.Write("Hello world");
            source.Write(new char[1], 0, 1);
            source.CopyTo(target);

            // Assert
            // Make sure content was written to the source.
            Assert.Equal(2, source.Buffer.BufferEntries.Count);
            Assert.Equal(1, target.Buffer.BufferEntries.Count);
            Assert.Same(source.Buffer.BufferEntries, target.Buffer.BufferEntries[0]);
        }

        [Fact]
        public void Copy_WritesContent_IfTargetTextWriterIsNotARazorTextWriter()
        {
            // Arrange
            var source = new RazorTextWriter(Encoding.UTF8);
            var target = new StringWriter();
            var expected = @"Hello world
abc";

            // Act
            source.WriteLine("Hello world");
            source.Write(new[] { 'x', 'a', 'b', 'c' }, 1, 3);
            source.CopyTo(target);

            // Assert
            Assert.Equal(expected, target.ToString());
        }

        [Fact]
        public async Task CopyAsync_WritesContent_IfTargetTextWriterIsARazorTextWriter()
        {
            // Arrange
            var source = new RazorTextWriter(Encoding.UTF8);
            var target = new RazorTextWriter(Encoding.UTF8);

            // Act
            source.WriteLine("Hello world");
            source.Write(new[] { 'x', 'a', 'b', 'c' }, 1, 3);
            await source.CopyToAsync(target);

            // Assert
            Assert.Equal(3, source.Buffer.BufferEntries.Count);
            Assert.Equal(1, target.Buffer.BufferEntries.Count);
            Assert.Same(source.Buffer.BufferEntries, target.Buffer.BufferEntries[0]);
        }

        [Fact]
        public async Task CopyAsync_WritesContent_IfTargetTextWriterIsNotARazorTextWriter()
        {
            // Arrange
            var source = new RazorTextWriter(Encoding.UTF8);
            var target = new StringWriter();
            var expected = @"Hello world
";

            // Act
            source.Write("Hello ");
            await source.WriteLineAsync(new[] { 'w', 'o', 'r', 'l', 'd' });
            await source.CopyToAsync(target);

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