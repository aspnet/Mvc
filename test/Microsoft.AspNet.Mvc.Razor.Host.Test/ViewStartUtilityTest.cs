// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.FileSystems;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class ViewStartProviderTest : IDisposable
    {
        private readonly string _rootPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetViewStartLocations_ReturnsEmptySequenceIfViewPathIsEmpty(string viewPath)
        {
            // Act
            var result = ViewStartUtility.GetViewStartLocations(new TestFileSystem(), viewPath);

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("/views/Home/MyView.cshtml")]
        [InlineData("~/views/Home/MyView.cshtml")]
        [InlineData("views/Home/MyView.cshtml")]
        public void GetViewStartLocations_ReturnsPotentialViewStartLocations(string inputPath)
        {
            // Arrange
            var expected = new[]
            {
                @"views\Home\_viewstart.cshtml",
                @"views\_viewstart.cshtml",
                @"_viewstart.cshtml"
            };
            var fileSystemDir = Path.Combine(_rootPath, Path.GetRandomFileName());
            var viewPath = Path.Combine(fileSystemDir, "Views", "Home", "MyView.cshtml");
            Directory.CreateDirectory(Path.GetDirectoryName(viewPath));
            File.WriteAllText(viewPath, string.Empty);
            var fileSystem = new PhysicalFileSystem(fileSystemDir);

            // Act
            var result = ViewStartUtility.GetViewStartLocations(fileSystem, inputPath);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetViewStartLocations_ReturnsPotentialViewStartLocations_IfPathIsAbsolute()
        {
            // Arrange
            var expected = new[]
            {
                @"Areas\MyArea\Sub\Views\Admin\_viewstart.cshtml",
                @"Areas\MyArea\Sub\Views\_viewstart.cshtml",
                @"Areas\MyArea\Sub\_viewstart.cshtml",
                @"Areas\MyArea\_viewstart.cshtml",
                @"Areas\_viewstart.cshtml",
                @"_viewstart.cshtml",
            };
            var fileSystemDir = Path.Combine(_rootPath, Path.GetRandomFileName());
            var viewPath = Path.Combine(fileSystemDir, "Areas", "MyArea", "Sub", "Views", "Admin", "Test.cshtml");
            Directory.CreateDirectory(Path.GetDirectoryName(viewPath));
            File.WriteAllText(viewPath, string.Empty);
            var fileSystem = new PhysicalFileSystem(fileSystemDir);

            // Act
            var result = ViewStartUtility.GetViewStartLocations(fileSystem, viewPath);

            // Assert
            Assert.Equal(expected, result);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_rootPath, recursive: true);
            }
            catch
            {
            }
        }
    }
}