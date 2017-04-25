﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class RazorPagesWithBasePathTest : IClassFixture<MvcTestFixture<RazorPagesWebSite.StartupWithBasePath>>
    {
        public RazorPagesWithBasePathTest(MvcTestFixture<RazorPagesWebSite.StartupWithBasePath> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Fact]
        public async Task PageOutsideBasePath_IsNotRouteable()
        {
            // Act
            var response = await Client.GetAsync("/HelloWorld");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task IndexAtBasePath_IsRouteableAtRoot()
        {
            // Act
            var response = await Client.GetAsync("/");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello from /Index", content.Trim());
        }

        [Fact]
        public async Task IndexAtBasePath_IsRouteableViaIndex()
        {
            // Act
            var response = await Client.GetAsync("/Index");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello from /Index", content.Trim());
        }

        [Fact]
        public async Task IndexInSubdirectory_IsRouteableViaDirectoryName()
        {
            // Act
            var response = await Client.GetAsync("/Admin/Index");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello from /Admin/Index", content.Trim());
        }

        [Fact]
        public async Task PageWithRouteTemplateInSubdirectory_IsRouteable()
        {
            // Act
            var response = await Client.GetAsync("/Admin/RouteTemplate/1/MyRouteSuffix/");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello from /Admin/RouteTemplate 1", content.Trim());
        }

        [Fact]
        public async Task PageWithRouteTemplateInSubdirectory_IsRouteable_WithOptionalParameters()
        {
            // Act
            var response = await Client.GetAsync("/Admin/RouteTemplate/my-user-id/MyRouteSuffix/4");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello from /Admin/RouteTemplate my-user-id 4", content.Trim());
        }

        [Fact]
        public async Task AuthConvention_IsAppliedOnBasePathRelativePaths_ForFiles()
        {
            // Act
            var response = await Client.GetAsync("/Conventions/Auth");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Login?ReturnUrl=%2FConventions%2FAuth", response.Headers.Location.PathAndQuery);
        }

        [Fact]
        public async Task AuthConvention_IsAppliedOnBasePathRelativePaths_For_Folders()
        {
            // Act
            var response = await Client.GetAsync("/Conventions/AuthFolder");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Login?ReturnUrl=%2FConventions%2FAuthFolder", response.Headers.Location.PathAndQuery);
        }

        [Fact]
        public async Task ViewStart_IsDiscoveredWhenRootDirectoryIsSpecified()
        {
            // Test for https://github.com/aspnet/Mvc/issues/5915
            //Arrange
            var expected = $"Hello from _ViewStart{Environment.NewLine}Hello from /Pages/WithViewStart/Index.cshtml!";

            // Act
            var response = await Client.GetStringAsync("/WithViewStart");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task ViewImport_IsDiscoveredWhenRootDirectoryIsSpecified()
        {
            // Test for https://github.com/aspnet/Mvc/issues/5915
            //Arrange
            var expected = "Hello from CustomService!";

            // Act
            var response = await Client.GetStringAsync("/WithViewImport");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task FormTagHelper_WithPage_GeneratesLinksToSelf()
        {
            //Arrange
            var expected = "<form method=\"POST\" action=\"/TagHelper/SelfPost/10\">";

            // Act
            var response = await Client.GetStringAsync("/TagHelper/SelfPost");

            // Assert
            Assert.Contains(expected, response.Trim());
        }

        [Fact]
        public async Task FormTagHelper_WithPage_AllowsPostingToAnotherPage()
        {
            //Arrange
            var expected = "<form method=\"POST\" action=\"/TagHelper/SelfPost/10\">";

            // Act
            var response = await Client.GetStringAsync("/TagHelper/CrossPost");

            // Assert
            Assert.Contains(expected, response.Trim());
        }

        [Fact]
        public async Task FormActionTagHelper_WithPage_AllowsPostingToAnotherPage()
        {
            //Arrange
            var expected = 
@"<button formaction=""/TagHelper/CrossPost/10"" />
<input type=""submit"" formaction=""/TagHelper/CrossPost/10"" />
<input type=""image"" formaction=""/TagHelper/CrossPost/10"" />";

            // Act
            var response = await Client.GetStringAsync("/TagHelper/FormAction");

            // Assert
            Assert.Equal(expected, response.Trim());
        }

        [Fact]
        public async Task RedirectFromPage_RedirectsToPathWithoutIndexSegment()
        {
            //Arrange
            var expected = "/Redirects";

            // Act
            var response = await Client.GetAsync("/Redirects/Index");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task RedirectFromPage_ToIndex_RedirectsToPathWithoutIndexSegment()
        {
            //Arrange
            var expected = "/Redirects";

            // Act
            var response = await Client.GetAsync("/Redirects/RedirectToIndex");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal(expected, response.Headers.Location.ToString());
        }

        [Fact]
        public async Task PageRoute_UsingDefaultPageNameToRoute()
        {
            // Arrange
            var expected = @"<a href=""/Routes/Sibling/10"">Link</a>";

            // Act
            var response = await Client.GetStringAsync("/Routes/RouteUsingDefaultName");

            // Assert
            Assert.Equal(expected, response.Trim());
        }
    }
}
