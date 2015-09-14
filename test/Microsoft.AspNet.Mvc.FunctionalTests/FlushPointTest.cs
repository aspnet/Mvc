// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using RazorWebSite;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class FlushPointTest
    {
        private const string SiteName = nameof(RazorWebSite);
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;
        private readonly Action<IServiceCollection> _configureServices = new Startup().ConfigureServices;

        [Fact]
        public async Task FlushPointsAreExecutedForPagesWithLayouts()
        {
            var waitService = new WaitService();
            var server = TestHelper.CreateServer(_app, SiteName,
                services =>
                {
                    _configureServices(services);
                    services.AddInstance(waitService);
                });
            var client = server.CreateClient();

            // Act
            var stream = await client.GetStreamAsync("http://localhost/FlushPoint/PageWithLayout");

            // Assert - 1
            Assert.Equal(@"<title>Page With Layout</title>", GetTrimmedString(stream));
            waitService.WaitForServer();

            // Assert - 2
            Assert.Equal(@"RenderBody content", GetTrimmedString(stream));
            waitService.WaitForServer();

            // Assert - 3
            Assert.Equal(@"<span>Content that takes time to produce</span>",
                        GetTrimmedString(stream));
        }

        [Fact]
        public async Task FlushPointsAreExecutedForPagesWithoutLayouts()
        {
            var waitService = new WaitService();
            var server = TestHelper.CreateServer(_app, SiteName,
                services =>
                {
                    _configureServices(services);
                    services.AddInstance(waitService);
                });
            var client = server.CreateClient();

            // Act
            var stream = await client.GetStreamAsync("http://localhost/FlushPoint/PageWithoutLayout");

            // Assert - 1
            Assert.Equal("Initial content", GetTrimmedString(stream));
            waitService.WaitForServer();

            // Assert - 2
            Assert.Equal("Secondary content", GetTrimmedString(stream));
            waitService.WaitForServer();

            // Assert - 3
            Assert.Equal("Inside partial", GetTrimmedString(stream));
            waitService.WaitForServer();

            // Assert - 4
            Assert.Equal(
                @"After flush inside partial<form action=""/FlushPoint/PageWithoutLayout"" method=""post"">" +
                @"<input id=""Name1"" name=""Name1"" type=""text"" value="""" />",
                GetTrimmedString(stream),
                ignoreLineEndingDifferences: true);
            waitService.WaitForServer();

            // Assert - 5
            Assert.Equal(
                @"<input id=""Name2"" name=""Name2"" type=""text"" value="""" /></form>",
                GetTrimmedString(stream));
        }

        [Theory]
        [InlineData("PageWithPartialsAndViewComponents", "FlushAsync invoked inside RenderSection")]
        [InlineData("PageWithRenderSectionAsync", "FlushAsync invoked inside RenderSectionAsync")]
        public async Task FlushPointsAreExecutedForPagesWithComponentsPartialsAndSections(string action, string title)
        {
            var waitService = new WaitService();
            var server = TestHelper.CreateServer(_app, SiteName,
                services =>
                {
                    _configureServices(services);
                    services.AddInstance(waitService);
                });
            var client = server.CreateClient();

            // Act
            var stream = await client.GetStreamAsync("http://localhost/FlushPoint/" + action);

            // Assert - 1
            Assert.Equal(
                $@"<title>{ title }</title>
RenderBody content",
                GetTrimmedString(stream),
                ignoreLineEndingDifferences: true);
            waitService.WaitForServer();

            // Assert - 2
            Assert.Equal(
                @"partial-content

Value from TaskReturningString
<p>section-content</p>",
                GetTrimmedString(stream),
                ignoreLineEndingDifferences: true);
            waitService.WaitForServer();

            // Assert - 3
            Assert.Equal(
                @"component-content
    <span>Content that takes time to produce</span>

More content from layout",
                GetTrimmedString(stream),
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public async Task FlushPointsNestedLayout()
        {
            // Arrange
            var waitService = new WaitService();
            var server = TestHelper.CreateServer(_app, SiteName,
                services =>
                {
                    _configureServices(services);
                    services.AddInstance(waitService);
                });
            var client = server.CreateClient();

            // Act
            var stream = await client.GetStreamAsync("http://localhost/FlushPoint/PageWithNestedLayout");

            // Assert - 1
            Assert.Equal(
                @"Inside Nested Layout
<title>Nested Page With Layout</title>",
                GetTrimmedString(stream),
                ignoreLineEndingDifferences: true);
            waitService.WaitForServer();

            // Assert - 2
            Assert.Equal("<span>Nested content that takes time to produce</span>", GetTrimmedString(stream));
        }

        [Fact]
        public async Task FlushBeforeCallingLayout()
        {
            var waitService = new WaitService();
            var server = TestHelper.CreateServer(_app, SiteName,
                services =>
                {
                    _configureServices(services);
                    services.AddInstance(waitService);
                });
            var client = server.CreateClient();

             var expectedMessage = "Layout page '/Views/FlushPoint/PageWithFlushBeforeLayout.cshtml'" +
                " cannot be rendered after 'FlushAsync' has been invoked.";

            // Act
            var stream = await client.GetStreamAsync("http://localhost/FlushPoint/PageWithFlushBeforeLayout");

            // Assert - 1
            Assert.Equal("Initial content", GetTrimmedString(stream));
            waitService.WaitForServer();

            // Assert - 2
            try
            {
                GetTrimmedString(stream);
            }
            catch (Exception ex)
            {
                Assert.Equal(expectedMessage, ex.InnerException.Message);
            }
        }

        private string GetTrimmedString(Stream stream)
        {
            var buffer = new byte[1024];
            var count = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, count).Trim();
        }
    }
}
