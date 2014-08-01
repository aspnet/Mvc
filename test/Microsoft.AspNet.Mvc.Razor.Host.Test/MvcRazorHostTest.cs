// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.Host.Test
{
    public class MvcRazorHostTest
    {
        [Fact]
        public void GetCodeTree_ReturnsParsedCodeTree()
        {
            // Arrange
            var content = @"
@model Foo
@inject IHtmlHelper Foo
@{
    Layout = ""/Layout.cshtml"";
}
";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var host = new MvcRazorHost("someBasetype");

            // Act
            var result = host.GetCodeTree("/views/home/view.cshtml", stream);

            // Assert
            Assert.IsType<ModelChunk>(result[1]);
            Assert.IsType<InjectChunk>(result[2]);
        }

        [Fact]
        public void GetCodeTree_DoesNotCloseStream()
        {
            // Arrange
            var host = new MvcRazorHost("MyBaseType");
            var stream = new Mock<MemoryStream> { CallBase = true };

            // Act
            host.GetCodeTree("somepath.cshtml", stream.Object);

            // Assert
            stream.Verify(s => s.Close(), Times.Never());
        }

        [Fact]
        public void GenerateCode_DoesNotCloseStream()
        {
            // Arrange
            var host = new MvcRazorHost("MyBaseType");
            var stream = new Mock<MemoryStream> { CallBase = true };

            // Act
            host.GenerateCode("somepath.cshtml", stream.Object);

            // Assert
            stream.Verify(s => s.Close(), Times.Never());
        }
    }
}