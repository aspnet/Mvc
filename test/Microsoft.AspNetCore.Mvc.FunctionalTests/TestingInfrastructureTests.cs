﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using BasicWebSite;
using BasicWebSite.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using RazorPagesClassLibrary;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class TestingInfrastructureTests : IClassFixture<WebApplicationFactory<BasicWebSite.Startup>>
    {
        public TestingInfrastructureTests(WebApplicationFactory<BasicWebSite.Startup> fixture)
        {
            Factory = fixture.Factories.FirstOrDefault() ?? fixture.WithWebHostBuilder(ConfigureWebHostBuilder);
            Client = Factory.CreateClient();
        }

        private static void ConfigureWebHostBuilder(IWebHostBuilder builder) =>
            builder.ConfigureTestServices(s => s.AddSingleton<TestService, OverridenService>());

        public WebApplicationFactory<Startup> Factory { get; }
        public HttpClient Client { get; }

        [Fact]
        public async Task TestingInfrastructure_CanOverrideServiceFromWithinTheTest()
        {
            // Act
            var response = await Client.GetStringAsync("Testing/Builder");

            // Assert
            Assert.Equal("Test", response);
        }

        [Fact]
        public void TestingInfrastructure_CreateClientThrowsInvalidOperationForNonEntryPoint()
        {
            var factory = new WebApplicationFactory<ClassLibraryStartup>();
            var ex = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());
            Assert.Equal($"The provided Type '{typeof(RazorPagesClassLibrary.ClassLibraryStartup).Name}' does not belong to an assembly with an entry point. A common cause for this error is providing a Type from a class library.",
               ex.Message);
        }

        [Fact]
        public async Task TestingInfrastructure_RedirectHandlerWorksWithPreserveMethod()
        {
            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "Testing/RedirectHandler/2")
            {
                Content = new ObjectContent<Number>(new Number { Value = 5 }, new JsonMediaTypeFormatter())
            };
            request.Headers.Add("X-Pass-Thru", "Some-Value");
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var xPassThruValue = Assert.Single(response.Headers.GetValues("X-Pass-Thru"));
            Assert.Equal("Some-Value", xPassThruValue);

            var handlerResponse = await response.Content.ReadAsAsync<RedirectHandlerResponse>();
            Assert.Equal(5, handlerResponse.Url);
            Assert.Equal(5, handlerResponse.Body);
        }

        [Fact]
        public async Task TestingInfrastructure_RedirectHandlerUsesOriginalRequestHeaders()
        {
            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "Testing/RedirectHandler/Headers");
            var client = Factory.CreateDefaultClient(
                new RedirectHandler(), new TestHandler());
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var modifiedHeaderWasSent = await response.Content.ReadAsStringAsync();

            Assert.Equal("false", modifiedHeaderWasSent);
        }

        [Fact]
        public async Task TestingInfrastructure_PostRedirectGetWorksWithCookies()
        {
            // Act
            var acquireToken = await Client.GetAsync("Testing/AntiforgerySimulator/3");
            Assert.Equal(HttpStatusCode.OK, acquireToken.StatusCode);

            var response = await Client.PostAsync(
                "Testing/PostRedirectGet/Post/3",
                content: null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var handlerResponse = await response.Content.ReadAsAsync<PostRedirectGetGetResponse>();
            Assert.Equal(4, handlerResponse.TempDataValue);
            Assert.Equal("Value-4", handlerResponse.CookieValue);
        }

        [Fact]
        public async Task TestingInfrastructure_PutWithoutBodyFollowsRedirects()
        {
            // Act
            var response = await Client.PutAsync("Testing/Put/3", content: null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(5, await response.Content.ReadAsAsync<int>());
        }

        private class OverridenService : TestService
        {
            public OverridenService()
            {
                Message = "Test";
            }
        }

        private class TestHandler : DelegatingHandler
        {
            public TestHandler()
            {
            }

            public TestHandler(HttpMessageHandler innerHandler) : base(innerHandler)
            {
            }

            public bool HeaderAdded { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (!HeaderAdded)
                {
                    request.Headers.Add("X-Added-Header", "true");
                    HeaderAdded = true;
                }

                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}
