// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.Framework.WebEncoders.Testing;
using Xunit;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class StringHtmlContentTest
    {
        [Fact]
        public void CreateStringHtmlContent()
        {
            // Arrange & Act
            var content = new StringHtmlContent("Hello World");

            // Assert
            var result = Assert.IsType<StringHtmlContent>(content);
            Assert.Equal("Hello World", result.ToString());
        }

        [Fact]
        public void ToString_ReturnsAString()
        {
            // Arrange & Act
            var content = new StringHtmlContent("Hello World");

            // Assert
            Assert.Equal("Hello World", content.ToString());
        }

        [Fact]
        public void ToString_ReturnsNullForNullInput()
        {
            // Arrange & Act
            var content = new StringHtmlContent(null);

            // Assert
            Assert.Null(content.ToString());
        }

        [Fact]
        public void CreateStringHtmlContent_InternalConstructor_WritesWithoutEncoding()
        {
            // Arrange & Act
            var content = new StringHtmlContent("Hello World", encodeOnWrite: false);

            // Assert
            using (var writer = new StringWriter())
            {
                content.WriteTo(writer, new CommonTestEncoder());
                Assert.Equal("Hello World", writer.ToString());
            }
        }

        [Fact]
        public void WriteTo_WritesContent()
        {
            // Arrange & Act
            var content = new StringHtmlContent("Hello World");

            // Assert
            using (var writer = new StringWriter())
            {
                content.WriteTo(writer, new CommonTestEncoder());
                Assert.Equal("HtmlEncode[[Hello World]]", writer.ToString());
            }
        }
    }
}
