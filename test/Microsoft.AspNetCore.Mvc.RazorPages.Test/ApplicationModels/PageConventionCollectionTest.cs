﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public class PageConventionCollectionTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void EnsureValidPageName_ThrowsIfPageNameIsNullOrEmpty(string pageName)
        {
            // Act & Assert
            var ex = ExceptionAssert.ThrowsArgument(
                () => PageConventionCollection.EnsureValidPageName(pageName),
                "pageName",
                "Value cannot be null or empty.");
        }

        [Theory]
        [InlineData("path-without-slash")]
        [InlineData(@"c:\myapp\path-without-slash")]
        public void EnsureValidPageName_ThrowsIfPageNameDoesNotStartWithLeadingSlash(string pageName)
        {
            // Arrange
            var expected = $"'{pageName}' is not a valid page name. A page name is path relative to the Razor Pages root directory that starts with a leading forward slasg ('/') and does not contain the file extension.";
            // Act & Assert
            var ex = ExceptionAssert.ThrowsArgument(
                () => PageConventionCollection.EnsureValidPageName(pageName),
                "pageName",
                expected);
        }

        [Fact]
        public void EnsureValidPageName_ThrowsIfPageNameHasExtension()
        {
            // Arrange
            var pageName = "/Page.cshtml";
            var expected = $"'{pageName}' is not a valid page name. A page name is path relative to the Razor Pages root directory that starts with a leading forward slasg ('/') and does not contain the file extension.";
            // Act & Assert
            var ex = ExceptionAssert.ThrowsArgument(
                () => PageConventionCollection.EnsureValidPageName(pageName),
                "pageName",
                expected);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void EnsureValidFolderPath_ThrowsIfPathIsNullOrEmpty(string folderPath)
        {
            // Arrange
            // Act & Assert
            var ex = ExceptionAssert.ThrowsArgument(
                () => PageConventionCollection.EnsureValidFolderPath(folderPath),
                "folderPath",
               "Value cannot be null or empty.");
        }

        [Theory]
        [InlineData("path-without-slash")]
        [InlineData(@"c:\myapp\path-without-slash")]
        public void EnsureValidFolderPath_ThrowsIfPageNameDoesNotStartWithLeadingSlash(string folderPath)
        {
            // Arrange
            // Act & Assert
            var ex = ExceptionAssert.ThrowsArgument(
                () => PageConventionCollection.EnsureValidPageName(folderPath),
                "pageName",
                "Path must be a root relative path that starts with a forward slash '/'.");
        }
    }
}
