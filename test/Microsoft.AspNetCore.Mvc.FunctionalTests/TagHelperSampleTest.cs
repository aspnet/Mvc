// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class TagHelperSampleTest : IClassFixture<MvcSampleFixture<TagHelperSample.Web.Startup>>
    {
        public TagHelperSampleTest(MvcSampleFixture<TagHelperSample.Web.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Fact]
        public async Task HomeController_Index_ReturnsExpectedContent()
        {
            // Arrange
            var resetResponse = await Client.PostAsync("http://localhost/Home/Reset", content: null);

            // Guard 1 (start from scratch)
            AssertRedirectsToHome(resetResponse);

            var createBillyContent = CreateUserFormContent("Billy", "2000-11-28", 0, "hello");
            var createBillyResponse = await Client.PostAsync("http://localhost/Home/Create", createBillyContent);

            // Guard 2 (ensure user 0 exists)
            AssertRedirectsToHome(createBillyResponse);

            var createBobbyContent = CreateUserFormContent("Bobby", "1999-10-27", 1, "howdy");
            var createBobbyResponse = await Client.PostAsync("http://localhost/Home/Create", createBobbyContent);

            // Guard 3 (ensure user 1 exists)
            AssertRedirectsToHome(createBobbyResponse);

            var expectedMediaType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");
            var outputFile = "compiler/resources/TagHelperSample.Web.Home.Index.html";
            var resourceAssembly = typeof(TagHelperSampleTest).GetTypeInfo().Assembly;
            var expectedContent = await ResourceFile.ReadResourceAsync(resourceAssembly, outputFile, sourceFile: false);

            // Act
            var response = await Client.GetAsync("http://localhost/");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedMediaType, response.Content.Headers.ContentType);

#if GENERATE_BASELINES
            ResourceFile.UpdateFile(resourceAssembly, outputFile, expectedContent, responseContent);
#else
            Assert.Equal(
                PlatformNormalizer.NormalizeContent(expectedContent),
                responseContent,
                ignoreLineEndingDifferences: true);
#endif
        }

        [Fact]
        public async Task HomeController_Index_ReturnsExpectedContent_AfterReset()
        {
            // Arrange
            var resetResponse = await Client.PostAsync("http://localhost/Home/Reset", content: null);

            // Guard (start from scratch)
            AssertRedirectsToHome(resetResponse);

            var expectedMediaType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");
            var outputFile = "compiler/resources/TagHelperSample.Web.Home.Index-Reset.html";
            var resourceAssembly = typeof(TagHelperSampleTest).GetTypeInfo().Assembly;
            var expectedContent = await ResourceFile.ReadResourceAsync(resourceAssembly, outputFile, sourceFile: false);

            // Act
            var response = await Client.GetAsync("http://localhost/");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedMediaType, response.Content.Headers.ContentType);

#if GENERATE_BASELINES
            ResourceFile.UpdateFile(resourceAssembly, outputFile, expectedContent, responseContent);
#else
            Assert.Equal(expectedContent, responseContent, ignoreLineEndingDifferences: true);
#endif
        }

        [Fact]
        public async Task HomeController_Create_Get_ReturnsSuccess()
        {
            // Act
            var response = await Client.GetAsync("http://localhost/Home/Create");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task HomeController_Create_Post_ReturnsSuccess()
        {
            // Arrange
            var createBillyContent = CreateUserFormContent("Billy", "2000-11-30", 0, "hello");

            // Act
            var response = await Client.PostAsync("http://localhost/Home/Create", createBillyContent);

            // Assert
            AssertRedirectsToHome(response);
        }

        [Fact]
        public async Task HomeController_Edit_Get_ReturnsSuccess()
        {
            // Arrange
            var createBillyContent = CreateUserFormContent("Billy", "2000-11-30", 0, "hello");
            var createBilly = await Client.PostAsync("http://localhost/Home/Create", createBillyContent);

            // Guard (ensure user 0 exists)
            AssertRedirectsToHome(createBilly);

            // Act
            var response = await Client.GetAsync("http://localhost/Home/Edit/0");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task HomeController_Edit_Post_ReturnsSuccess()
        {
            // Arrange
            var createBillyContent = CreateUserFormContent("Billy", "2000-11-30", 0, "hello");
            var createBilly = await Client.PostAsync("http://localhost/Home/Create", createBillyContent);

            // Guard (ensure user 0 exists)
            AssertRedirectsToHome(createBilly);

            var changeBillyContent = CreateUserFormContent("Bobby", "1999-11-30", 1, "howdy");

            // Act
            var changeBilly = await Client.PostAsync("http://localhost/Home/Edit/0", changeBillyContent);

            // Assert
            AssertRedirectsToHome(changeBilly);
        }

        [Fact]
        public async Task HomeController_Reset_ReturnsSuccess()
        {
            // Arrange and Act
            var response = await Client.PostAsync("http://localhost/Home/Reset", content: null);

            // Assert
            AssertRedirectsToHome(response);
        }

        public static TheoryData MoviesControllerPageData
        {
            get
            {
                return new TheoryData<Func<HttpClient, Task<HttpResponseMessage>>>
                {
                    async (client) =>  await client.GetAsync("http://localhost/Movies"),
                    async (client) =>  await client.PostAsync(
                        "http://localhost/Movies/UpdateMovieRatings",
                        new FormUrlEncodedContent(Enumerable.Empty<KeyValuePair<string, string>>())),
                    async (client) =>  await client.PostAsync(
                        "http://localhost/Movies/UpdateCriticsQuotes",
                        new FormUrlEncodedContent(Enumerable.Empty<KeyValuePair<string, string>>())),
                    async (client) =>
                    {
                        await client.PostAsync(
                            "http://localhost/Movies/UpdateCriticsQuotes",
                            new FormUrlEncodedContent(Enumerable.Empty<KeyValuePair<string, string>>()));
                        await client.PostAsync(
                            "http://localhost/Movies/UpdateCriticsQuotes",
                            new FormUrlEncodedContent(Enumerable.Empty<KeyValuePair<string, string>>()));

                        return await client.GetAsync("http://localhost/Movies/Index");
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(MoviesControllerPageData))]
        public async Task MoviesController_Pages_ReturnSuccess(Func<HttpClient, Task<HttpResponseMessage>> requestPage)
        {
            // Act
            var response = await requestPage(Client);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TagHelperController_ConditionalComment_ReturnsExpectedContent()
        {
            // Arrange
            var expectedMediaType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");
            var outputFile = "compiler/resources/TagHelperSample.Web.TagHelper.ConditionalComment.html";
            var resourceAssembly = typeof(TagHelperSampleTest).GetTypeInfo().Assembly;
            var expectedContent = await ResourceFile.ReadResourceAsync(resourceAssembly, outputFile, sourceFile: false);

            // Act
            var response = await Client.GetAsync("http://localhost/TagHelper/ConditionalComment");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedMediaType, response.Content.Headers.ContentType);

#if GENERATE_BASELINES
            ResourceFile.UpdateFile(resourceAssembly, outputFile, expectedContent, responseContent);
#else
            Assert.Equal(expectedContent, responseContent, ignoreLineEndingDifferences: true);
#endif
        }

        private static void AssertRedirectsToHome(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            var redirectLocations = response.Headers.GetValues("Location");
            var redirectsTo = Assert.Single(redirectLocations);
            Assert.Equal("/", redirectsTo, StringComparer.Ordinal);
        }

        private static HttpContent CreateUserFormContent(
            string name,
            string dateOfBirth,
            int yearsEmployeed,
            string blurb)
        {
            var form = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Name", name),
                new KeyValuePair<string, string>("DateOfBirth", dateOfBirth),
                new KeyValuePair<string, string>("YearsEmployeed", yearsEmployeed.ToString()),
                new KeyValuePair<string, string>("Blurb", blurb),
            };
            var content = new FormUrlEncodedContent(form);

            return content;
        }
    }
}