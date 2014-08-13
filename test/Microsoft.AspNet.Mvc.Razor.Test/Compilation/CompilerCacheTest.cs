// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.FileSystems;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.Test
{
    public class CompilerCacheTest
    {
        [Fact]
        public void GetOrAdd_ReturnsCompilationResultFromFactory()
        {
            // Arrange
            var cache = new CompilerCache();
            var fileInfo = Mock.Of<IFileInfo>();
            var type = GetType();
            var expected = UncachedCompilationResult.Successful(type, "hello world");

            // Act
            var actual = cache.GetOrAdd(fileInfo, () => expected);

            // Assert
            Assert.Same(expected, actual);
            Assert.Equal("hello world", expected.CompiledContent);
            Assert.Same(type, expected.CompiledType);
        }

        [Fact]
        public void GetOrAdd_DoesNotCacheCompiledContent()
        {
            // Arrange
            var lastModified = DateTime.UtcNow;
            var cache = new CompilerCache();
            var fileInfo = new Mock<IFileInfo>();
            fileInfo.SetupGet(f => f.PhysicalPath)
                    .Returns("test");
            fileInfo.SetupGet(f => f.LastModified)
                    .Returns(lastModified);
            var type = GetType();
            var expected = UncachedCompilationResult.Successful(type, "hello world");

            // Act
            cache.GetOrAdd(fileInfo.Object, () => expected);
            var actual = cache.GetOrAdd(fileInfo.Object, () => expected);

            // Assert
            Assert.NotSame(expected, actual);
            var result = Assert.IsType<CompilationResult>(actual);
            Assert.Null(actual.CompiledContent);
            Assert.Same(type, expected.CompiledType);
        }
    }
}