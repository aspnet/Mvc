// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public class DefaultRazorViewEngineFileProviderAccessorTest
    {
        [Fact]
        public void FileProvider_ReturnsInstanceIfExactlyOneFileProviderIsSpecified()
        {
            // Arrange
            var fileProvider = new TestFileProvider();
            var options = new RazorViewEngineOptions();
            options.FileProviders.Add(fileProvider);
            var optionsAccessor = new Mock<IOptions<RazorViewEngineOptions>>();
            optionsAccessor.SetupGet(o => o.Value).Returns(options);
            var fileProviderAccessor = new DefaultRazorViewEngineFileProviderAccessor(optionsAccessor.Object);

            // Act
            var actual = fileProviderAccessor.FileProvider;

            // Assert
            Assert.Same(fileProvider, actual);
        }

        [Fact]
        public void Constructor_ThrowsIfNoInstancesAreRegistered()
        {
            // Arrange
            var expected =
                $"'{typeof(RazorViewEngineOptions).FullName}.{nameof(RazorViewEngineOptions.FileProviders)}' must " +
                $"not be empty. At least one '{typeof(IFileProvider).FullName}' is required to locate a view for " +
                "rendering." + Environment.NewLine +
                "Parameter name: optionsAccessor";
            var options = new RazorViewEngineOptions();
            var optionsAccessor = new Mock<IOptions<RazorViewEngineOptions>>();
            optionsAccessor.SetupGet(o => o.Value).Returns(options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                "optionsAccessor",
                () => new DefaultRazorViewEngineFileProviderAccessor(optionsAccessor.Object));
            Assert.Equal(expected, exception.Message);
        }

        [Fact]
        public void FileProvider_ReturnsCompositeFileProviderIfMoreThanOneInstanceIsRegistered()
        {
            // Arrange
            var options = new RazorViewEngineOptions();
            options.FileProviders.Add(new TestFileProvider());
            options.FileProviders.Add(new TestFileProvider());
            var optionsAccessor = new Mock<IOptions<RazorViewEngineOptions>>();
            optionsAccessor.SetupGet(o => o.Value).Returns(options);
            var fileProviderAccessor = new DefaultRazorViewEngineFileProviderAccessor(optionsAccessor.Object);

            // Act
            var actual = fileProviderAccessor.FileProvider;

            // Assert
            Assert.IsType<CompositeFileProvider>(actual);
        }
    }
}