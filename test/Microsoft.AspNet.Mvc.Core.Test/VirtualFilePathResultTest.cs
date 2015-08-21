// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class VirtualFilePathResultTest
    {
        [Fact]
        public void Constructor_SetsFileName()
        {
            // Arrange & Act
            var path = Path.GetFullPath("helllo.txt");
            var result = new VirtualFilePathResult(path, "text/plain");

            // Act & Assert
            Assert.Equal(path, result.FileName);
        }

        [Fact]
        public async Task ExecuteResultAsync_FallsBackToWebRootFileProvider_IfNoFileProviderIsPresent()
        {
            // Arrange
            var path = Path.Combine("TestFiles", "FilePathResultTestFile.txt");
            var result = new VirtualFilePathResult(path, "text/plain");

            var appEnvironment = new Mock<IHostingEnvironment>();
            appEnvironment.Setup(app => app.WebRootFileProvider)
                .Returns(new PhysicalFileProvider(Directory.GetCurrentDirectory()));

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            httpContext.RequestServices = new ServiceCollection()
                .AddInstance<IHostingEnvironment>(appEnvironment.Object)
                .BuildServiceProvider();
            var context = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            // Act
            await result.ExecuteResultAsync(context);
            httpContext.Response.Body.Position = 0;

            // Assert
            Assert.NotNull(httpContext.Response.Body);
            var contents = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
            Assert.Equal("FilePathResultTestFile contents", contents);
        }

        [Fact]
        public async Task ExecuteResultAsync_SetsSuppliedContentTypeAndEncoding()
        {
            // Arrange
            var expectedContentType = "text/foo; charset=us-ascii";
            var result = new VirtualFilePathResult("FilePathResultTestFile_ASCII.txt", MediaTypeHeaderValue.Parse(expectedContentType))
            {
                FileProvider = new PhysicalFileProvider(Path.GetFullPath("TestFiles")),
            };

            var httpContext = new DefaultHttpContext();
            var memoryStream = new MemoryStream();
            httpContext.Response.Body = memoryStream;
            var context = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            // Act
            await result.ExecuteResultAsync(context);

            // Assert
            var contents = Encoding.ASCII.GetString(memoryStream.ToArray());
            Assert.Equal("FilePathResultTestFile contents ASCII encoded", contents);
            Assert.Equal(expectedContentType, httpContext.Response.ContentType);
        }

        [Fact]
        public async Task ExecuteResultAsync_ReturnsFileContentsForRelativePaths()
        {
            // Arrange
            var path = Path.Combine("TestFiles", "FilePathResultTestFile.txt");
            var result = new VirtualFilePathResult(path, "text/plain")
            {
                FileProvider = new PhysicalFileProvider(Path.GetFullPath(".")),
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            var context = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            // Act
            await result.ExecuteResultAsync(context);
            httpContext.Response.Body.Position = 0;

            // Assert
            Assert.NotNull(httpContext.Response.Body);
            var contents = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
            Assert.Equal("FilePathResultTestFile contents", contents);
        }

        [Theory]
        [InlineData("FilePathResultTestFile.txt", "TestFiles")]
        [InlineData("TestFiles/FilePathResultTestFile.txt", ".")]
        [InlineData("TestFiles\\FilePathResultTestFile.txt", ".")]
        [InlineData("~/FilePathResultTestFile.txt", "TestFiles")]
        [InlineData("~/TestFiles/FilePathResultTestFile.txt", ".")]
        [InlineData("~/TestFiles\\FilePathResultTestFile.txt", ".")]
        [InlineData("~\\TestFiles\\FilePathResultTestFile.txt", ".")]
        public async Task ExecuteResultAsync_ReturnsFiles_ForDifferentPaths(string path, string fileProviderRoot)
        {
            // Arrange
            var result = new VirtualFilePathResult(path, "text/plain")
            {
                FileProvider = new PhysicalFileProvider(Path.GetFullPath(fileProviderRoot)),
            };
            var httpContext = new DefaultHttpContext();
            var memoryStream = new MemoryStream();
            httpContext.Response.Body = memoryStream;

            var context = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            // Act
            await result.ExecuteResultAsync(context);
            httpContext.Response.Body.Position = 0;

            // Assert
            var contents = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
            Assert.Equal("FilePathResultTestFile contents", contents);
        }

        [Fact]
        public async Task ExecuteResultAsync_WorksWithNonDiskBasedFiles()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var expectedData = "This is an embedded resource";
            var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(expectedData));

            var nonDiskFileInfo = new Mock<IFileInfo>();
            nonDiskFileInfo.SetupGet(fi => fi.Exists).Returns(true);
            nonDiskFileInfo.SetupGet(fi => fi.PhysicalPath).Returns(() => null); // set null to indicate non-disk file
            nonDiskFileInfo.Setup(fi => fi.CreateReadStream()).Returns(sourceStream);
            var nonDiskFileProvider = new Mock<IFileProvider>();
            nonDiskFileProvider.Setup(fp => fp.GetFileInfo(It.IsAny<string>())).Returns(nonDiskFileInfo.Object);

            var filePathResult = new VirtualFilePathResult("/SampleEmbeddedFile.txt", "text/plain")
            {
                FileProvider = nonDiskFileProvider.Object
            };

            // Act
            await filePathResult.ExecuteResultAsync(actionContext);

            // Assert
            httpContext.Response.Body.Position = 0;
            var contents = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
            Assert.Equal(expectedData, contents);
        }

        [Theory]
        // Root of the file system, forward slash and back slash
        [InlineData("FilePathResultTestFile.txt")]
        [InlineData("/FilePathResultTestFile.txt")]
        [InlineData("\\FilePathResultTestFile.txt")]
        // Paths with subfolders and mixed slash kinds
        [InlineData("/SubFolder/SubFolderTestFile.txt")]
        [InlineData("\\SubFolder\\SubFolderTestFile.txt")]
        [InlineData("/SubFolder\\SubFolderTestFile.txt")]
        [InlineData("\\SubFolder/SubFolderTestFile.txt")]
        // '.' has no special meaning
        [InlineData("./FilePathResultTestFile.txt")]
        [InlineData(".\\FilePathResultTestFile.txt")]
        [InlineData("./SubFolder/SubFolderTestFile.txt")]
        [InlineData(".\\SubFolder\\SubFolderTestFile.txt")]
        [InlineData("./SubFolder\\SubFolderTestFile.txt")]
        [InlineData(".\\SubFolder/SubFolderTestFile.txt")]
        // Traverse to the parent directory and back to the file system directory
        [InlineData("..\\TestFiles/FilePathResultTestFile.txt")]
        [InlineData("..\\TestFiles\\FilePathResultTestFile.txt")]
        [InlineData("..\\TestFiles/SubFolder/SubFolderTestFile.txt")]
        [InlineData("..\\TestFiles\\SubFolder\\SubFolderTestFile.txt")]
        [InlineData("..\\TestFiles/SubFolder\\SubFolderTestFile.txt")]
        [InlineData("..\\TestFiles\\SubFolder/SubFolderTestFile.txt")]
        // '~/' and '~\' mean the application root folder
        [InlineData("~/FilePathResultTestFile.txt")]
        [InlineData("~/SubFolder/SubFolderTestFile.txt")]
        [InlineData("~/SubFolder\\SubFolderTestFile.txt")]
        public async Task GetFilePath_ThrowsFileNotFound_IfFileProviderCanNotFindTheFile(string path)
        {
            // Arrange
            // Point the IFileProvider root to a different subfolder
            var fileProvider = new PhysicalFileProvider(Path.GetFullPath("./Properties"));
            var filePathResult = new VirtualFilePathResult(path, "text/plain")
            {
                FileProvider = fileProvider,
            };

            var expectedMessage = "Could not find file: " + path;
            var context = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());

            // Act
            var ex = await Assert.ThrowsAsync<FileNotFoundException>(() => filePathResult.ExecuteResultAsync(context));

            // Assert
            Assert.Equal(expectedMessage, ex.Message);
            Assert.Equal(path, ex.FileName);
        }

        [Theory]
        [InlineData("~/hello/world.txt", "/hello/world.txt")]
        [InlineData("~\\hello/world.txt", "/hello/world.txt")]
        [InlineData("~\\hello\\world.txt", "/hello/world.txt")]
        [InlineData("\\hello\\world.txt", "/hello/world.txt")]
        public void NormalizePath_HandlesSlashesAndTilde(string input, string expected)
        {
            // Arrange
            var result = new VirtualFilePathResult(input, "text/plain");

            // Act
            var normalizedPath = result.NormalizePath(input);

            // Assert
            Assert.Equal(expected, normalizedPath);
        }
    }
}