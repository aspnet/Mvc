// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Collections.Generic;
using Microsoft.AspNet.FileSystems;
using Moq;
using Xunit;
using System.Text;

namespace Microsoft.AspNet.Mvc.Razor.Test
{
    public class CompilationResultTest
    {
        [Fact]
        public void FailedResult_ThrowsWhenAccessingCompiledType()
        {
            // Arrange
            var expected = @"Compilation for 'myfile' failed:
hello
world";
            var originalContent = "Original file content";
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(f => f.PhysicalPath)
                    .Returns("myfile");
            var contentBytes = Encoding.UTF8.GetBytes(originalContent);
            fileInfo.Setup(f => f.CreateReadStream())
                    .Returns(new MemoryStream(contentBytes));
            var result = CompilationResult.Failed(fileInfo.Object,
                                                 "<h1>hello world</h1>",
                                                 new[] { new CompilationMessage("hello"), new CompilationMessage("world") },
                                                 new Dictionary<string, object> { { "key", "value" } });

            // Act
            var ex = Assert.Throws<CompilationFailedException>(() => result.CompiledType);

            // Assert
            Assert.Equal(expected, ex.Message);
            Assert.Equal("value", ex.Data["key"]);
            Assert.Equal(originalContent, ex.FileContent);
        }
    }
}