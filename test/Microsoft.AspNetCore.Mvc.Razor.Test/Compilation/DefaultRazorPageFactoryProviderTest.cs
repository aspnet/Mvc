// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    public class DefaultRazorPageFactoryProviderTest
    {
        [Fact]
        public void CreateFactory_ReturnsViewDescriptor_ForUnsuccessfulResults()
        {
            // Arrange
            var path = "/file-does-not-exist";
            var expirationTokens = new[]
            {
                Mock.Of<IChangeToken>(),
                Mock.Of<IChangeToken>(),
            };
            var descriptor = new CompiledViewDescriptor
            {
                RelativePath = path,
                ExpirationTokens = expirationTokens,
            };
            var compilerCache = new Mock<IViewCompiler>();
            compilerCache
                .Setup(f => f.CompileAsync(It.IsAny<string>()))
                .ReturnsAsync(descriptor);

            var factoryProvider = new DefaultRazorPageFactoryProvider(GetCompilerProvider(compilerCache.Object));

            // Act
            var result = factoryProvider.CreateFactory(path);

            // Assert
            Assert.False(result.Success);
            Assert.Same(descriptor, result.ViewDescriptor);
        }

        [Fact]
        public void CreateFactory_ReturnsViewDescriptor_ForSuccessfulResults()
        {
            // Arrange
            var relativePath = "/file-exists";
            var expirationTokens = new[]
            {
                Mock.Of<IChangeToken>(),
                Mock.Of<IChangeToken>(),
            };
            var descriptor = new CompiledViewDescriptor
            {
                RelativePath = relativePath,
                ViewAttribute = new RazorViewAttribute(relativePath, typeof(TestRazorPage)),
                ExpirationTokens = expirationTokens,
            };
            var compilerCache = new Mock<IViewCompiler>();
            compilerCache
                .Setup(f => f.CompileAsync(It.IsAny<string>()))
                .ReturnsAsync(descriptor);

            var factoryProvider = new DefaultRazorPageFactoryProvider(GetCompilerProvider(compilerCache.Object));

            // Act
            var result = factoryProvider.CreateFactory(relativePath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(expirationTokens, descriptor.ExpirationTokens);
        }

        [Fact]
        public void CreateFactory_ProducesDelegateThatSetsPagePath()
        {
            // Arrange
            var relativePath = "/file-exists";
            var descriptor = new CompiledViewDescriptor
            {
                RelativePath = relativePath,
                ViewAttribute = new RazorViewAttribute(relativePath, typeof(TestRazorPage)),
                ExpirationTokens = Array.Empty<IChangeToken>(),
            };
            var viewCompiler = new Mock<IViewCompiler>();
            viewCompiler
                .Setup(f => f.CompileAsync(It.IsAny<string>()))
                .ReturnsAsync(descriptor);

            var factoryProvider = new DefaultRazorPageFactoryProvider(GetCompilerProvider(viewCompiler.Object));

            // Act
            var result = factoryProvider.CreateFactory(relativePath);

            // Assert
            Assert.True(result.Success);
            var actual = result.RazorPageFactory();
            Assert.Equal("/file-exists", actual.Path);
        }

        private IViewCompilerProvider GetCompilerProvider(IViewCompiler cache)
        {
            var compilerCacheProvider = new Mock<IViewCompilerProvider>();
            compilerCacheProvider
                .Setup(c => c.GetCompiler())
                .Returns(cache);

            return compilerCacheProvider.Object;
        }

        private class TestRazorPage : RazorPage
        {
            public override Task ExecuteAsync()
            {
                throw new NotImplementedException();
            }
        }
    }
}
