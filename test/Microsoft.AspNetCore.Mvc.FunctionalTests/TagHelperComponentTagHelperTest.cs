// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class TagHelperComponentTagHelperTest : IClassFixture<MvcTestFixture<RazorWebSite.Startup>>
    {
        public TagHelperComponentTagHelperTest(MvcTestFixture<RazorWebSite.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Fact]
        public async Task InjectsTestHeadTagHelperComponent()
        {
            // Arrange
            var url = "http://localhost/TagHelperComponent/Index";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var expected = $"<head inject=\"true\">\r\nHello from Tag Helper Component\r\n<script>'This was injected!!'</script></head>";

            // Act
            var response = await Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expected, content);
        }
    }
}
