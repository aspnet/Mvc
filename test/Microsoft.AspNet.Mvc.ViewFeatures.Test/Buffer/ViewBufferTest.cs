// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Html;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewFeatures.Buffer;
using Microsoft.Extensions.WebEncoders.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.Buffer
{
    public class ViewBufferTest
    {
        [Fact]
        public void Append_AddsStringRazorValue()
        {
            // Arrange
            var buffer = new ViewBuffer(new TestViewBufferScope(), "some-name");

            // Act
            buffer.Append("Hello world");

            // Assert
            var page = Assert.Single(buffer.Pages);
            Assert.Equal(1, page.Count);
            Assert.Equal("Hello world", page.Buffer[0].Value);
        }

        [Fact]
        public void Append_AddsHtmlContentRazorValue()
        {
            // Arrange
            var buffer = new ViewBuffer(new TestViewBufferScope(), "some-name");
            var content = new HtmlString("hello-world");

            // Act
            buffer.AppendHtml(content);

            // Assert
            var page = Assert.Single(buffer.Pages);
            Assert.Equal(1, page.Count);
            Assert.Same(content, page.Buffer[0].Value);
        }

        [Fact]
        public void AppendHtml_AddsHtmlStringValues()
        {
            // Arrange
            var buffer = new ViewBuffer(new TestViewBufferScope(), "some-name");
            var value = "Hello world";

            // Act
            buffer.AppendHtml(value);

            // Assert
            var page = Assert.Single(buffer.Pages);
            Assert.Equal(1, page.Count);
            var htmlString = Assert.IsType<HtmlString>(page.Buffer[0].Value);
            Assert.Equal("Hello world", htmlString.ToString());
        }

        [Fact]
        public void Append_CreatesNewPages_WhenCurrentPageIsFull()
        {
            // Arrange
            var buffer = new ViewBuffer(new TestViewBufferScope(), "some-name");
            var expected = Enumerable.Range(0, TestViewBufferScope.DefaultBufferSize).Select(i => i.ToString());

            // Act
            foreach (var item in expected)
            {
                buffer.Append(item);
            }
            buffer.Append("Hello");
            buffer.Append("world");

            // Assert
            Assert.Equal(2, buffer.Pages[0].Count);
            Assert.Collection(buffer.Pages,
                page => Assert.Equal(expected, page.Buffer.Select(v => v.Value)),
                page =>
                {
                    var array = page.Buffer;
                    Assert.Equal("Hello", array[0].Value);
                    Assert.Equal("world", array[1].Value);
                });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(TestViewBufferScope.DefaultBufferSize + 3)]
        public void Clear_ResetsBackingBufferAndIndex(int valuesToWrite)
        {
            // Arrange
            var buffer = new ViewBuffer(new TestViewBufferScope(), "some-name");

            // Act
            for (var i = 0; i < valuesToWrite; i++)
            {
                buffer.Append("Hello");
            }
            buffer.Clear();
            buffer.Append("world");

            // Assert
            var page = Assert.Single(buffer.Pages);
            Assert.Equal(1, page.Count);
            Assert.Equal("world", page.Buffer[0].Value);
        }

        [Fact]
        public void WriteTo_WritesSelf_WhenWriterIsHtmlTextWriter()
        {
            // Arrange
            var buffer = new ViewBuffer(new TestViewBufferScope(), "some-name");
            var htmlWriter = new Mock<HtmlTextWriter>();
            htmlWriter.Setup(w => w.Write(buffer)).Verifiable();

            // Act
            buffer.Append("Hello world");
            buffer.WriteTo(htmlWriter.Object, new HtmlTestEncoder());

            // Assert
            htmlWriter.Verify();
        }

        [Fact]
        public void WriteTo_WritesRazorValues_ToTextWriter()
        {
            // Arrange
            var buffer = new ViewBuffer(new TestViewBufferScope(), "some-name");
            var writer = new StringWriter();

            // Act
            buffer.Append("Hello");
            buffer.AppendHtml(new HtmlString(" world"));
            buffer.AppendHtml(" 123");
            buffer.WriteTo(writer, new HtmlTestEncoder());

            // Assert
            Assert.Equal("Hello world 123", writer.ToString());
        }

        [Theory]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(23)]
        public void WriteTo_WritesRazorValuesFromAllBuffers(int valuesToWrite)
        {
            // Arrange
            var buffer = new ViewBuffer(new TestViewBufferScope(4), "some-name");
            var writer = new StringWriter();
            var expected = string.Join("", Enumerable.Range(0, valuesToWrite).Select(_ => "abc"));

            // Act
            for (var i = 0; i < valuesToWrite; i++)
            {
                buffer.AppendHtml("abc");
            }
            buffer.WriteTo(writer, new HtmlTestEncoder());

            // Assert
            Assert.Equal(expected, writer.ToString());
        }

        [Fact]
        public async Task WriteToAsync_WritesSelf_WhenWriterIsHtmlTextWriter()
        {
            // Arrange
            var buffer = new ViewBuffer(new TestViewBufferScope(), "some-name");
            var htmlWriter = new Mock<HtmlTextWriter>();
            htmlWriter.Setup(w => w.Write(buffer)).Verifiable();

            // Act
            buffer.Append("Hello world");
            await buffer.WriteToAsync(htmlWriter.Object, new HtmlTestEncoder());

            // Assert
            htmlWriter.Verify();
        }

        [Fact]
        public async Task WriteToAsync_WritesRazorValues_ToTextWriter()
        {
            // Arrange
            var buffer = new ViewBuffer(new TestViewBufferScope(), "some-name");
            var writer = new StringWriter();

            // Act
            buffer.Append("Hello");
            buffer.AppendHtml(new HtmlString(" world"));
            buffer.AppendHtml(" 123");

            await buffer.WriteToAsync(writer, new HtmlTestEncoder());

            // Assert
            Assert.Equal("Hello world 123", writer.ToString());
        }

        [Theory]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(23)]
        public async Task WriteToAsync_WritesRazorValuesFromAllBuffers(int valuesToWrite)
        {
            // Arrange
            var buffer = new ViewBuffer(new TestViewBufferScope(4), "some-name");
            var writer = new StringWriter();
            var expected = string.Join("", Enumerable.Range(0, valuesToWrite).Select(_ => "abc"));

            // Act
            for (var i = 0; i < valuesToWrite; i++)
            {
                buffer.AppendHtml("abc");
            }

            await buffer.WriteToAsync(writer, new HtmlTestEncoder());

            // Assert
            Assert.Equal(expected, writer.ToString());
        }
    }
}
