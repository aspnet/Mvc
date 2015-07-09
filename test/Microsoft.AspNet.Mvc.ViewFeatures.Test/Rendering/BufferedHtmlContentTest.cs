// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using Microsoft.AspNet.Html.Abstractions;
using Microsoft.Framework.WebEncoders;
using Microsoft.Framework.WebEncoders.Testing;
using Xunit;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class BufferedHtmlContentTest
    {
        [Fact]
        public void AppendString_AppendsAString()
        {
            // Arrange
            var content = new BufferedHtmlContent();
            var writer = new StringWriter();

            // Act
            content.Append("Hello");

            // Assert
            var result = Assert.Single(content);
            Assert.IsType(typeof(string), result);
        }

        [Fact]
        public void AppendCharArray_AppendsAsString()
        {
            // Arrange
            var content = new BufferedHtmlContent();
            var writer = new StringWriter();

            // Act
            content.Append(new char[] { 'h', 'e', 'l', 'l', 'o' }, 0, 5);

            // Assert
            var result = Assert.Single(content);
            Assert.IsType(typeof(string), result);
        }

        [Fact]
        public void AppendIHtmlContent_AppendsAsIs()
        {
            // Arrange
            var content = new BufferedHtmlContent();
            var writer = new StringWriter();

            // Act
            content.Append(new TestHtmlContent("Hello"));

            // Assert
            var result = Assert.Single(content);
            var testHtmlContent = Assert.IsType<TestHtmlContent>(result);
            testHtmlContent.WriteTo(writer, new CommonTestEncoder());
            Assert.Equal("Written from TestHtmlContent: Hello", writer.ToString());
            writer.Dispose();
        }

        [Fact]
        public void CanAppendMultipleItems()
        {
            // Arrange
            var content = new BufferedHtmlContent();
            var writer = new StringWriter();

            // Act
            content.Append(new TestHtmlContent("hello"));
            content.Append("Test");

            // Assert
            Assert.Equal(2, content.Count());
            content.WriteTo(writer, new CommonTestEncoder());
            Assert.Equal("Written from TestHtmlContent: helloTest", writer.ToString());
        }

        [Fact]
        public void Clear_DeletesAllItems()
        {
            // Arrange
            var content = new BufferedHtmlContent();
            content.Append(new TestHtmlContent("hello"));
            content.Append("Test");

            // Act
            content.Clear();

            // Assert
            Assert.Equal(0, content.Count());
        }

        [Fact]
        public void WriteTo_WritesAllItems()
        {
            // Arrange
            var content = new BufferedHtmlContent();
            var writer = new StringWriter();
            content.Append(new TestHtmlContent("Hello"));
            content.Append("Test");

            // Act
            content.WriteTo(writer, new CommonTestEncoder());

            // Assert
            Assert.Equal(2, content.Count());
            Assert.Equal("Written from TestHtmlContent: HelloTest", writer.ToString());
        }

        [Fact]
        public void CanIterateThroughItems()
        {
            // Arrange
            var content = new BufferedHtmlContent();
            var writer = new StringWriter();
            content.Append(new TestHtmlContent("Hello"));
            content.Append("Test");

            // Act & Assert
            foreach (var item in content)
            {
                Assert.True(
                    typeof(IHtmlContent).IsAssignableFrom(item.GetType()) ||
                    typeof(string).IsAssignableFrom(item.GetType()));
            }
        }

        private class TestHtmlContent : IHtmlContent
        {
            private string _content;

            public TestHtmlContent(string content)
            {
                _content = content;
            }

            public void WriteTo(TextWriter writer, IHtmlEncoder encoder)
            {
                writer.Write("Written from TestHtmlContent: " + _content);
            }
        }
    }
}
