// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Testing;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core
{
    public class RazorViewEngineOptionsTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void SettingViewExtensionToNullOrEmptyThrows(string value)
        {
            // Arrange
            var options = new RazorViewEngineOptions();

            // Act and Assert
            var ex = ExceptionAssert.ThrowsArgumentNullOrEmpty(() =>
            {
                options.ViewExtension = value;
            }, "ViewExtension");
        }

        [Theory]
        [InlineData("test-extension0", ".test-extension0")]
        [InlineData(".test-extension1", ".test-extension1")]
        public void ViewExtensionPrependsDotIfNotSpecified(string value, string expected)
        {
            // Arrange
            var options = new RazorViewEngineOptions();

            // Act
            options.ViewExtension = value;

            // Assert
            Assert.Equal(expected, options.ViewExtension);
        }
    }
}