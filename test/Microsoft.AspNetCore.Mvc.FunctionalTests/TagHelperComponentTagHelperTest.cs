﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class TagHelperComponentTagHelperTest : IClassFixture<MvcTestFixture<RazorWebSite.Startup>>
    {
        private static readonly Assembly _resourcesAssembly = typeof(TagHelperComponentTagHelperTest).GetTypeInfo().Assembly;

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
            var outputFile = "compiler/resources/RazorWebSite.TagHelperComponent.Index.html";
            var expectedContent =
                await ResourceFile.ReadResourceAsync(_resourcesAssembly, outputFile, sourceFile: false);

            // Act
            var response = await Client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

#if GENERATE_BASELINES
            ResourceFile.UpdateFile(_resourcesAssembly, outputFile, expectedContent, responseContent);
#else
            Assert.Equal(expectedContent, responseContent, ignoreLineEndingDifferences: true);
#endif
        }
    }
}
