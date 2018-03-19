// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class LinkGenerationTests : IClassFixture<MvcTestFixture<BasicWebSite.Startup>>
    {
        // Some tests require comparing the actual response body against an expected response baseline
        // so they require a reference to the assembly on which the resources are located, in order to
        // make the tests less verbose, we get a reference to the assembly with the resources and we
        // use it on all the rest of the tests.
        private static readonly Assembly _resourcesAssembly = typeof(LinkGenerationTests).GetTypeInfo().Assembly;

        public LinkGenerationTests(MvcTestFixture<BasicWebSite.Startup> fixture)
        {
            Client = fixture.CreateDefaultClient();
        }

        public HttpClient Client { get; }

        public static TheoryData<string, string> RelativeLinksData
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { "http://localhost/Home/RedirectToActionReturningTaskAction", "/Home/ActionReturningTask" },
                    { "http://localhost/Home/RedirectToRouteActionAsMethodAction", "/Home/ActionReturningTask" },
                    { "http://localhost/Home/RedirectToRouteUsingRouteName", "/api/orders/10" },
                    { "http://pingüino/Home/RedirectToRouteUsingRouteName", "/api/orders/10" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(RelativeLinksData))]
        public async Task GeneratedLinksWithActionResults_AreRelativeLinks_WhenSetOnLocationHeader(
            string url,
            string expected)
        {
            // Act
            var response = await Client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            // Location.ToString() in mono returns file://url. (https://github.com/aspnet/External/issues/21)
            Assert.Equal(
                TestPlatformHelper.IsMono ? new Uri(expected) : new Uri(expected, UriKind.Relative),
                response.Headers.Location);
        }

        [Fact]
        public async Task GeneratedLinks_AreNotPunyEncoded_WhenGeneratedOnViews()
        {
            // Arrange
            var expectedMediaType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");
            var outputFile = "compiler/resources/BasicWebSite.Home.ActionLinkView.html";
            var expectedContent =
                await ResourceFile.ReadResourceAsync(_resourcesAssembly, outputFile, sourceFile: false);

            // Act
            // The host is not important as everything runs in memory and tests are isolated from each other.
            var response = await Client.GetAsync("http://localhost/Home/ActionLinkView");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedMediaType, response.Content.Headers.ContentType);

#if GENERATE_BASELINES
            ResourceFile.UpdateFile(_resourcesAssembly, outputFile, expectedContent, responseContent);
#else
            Assert.Equal(expectedContent, responseContent, ignoreLineEndingDifferences: true);
#endif
        }
    }
}