// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class EmbeddedViewSampleTest : IClassFixture<MvcTestFixture<EmbeddedViewSample.Web.Startup>>
    {
        public EmbeddedViewSampleTest(MvcTestFixture<EmbeddedViewSample.Web.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Fact]
        public async Task RazorViewEngine_UsesFileProviderOnViewEngineOptionsToLocateViews()
        {
            // Arrange
            var expectedMessage = "Hello test-user, this is /Home";

            // Act
            var response = await Client.GetStringAsync("http://localhost/Home?User=test-user");

            // Assert
            Assert.Equal(expectedMessage, response);
        }

        [Fact]
        public async Task RazorViewEngine_UsesFileProviderOnViewEngineOptionsToLocateAreaViews()
        {
            // Arrange
            var expectedMessage = "Hello admin-user, this is /Restricted/Admin/Login";
            var target = "http://localhost/Restricted/Admin/Login?AdminUser=admin-user";

            // Act
            var response = await Client.GetStringAsync(target);

            // Assert
            Assert.Equal(expectedMessage, response);
        }
    }
}