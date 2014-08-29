﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using BasicWebSite;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class BasicTests
    {
        private readonly IServiceProvider _provider = TestHelper.CreateServices("BasicWebSite");
        private readonly Action<IBuilder> _app = new Startup().Configure;

        // Some tests require comparing the actual response body against an expected response baseline
        // so they require a reference to the assembly on which the resources are located, in order to
        // make the tests less verbose, we get a reference to the assembly with the resources and we
        // use it on all the rest of the tests.
        private readonly Assembly _resourcesAssembly = typeof(BasicTests).GetTypeInfo().Assembly;

        [InlineData("http://localhost/")]
        [InlineData("http://localhost/Home")]
        [InlineData("http://localhost/Home/Index")]
        [InlineData("http://localhost/Users")]
        [InlineData("http://localhost/Monitor/CountActionDescriptorInvocations")]
        public async Task CanRender_ViewsWithLayout(string url)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            var expectedMediaType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");

            // The K runtime compiles every file under compiler/resources as a resource at runtime with the same name
            // as the file name, in order to update a baseline you just need to change the file in that folder.
            var expectedContent = await _resourcesAssembly.ReadResourceAsStringAsync("compiler/resources/BasicWebSite.Home.Index.html");

            // Act

            // The host is not important as everything runs in memory and tests are isolated from each other.
            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedMediaType, response.Content.Headers.ContentType);
            Assert.Equal(expectedContent, responseContent);
        }

        [Fact]
        public async Task CanRender_SimpleViews()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            var expectedContent = await _resourcesAssembly.ReadResourceAsStringAsync("compiler/resources/BasicWebSite.Home.PlainView.html");
            var expectedMediaType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");


            // Act
            var response = await client.GetAsync("http://localhost/Home/PlainView");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedMediaType, response.Content.Headers.ContentType);
            Assert.Equal(expectedContent, responseContent);
        }

        [Fact]
        public async Task CanReturn_ResultsWithoutContent()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Home/NoContentResult");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Null(response.Content.Headers.ContentType);
            Assert.Equal(0, response.Content.Headers.ContentLength);
            Assert.Equal(0, responseContent.Length);
        }

        [Fact]
        public async Task ReturningTaskFromAction_ProducesNoContentResult()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Home/ActionReturningTask");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello world", body);
        }

        [Fact]
        public async Task ActionDescriptors_CreatedOncePerRequest()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            var expectedContent = "1";

            // Act and Assert
            for (var i = 0; i < 3; i++)
            {
                var result = await client.GetAsync("http://localhost/Monitor/CountActionDescriptorInvocations");
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                var responseContent = await result.Content.ReadAsStringAsync();

                Assert.Equal(expectedContent, responseContent);
            }
        }
    }
}