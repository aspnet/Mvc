// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    // The EmbeddedFileSystem used by EmbeddedViewSample.Web performs case sensitive lookups for files.
    // These tests verify that we correctly normalize route values when constructing view lookup paths.
    public class EmbeddedViewSampleCaseSensitivityTest : IClassFixture<MvcTestFixture<EmbeddedViewSample.Web.Startup>>
    {
        public EmbeddedViewSampleCaseSensitivityTest(MvcTestFixture<EmbeddedViewSample.Web.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Fact]
        public async Task RazorViewEngine_NormalizesActionName_WhenLookingUpViewPaths()
        {
            // Arrange
            var expectedMessage = "Hello test-user, this is /EmbeddedView_Home";

            // Act
            var response = await Client.GetStringAsync("http://localhost/EmbeddedView_Home/index?User=test-user");

            // Assert
            Assert.Equal(expectedMessage, response);
        }

        [Fact]
        public async Task RazorViewEngine_NormalizesControllerRouteValue_WhenLookingUpViewPaths()
        {
            // Arrange
            var expectedMessage = "Hello test-user, this is /embeddedview_home";

            // Act
            var response = await Client.GetStringAsync("http://localhost/embeddedview_home?User=test-user");

            // Assert
            Assert.Equal(expectedMessage, response);
        }

        [Fact]
        public async Task RazorViewEngine_NormalizesAreaRouteValue_WhenLookupViewPaths()
        {
            // Arrange
            var expectedMessage = "Hello admin-user, this is /restricted/embeddedview_admin/login";
            var target = "http://localhost/restricted/embeddedview_admin/login?AdminUser=admin-user";

            // Act
            var response = await Client.GetStringAsync(target);

            // Assert
            Assert.Equal(expectedMessage, response);
        }
    }
}