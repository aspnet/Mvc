﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public class FileProviderRazorProjectTest
    {
        [Fact]
        public void EnumerateFiles_ReturnsEmptySequenceIfNoCshtmlFilesArePresent()
        {
            // Arrange
            var fileProvider = new TestFileProvider("BasePath");
            var file1 = fileProvider.AddFile("/File1.txt", "content");
            var file2 = fileProvider.AddFile("/File2.js", "content");
            fileProvider.AddDirectoryContent("/", new IFileInfo[] { file1, file2 });

            var accessor = Mock.Of<IRazorViewEngineFileProviderAccessor>(a => a.FileProvider == fileProvider);

            var razorProject = new FileProviderRazorProject(accessor, Mock.Of<IHostingEnvironment>(e => e.ContentRootPath == "BasePath"));

            // Act
            var razorFiles = razorProject.EnumerateItems("/");

            // Assert
            Assert.Empty(razorFiles);
        }

        [Fact]
        public void EnumerateFiles_ReturnsCshtmlFiles()
        {
            // Arrange
            var fileProvider = new TestFileProvider("BasePath");
            var file1 = fileProvider.AddFile("/File1.cshtml", "content");
            var file2 = fileProvider.AddFile("/File2.js", "content");
            var file3 = fileProvider.AddFile("/File3.cshtml", "content");
            fileProvider.AddDirectoryContent("/", new IFileInfo[] { file1, file2, file3 });

            var accessor = Mock.Of<IRazorViewEngineFileProviderAccessor>(a => a.FileProvider == fileProvider);

            var razorProject = new FileProviderRazorProject(accessor, Mock.Of<IHostingEnvironment>(e => e.ContentRootPath == "BasePath"));

            // Act
            var razorFiles = razorProject.EnumerateItems("/");

            // Assert
            Assert.Collection(
                razorFiles.OrderBy(f => f.FilePath),
                file =>
                {
                    Assert.Equal("/File1.cshtml", file.FilePath);
                    Assert.Equal("/", file.BasePath);
                    Assert.Equal(Path.Combine("BasePath", "File1.cshtml"), file.PhysicalPath);
                    Assert.Equal("File1.cshtml", file.RelativePhysicalPath);
                },
                file =>
                {
                    Assert.Equal("/File3.cshtml", file.FilePath);
                    Assert.Equal("/", file.BasePath);
                    Assert.Equal(Path.Combine("BasePath", "File3.cshtml"), file.PhysicalPath);
                    Assert.Equal("File3.cshtml", file.RelativePhysicalPath);
                });
        }

        [Fact]
        public void EnumerateFiles_IteratesOverAllCshtmlUnderRoot()
        {
            // Arrange
            var fileProvider = new TestFileProvider("BasePath");
            var directory1 = new TestDirectoryFileInfo
            {
                Name = "Level1-Dir1",
            };
            var file1 = fileProvider.AddFile("File1.cshtml", "content");
            var directory2 = new TestDirectoryFileInfo
            {
                Name = "Level1-Dir2",
            };
            fileProvider.AddDirectoryContent("/", new IFileInfo[] { directory1, file1, directory2 });

            var file2 = fileProvider.AddFile("/Level1-Dir1/File2.cshtml", "content");
            var file3 = fileProvider.AddFile("/Level1-Dir1/File3.cshtml", "content");
            var file4 = fileProvider.AddFile("/Level1-Dir1/File4.txt", "content");
            var directory3 = new TestDirectoryFileInfo
            {
                Name = "Level2-Dir1"
            };
            fileProvider.AddDirectoryContent("/Level1-Dir1", new IFileInfo[] { file2, directory3, file3, file4 });
            var file5 = fileProvider.AddFile(Path.Combine("Level1-Dir2", "File5.cshtml"), "content");
            fileProvider.AddDirectoryContent("/Level1-Dir2", new IFileInfo[] { file5 });
            fileProvider.AddDirectoryContent("/Level1/Level2", new IFileInfo[0]);

            var accessor = Mock.Of<IRazorViewEngineFileProviderAccessor>(a => a.FileProvider == fileProvider);

            var razorProject = new FileProviderRazorProject(accessor, Mock.Of<IHostingEnvironment>(e => e.ContentRootPath == "BasePath"));

            // Act
            var razorFiles = razorProject.EnumerateItems("/");

            // Assert
            Assert.Collection(razorFiles.OrderBy(f => f.FilePath),
                 file =>
                 {
                     Assert.Equal("/File1.cshtml", file.FilePath);
                     Assert.Equal("/", file.BasePath);
                     Assert.Equal(Path.Combine("BasePath", "File1.cshtml"), file.PhysicalPath);
                     Assert.Equal("File1.cshtml", file.RelativePhysicalPath);
                 },
                file =>
                {
                    Assert.Equal("/Level1-Dir1/File2.cshtml", file.FilePath);
                    Assert.Equal("/", file.BasePath);
                    Assert.Equal(Path.Combine("BasePath", "Level1-Dir1", "File2.cshtml"), file.PhysicalPath);
                    Assert.Equal(Path.Combine("Level1-Dir1", "File2.cshtml"), file.RelativePhysicalPath);
                },
                file =>
                {
                    Assert.Equal("/Level1-Dir1/File3.cshtml", file.FilePath);
                    Assert.Equal("/", file.BasePath);
                    Assert.Equal(Path.Combine("BasePath", "Level1-Dir1", "File3.cshtml"), file.PhysicalPath);
                    Assert.Equal(Path.Combine("Level1-Dir1", "File3.cshtml"), file.RelativePhysicalPath);
                },
                file =>
                {
                    Assert.Equal("/Level1-Dir2/File5.cshtml", file.FilePath);
                    Assert.Equal("/", file.BasePath);
                    Assert.Equal(Path.Combine("BasePath", "Level1-Dir2", "File5.cshtml"), file.PhysicalPath);
                    Assert.Equal(Path.Combine("Level1-Dir2", "File5.cshtml"), file.RelativePhysicalPath);
                });
        }

        [Fact]
        public void EnumerateFiles_IteratesOverAllCshtmlUnderPath()
        {
            // Arrange
            var fileProvider = new TestFileProvider("BasePath");
            var directory1 = new TestDirectoryFileInfo
            {
                Name = "Level1-Dir1",
            };
            var file1 = fileProvider.AddFile("/File1.cshtml", "content");
            var directory2 = new TestDirectoryFileInfo
            {
                Name = "Level1-Dir2",
            };
            fileProvider.AddDirectoryContent("/", new IFileInfo[] { directory1, file1, directory2 });

            var file2 = fileProvider.AddFile("/Level1-Dir1/File2.cshtml", "content");
            var file3 = fileProvider.AddFile("/Level1-Dir1/File3.cshtml", "content");
            var file4 = fileProvider.AddFile("/Level1-Dir1/File4.txt", "content");
            var directory3 = new TestDirectoryFileInfo
            {
                Name = "Level2-Dir1"
            };
            fileProvider.AddDirectoryContent("/Level1-Dir1", new IFileInfo[] { file2, directory3, file3, file4 });
            var file5 = fileProvider.AddFile(Path.Combine("Level1-Dir2", "File5.cshtml"), "content");
            fileProvider.AddDirectoryContent("/Level1-Dir2", new IFileInfo[] { file5 });
            fileProvider.AddDirectoryContent("/Level1/Level2", new IFileInfo[0]);

            var accessor = Mock.Of<IRazorViewEngineFileProviderAccessor>(a => a.FileProvider == fileProvider);

            var razorProject = new FileProviderRazorProject(accessor, Mock.Of<IHostingEnvironment>(e => e.ContentRootPath == "BasePath"));

            // Act
            var razorFiles = razorProject.EnumerateItems("/Level1-Dir1");

            // Assert
            Assert.Collection(razorFiles.OrderBy(f => f.FilePath),
                file =>
                {
                    Assert.Equal("/File2.cshtml", file.FilePath);
                    Assert.Equal("/Level1-Dir1", file.BasePath);
                    Assert.Equal(Path.Combine("BasePath", "Level1-Dir1", "File2.cshtml"), file.PhysicalPath);
                    Assert.Equal(Path.Combine("Level1-Dir1", "File2.cshtml"), file.RelativePhysicalPath);
                },
                file =>
                {
                    Assert.Equal("/File3.cshtml", file.FilePath);
                    Assert.Equal("/Level1-Dir1", file.BasePath);
                    Assert.Equal(Path.Combine("BasePath", "Level1-Dir1", "File3.cshtml"), file.PhysicalPath);
                    Assert.Equal(Path.Combine("Level1-Dir1", "File3.cshtml"), file.RelativePhysicalPath);
                });
        }

        [Fact]
        public void GetItem_ReturnsFileFromDisk()
        {
            var fileProvider = new TestFileProvider("BasePath");
            var file1 = fileProvider.AddFile("/File1.cshtml", "content");
            var file2 = fileProvider.AddFile("/File2.js", "content");
            var file3 = fileProvider.AddFile("/File3.cshtml", "content");
            fileProvider.AddDirectoryContent("/", new IFileInfo[] { file1, file2, file3 });

            var accessor = Mock.Of<IRazorViewEngineFileProviderAccessor>(a => a.FileProvider == fileProvider);

            var razorProject = new FileProviderRazorProject(accessor, Mock.Of<IHostingEnvironment>(e => e.ContentRootPath == "BasePath"));

            // Act
            var item = razorProject.GetItem("/File3.cshtml");

            // Assert
            Assert.True(item.Exists);
            Assert.Equal("/File3.cshtml", item.FilePath);
            Assert.Equal(string.Empty, item.BasePath);
            Assert.Equal(Path.Combine("BasePath", "File3.cshtml"), item.PhysicalPath);
            Assert.Equal("File3.cshtml", item.RelativePhysicalPath);
        }

        [Fact]
        public void GetItem_PhysicalPathDoesNotStartWithContentRoot_ReturnsNull()
        {
            var fileProvider = new TestFileProvider("BasePath2");
            var file1 = fileProvider.AddFile("/File1.cshtml", "content");
            var file2 = fileProvider.AddFile("/File2.js", "content");
            var file3 = fileProvider.AddFile("/File3.cshtml", "content");
            fileProvider.AddDirectoryContent("/", new IFileInfo[] { file1, file2, file3 });

            var accessor = Mock.Of<IRazorViewEngineFileProviderAccessor>(a => a.FileProvider == fileProvider);

            var razorProject = new FileProviderRazorProject(accessor, Mock.Of<IHostingEnvironment>(e => e.ContentRootPath == "BasePath"));

            // Act
            var item = razorProject.GetItem("/File3.cshtml");

            // Assert
            Assert.True(item.Exists);
            Assert.Equal("/File3.cshtml", item.FilePath);
            Assert.Equal(string.Empty, item.BasePath);
            Assert.Equal(Path.Combine("BasePath2", "File3.cshtml"), item.PhysicalPath);
            Assert.Null(item.RelativePhysicalPath);
        }

        [Fact]
        public void GetItem_ReturnsNotFoundResult()
        {
            // Arrange
            var fileProvider = new TestFileProvider("BasePath");
            var file = fileProvider.AddFile("/SomeFile.cshtml", "content");
            fileProvider.AddDirectoryContent("/", new IFileInfo[] { file });
            var accessor = Mock.Of<IRazorViewEngineFileProviderAccessor>(a => a.FileProvider == fileProvider);

            var razorProject = new FileProviderRazorProject(accessor, Mock.Of<IHostingEnvironment>(e => e.ContentRootPath == "BasePath"));

            // Act
            var item = razorProject.GetItem("/NotFound.cshtml");

            // Assert
            Assert.False(item.Exists);
        }
    }
}
