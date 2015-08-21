// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Routing;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class PhysicalPhysicalFilePathResultTest
    {
        [Fact]
        public void Constructor_SetsFileName()
        {
            // Arrange & Act
            var path = Path.GetFullPath("helllo.txt");
            var result = new PhysicalFilePathResult(path, "text/plain");

            // Act & Assert
            Assert.Equal(path, result.FileName);
        }

        [Fact]
        public async Task ExecuteResultAsync_FallsbackToStreamCopy_IfNoIHttpSendFilePresent()
        {
            // Arrange
            var path = Path.GetFullPath(Path.Combine("TestFiles", "FilePathResultTestFile.txt"));
            var result = new PhysicalFilePathResult(path, "text/plain");
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

        [Fact]
        public async Task ExecuteResultAsync_CallsSendFileAsync_IfIHttpSendFilePresent()
        {
            // Arrange
            var path = Path.GetFullPath(Path.Combine("TestFiles", "FilePathResultTestFile.txt"));
            var result = new PhysicalFilePathResult(path, "text/plain");
            var sendFileMock = new Mock<IHttpSendFileFeature>();
            sendFileMock
                .Setup(s => s.SendFileAsync(path, 0, null, CancellationToken.None))
                .Returns(Task.FromResult<int>(0));

            var httpContext = new DefaultHttpContext();
            httpContext.SetFeature(sendFileMock.Object);
            var context = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            // Act
            await result.ExecuteResultAsync(context);

            // Assert
            sendFileMock.Verify();
        }

        [Fact]
        public async Task ExecuteResultAsync_SetsSuppliedContentTypeAndEncoding()
        {
            // Arrange
            var expectedContentType = "text/foo; charset=us-ascii";
            var path = Path.GetFullPath(Path.Combine(".", "TestFiles", "FilePathResultTestFile_ASCII.txt"));
            var result = new PhysicalFilePathResult(path, MediaTypeHeaderValue.Parse(expectedContentType));

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

        [Theory]
        [InlineData('/', '\\')]
        [InlineData('\\', '/')]
        public async Task ExecuteResultAsync_WorksWithAbsolutePaths_UsingDifferentSlashes(char oldChar, char newChar)
        {
            // Arrange
            var path = Path.GetFullPath(Path.Combine(".", "TestFiles", "FilePathResultTestFile.txt"));
            path = path.Replace(oldChar, newChar);
            var result = new PhysicalFilePathResult(path, "text/plain");

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
        [InlineData("FilePathResultTestFile.txt")]
        [InlineData("./FilePathResultTestFile.txt")]
        [InlineData(".\\FilePathResultTestFile.txt")]
        [InlineData("~/FilePathResultTestFile.txt")]
        [InlineData("..\\TestFiles/FilePathResultTestFile.txt")]
        [InlineData("..\\TestFiles\\FilePathResultTestFile.txt")]
        [InlineData("..\\TestFiles/SubFolder/SubFolderTestFile.txt")]
        [InlineData("..\\TestFiles\\SubFolder\\SubFolderTestFile.txt")]
        [InlineData("..\\TestFiles/SubFolder\\SubFolderTestFile.txt")]
        [InlineData("..\\TestFiles\\SubFolder/SubFolderTestFile.txt")]
        [InlineData("~/SubFolder/SubFolderTestFile.txt")]
        [InlineData("~/SubFolder\\SubFolderTestFile.txt")]
        public async Task ExecuteAsync_ThrowsFileNotFound_ForNonRootedPaths(string path)
        {
            // Arrange
            // Point the IFileProvider root to a different subfolder
            var fileProvider = new PhysicalFileProvider(Path.GetFullPath("./Properties"));
            var result = new PhysicalFilePathResult(path, "text/plain");
            var context = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var expectedMessage = "Could not find file: " + path;

            // Act
            var ex = await Assert.ThrowsAsync<FileNotFoundException>(() => result.ExecuteResultAsync(context));

            // Assert
            Assert.Equal(expectedMessage, ex.Message);
            Assert.Equal(path, ex.FileName);
        }

        [Theory]
        [InlineData("/SubFolder/SubFolderTestFile.txt")]
        [InlineData("\\SubFolder\\SubFolderTestFile.txt")]
        [InlineData("/SubFolder\\SubFolderTestFile.txt")]
        [InlineData("\\SubFolder/SubFolderTestFile.txt")]
        [InlineData("./SubFolder/SubFolderTestFile.txt")]
        [InlineData(".\\SubFolder\\SubFolderTestFile.txt")]
        [InlineData("./SubFolder\\SubFolderTestFile.txt")]
        [InlineData(".\\SubFolder/SubFolderTestFile.txt")]
        public void ExecuteAsync_ThrowsDirectoryNotFound_IfItCanNotFindTheDirectory_ForRootPaths(string path)
        {
            // Arrange
            var result = new PhysicalFilePathResult(path, "text/plain");
            var context = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());

            // Act & Assert
            Assert.ThrowsAsync<DirectoryNotFoundException>(() => result.ExecuteResultAsync(context));
        }

        [Theory]
        [InlineData("/FilePathResultTestFile.txt")]
        [InlineData("\\FilePathResultTestFile.txt")]
        public void ExecuteAsync_ThrowsFileNotFound_WhenFileDoesNotExist_ForRootPaths(string path)
        {
            // Arrange
            var result = new PhysicalFilePathResult(path, "text/plain");
            var context = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());

            // Act & Assert
            Assert.ThrowsAsync<FileNotFoundException>(() => result.ExecuteResultAsync(context));
        }

        [Theory]
        [InlineData("/hello/world.txt", "/hello/world.txt")]
        [InlineData("/hello\\world.txt", "/hello/world.txt")]
        [InlineData("\\hello\\world.txt", "/hello/world.txt")]
        [InlineData("\\hello/world.txt", "/hello/world.txt")]
        public void NormalizePath_HandlesSlashes(string input, string expected)
        {
            // Arrange
            var result = new PhysicalFilePathResult("test", "text/plain");

            // Act
            var normalizedPath = result.NormalizePath(Path.GetFullPath(input));

            // Assert
            Assert.Equal(Path.GetFullPath(expected).Replace('\\', '/'), normalizedPath);
        }
    }
}