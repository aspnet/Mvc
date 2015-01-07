// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using BasicWebSite;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class TagHelpersTests
    {
        private readonly IServiceProvider _provider = TestHelper.CreateServices("TagHelpersWebSite");
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;

        // Some tests require comparing the actual response body against an expected response baseline
        // so they require a reference to the assembly on which the resources are located, in order to
        // make the tests less verbose, we get a reference to the assembly with the resources and we
        // use it on all the rest of the tests.
        private readonly Assembly _resourcesAssembly = typeof(TagHelpersTests).GetTypeInfo().Assembly;

        [Theory]
        [InlineData("Index")]
        [InlineData("About")]
        [InlineData("Help")]
        public async Task CanRenderViewsWithTagHelpers(string action)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            var expectedMediaType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");

            // The K runtime compiles every file under compiler/resources as a resource at runtime with the same name
            // as the file name, in order to update a baseline you just need to change the file in that folder.
            var expectedContent = await _resourcesAssembly.ReadResourceAsStringAsync(
                "compiler/resources/TagHelpersWebSite.Home." + action + ".html");

            // Act

            // The host is not important as everything runs in memory and tests are isolated from each other.
            var response = await client.GetAsync("http://localhost/Home/" + action);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedMediaType, response.Content.Headers.ContentType);
            Assert.Equal(expectedContent, responseContent);
        }

        [Theory]
        [InlineData("NestedViewStartTagHelper")]
        [InlineData("ViewWithLayoutAndNestedTagHelper")]
        public async Task TagHelpersAreInheritedFromViewStartPages(string action)
        {
            // Arrange
            var expected = string.Join(Environment.NewLine,
                                       "<root>root-content</root>",
                                       "",
                                       "",
                                       "<nested>nested-content</nested>");
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var result = await client.GetStringAsync("http://localhost/Home/" + action);

            // Assert
            Assert.Equal(expected, result.Trim());
        }

        [Fact]
        public async Task ViewsWithModelMetadataAttributes_CanRenderForm()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            var expectedContent = await _resourcesAssembly.ReadResourceAsStringAsync(
                "compiler/resources/TagHelpersWebSite.Employee.Create.html");

            // Act
            var response = await client.GetAsync("http://localhost/Employee/Create");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedContent, responseContent);
        }

        [Fact]
        public async Task ViewsWithModelMetadataAttributes_CanRenderPostedValue()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            var expectedContent = await _resourcesAssembly.ReadResourceAsStringAsync(
                "compiler/resources/TagHelpersWebSite.Employee.Details.AfterCreate.html");
            var validPostValues = new Dictionary<string, string>
            {
                { "FullName", "Boo" },
                { "Gender", "M" },
                { "Age", "22" },
                { "EmployeeId", "0" },
                { "JoinDate", "2014-12-01" },
                { "Email", "a@b.com" },
            };
            var postContent = new FormUrlEncodedContent(validPostValues);

            // Act
            var response = await client.PostAsync("http://localhost/Employee/Create", postContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedContent, responseContent);
        }

        [Fact]
        public async Task ViewsWithModelMetadataAttributes_CanHandleInvalidData()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            var expectedContent = await _resourcesAssembly.ReadResourceAsStringAsync(
                "compiler/resources/TagHelpersWebSite.Employee.Create.Invalid.html");
            var validPostValues = new Dictionary<string, string>
            {
                { "FullName", "Boo" },
                { "Gender", "M" },
                { "Age", "1000" },
                { "EmployeeId", "0" },
                { "Email", "a@b.com" },
                { "Salary", "z" }, 
            };
            var postContent = new FormUrlEncodedContent(validPostValues);

            // Act
            var response = await client.PostAsync("http://localhost/Employee/Create", postContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedContent, responseContent);
        }

        [Fact]
        public async Task CacheTagHelper_CanCachePortionsOfViewsPartialViewsAndViewComponents()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            client.BaseAddress = new Uri("http://localhost");
            client.DefaultRequestHeaders.Add("Locale", "N");

            // Act - 1
            // Verify that content gets cached based on vary-by-params
            var response1 = await client.GetStringAsync("/catalog?categoryId=1&correlationid=1");

            // Assert - 1
            var expected1 =
@"<h2>Category: Laptops</h2>
<h2>Region: North</h2>

    <h2>Cached content</h2>
    Locations closest to your locale:

NorthWest Store
<div>CorrelationId in View Component: 1</div>

    <partial-title>Listing items</partial-title>

Cached Content for Laptops
<div>CorrelationId in Partial: 1</div>

    <div>CorrelationId in Splash: 1</div>";
            Assert.Equal(expected1, response1.Trim());

            // Act - 2
            // Verify content gets changed in partials when one of the vary by parameters is changed
            var response2 = await client.GetStringAsync("/catalog?categoryId=3&correlationid=2");

            // Assert - 2
            var expected2 =
@"<h2>Category: Phones</h2>
<h2>Region: North</h2>

    <h2>Cached content</h2>
    Locations closest to your locale:

NorthWest Store
<div>CorrelationId in View Component: 1</div>

    <partial-title>Listing items</partial-title>

Cached Content for Phones
<div>CorrelationId in Partial: 2</div>

    <div>CorrelationId in Splash: 2</div>";

            Assert.Equal(expected2, response2.Trim());

            // Act - 3
            // Verify content gets changed in a View Component when the Vary-by-header parameters is changed
            client.DefaultRequestHeaders.Add("Locale", "East");
            var response3 = await client.GetStringAsync("/catalog?categoryId=3&correlationid=3");

            // Assert - 2
            var expected3 =
@"<h2>Category: Phones</h2>
<h2>Region: Default</h2>

    <h2>Cached content</h2>
    Locations closest to your locale:

Nationwide Store
<div>CorrelationId in View Component: 3</div>

    <partial-title>Listing items</partial-title>

Cached Content for Phones
<div>CorrelationId in Partial: 2</div>

    <div>CorrelationId in Splash: 3</div>";
            Assert.Equal(expected3, response3.Trim());
        }

        [Fact]
        public async Task CacheTagHelper_ExpiresContent_BasedOnExpiresParameter()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            client.BaseAddress = new Uri("http://localhost");

            // Act - 1
            var response1 = await client.GetStringAsync("/catalog/2");

            // Assert - 1
            var expected1 = "Cached content for 2";
            Assert.Equal(expected1, response1.Trim());

            // Act - 2
            await Task.Delay(TimeSpan.FromSeconds(1));
            var response2 = await client.GetStringAsync("/catalog/3");

            // Assert - 1
            var expected2 = "Cached content for 3";
            Assert.Equal(expected2, response2.Trim());
        }
    }
}