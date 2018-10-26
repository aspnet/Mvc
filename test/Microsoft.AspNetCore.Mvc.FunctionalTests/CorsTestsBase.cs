// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public abstract class CorsTestsBase<TStartup> : IClassFixture<MvcTestFixture<TStartup>> where TStartup : class
    {
        protected CorsTestsBase(MvcTestFixture<TStartup> fixture)
        {
            var factory = fixture.Factories.FirstOrDefault() ?? fixture.WithWebHostBuilder(ConfigureWebHostBuilder);
            Client = factory.CreateDefaultClient();
        }

        private static void ConfigureWebHostBuilder(IWebHostBuilder builder) =>
            builder.UseStartup<TStartup>();

        public HttpClient Client { get; }

        [Theory]
        [InlineData("GET")]
        [InlineData("HEAD")]
        [InlineData("POST")]
        public async Task ResourceWithSimpleRequestPolicy_Allows_SimpleRequests(string method)
        {
            // Arrange
            var origin = "http://example.com";
            var request = new HttpRequestMessage(new HttpMethod(method), "http://localhost/Cors/GetBlogComments");
            request.Headers.Add(CorsConstants.Origin, origin);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("[\"comment1\",\"comment2\",\"comment3\"]", content);
            var responseHeaders = response.Headers;
            var header = Assert.Single(response.Headers);
            Assert.Equal(CorsConstants.AccessControlAllowOrigin, header.Key);
            Assert.Equal(new[] { "*" }, header.Value.ToArray());
        }

        [Fact]
        public async Task OptionsRequest_NonPreflight_ExecutesOptionsAction()
        {
            // Arrange
            var request = new HttpRequestMessage(new HttpMethod("OPTIONS"), "http://localhost/NonCors/GetOptions");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("[\"Create\",\"Update\",\"Delete\"]", content);
            Assert.Empty(response.Headers);
        }

        [Fact]
        public async Task PreflightRequestOnNonCorsEnabledController_ExecutesOptionsAction()
        {
            // Arrange
            var request = new HttpRequestMessage(new HttpMethod("OPTIONS"), "http://localhost/NonCors/GetOptions");
            request.Headers.Add(CorsConstants.Origin, "http://example.com");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, "POST");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("[\"Create\",\"Update\",\"Delete\"]", content);
            Assert.Empty(response.Headers);
        }

        [Fact]
        public virtual async Task PreflightRequestOnNonCorsEnabledController_DoesNotMatchTheAction()
        {
            // Arrange
            var request = new HttpRequestMessage(new HttpMethod("OPTIONS"), "http://localhost/NonCors/Post");
            request.Headers.Add(CorsConstants.Origin, "http://example.com");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, "POST");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("HEAD")]
        [InlineData("POST")]
        [InlineData("PUT")]
        public async Task OriginMatched_ReturnsHeaders(string method)
        {
            // Arrange
            var request = new HttpRequestMessage(
                new HttpMethod(CorsConstants.PreflightHttpMethod),
                "http://localhost/Cors/GetBlogComments");

            // Adding a custom header makes it a non-simple request.
            request.Headers.Add(CorsConstants.Origin, "http://example.com");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, method);
            request.Headers.Add(CorsConstants.AccessControlRequestHeaders, "Custom");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            // MVC applied the policy and since that did not pass, there were no access control headers.
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Collection(
                response.Headers.OrderBy(h => h.Key),
                h =>
                {
                    Assert.Equal(CorsConstants.AccessControlAllowMethods, h.Key);
                    Assert.Equal(new[] { "GET,POST,HEAD" }, h.Value);
                },
                h =>
                {
                    Assert.Equal(CorsConstants.AccessControlAllowOrigin, h.Key);
                    Assert.Equal(new[] { "*" }, h.Value);
                });

            // It should short circuit and hence no result.
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal(string.Empty, content);
        }

        [Fact]
        public async Task SuccessfulCorsRequest_AllowsCredentials_IfThePolicyAllowsCredentials()
        {
            // Arrange
            var request = new HttpRequestMessage(
                HttpMethod.Put,
                "http://localhost/Cors/EditUserComment?userComment=abcd");

            // Adding a custom header makes it a non-simple request.
            request.Headers.Add(CorsConstants.Origin, "http://example.com");
            request.Headers.Add(CorsConstants.AccessControlExposeHeaders, "exposed1,exposed2");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseHeaders = response.Headers;
            Assert.Equal(
                new[] { "*" },
                responseHeaders.GetValues(CorsConstants.AccessControlAllowOrigin).ToArray());
            Assert.Equal(
               new[] { "true" },
               responseHeaders.GetValues(CorsConstants.AccessControlAllowCredentials).ToArray());
            Assert.Equal(
               new[] { "exposed1,exposed2" },
               responseHeaders.GetValues(CorsConstants.AccessControlExposeHeaders).ToArray());

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("abcd", content);
        }

        [Fact]
        public async Task SuccessfulPreflightRequest_AllowsCredentials_IfThePolicyAllowsCredentials()
        {
            // Arrange
            var request = new HttpRequestMessage(
                new HttpMethod(CorsConstants.PreflightHttpMethod),
                "http://localhost/Cors/EditUserComment?userComment=abcd");

            // Adding a custom header makes it a non-simple request.
            request.Headers.Add(CorsConstants.Origin, "http://example.com");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, "PUT");
            request.Headers.Add(CorsConstants.AccessControlRequestHeaders, "header1,header2");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseHeaders = response.Headers;
            Assert.Equal(
                new[] { "*" },
                responseHeaders.GetValues(CorsConstants.AccessControlAllowOrigin).ToArray());
            Assert.Equal(
               new[] { "true" },
               responseHeaders.GetValues(CorsConstants.AccessControlAllowCredentials).ToArray());
            Assert.Equal(
               new[] { "header1,header2" },
               responseHeaders.GetValues(CorsConstants.AccessControlAllowHeaders).ToArray());
            Assert.Equal(
               new[] { "PUT,POST" },
               responseHeaders.GetValues(CorsConstants.AccessControlAllowMethods).ToArray());

            var content = await response.Content.ReadAsStringAsync();
            Assert.Empty(content);
        }

        [Fact]
        public async Task PolicyFailed_Allows_ActualRequest_WithMissingResponseHeaders()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Put, "http://localhost/Cors/GetUserComments");

            // Adding a custom header makes it a non simple request.
            request.Headers.Add(CorsConstants.Origin, "http://example2.com");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            // MVC applied the policy and since that did not pass, there were no access control headers.
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(response.Headers);

            // It still have executed the action.
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("[\"usercomment1\",\"usercomment2\",\"usercomment3\"]", content);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("HEAD")]
        [InlineData("POST")]
        public async Task DisableCors_ActionsCanOverride_ControllerLevel(string method)
        {
            // Arrange
            var request = new HttpRequestMessage(new HttpMethod(method), "http://localhost/Cors/GetExclusiveContent");

            // Exclusive content is not available on other sites.
            request.Headers.Add(CorsConstants.Origin, "http://example.com");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Since there are no response headers, the client should step in to block the content.
            Assert.Empty(response.Headers);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("exclusive", content);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("HEAD")]
        [InlineData("POST")]
        public async Task DisableCors_PreFlight_ActionsCanOverride_ControllerLevel(string method)
        {
            // Arrange
            var request = new HttpRequestMessage(
                new HttpMethod(CorsConstants.PreflightHttpMethod),
                "http://localhost/Cors/GetExclusiveContent");

            // Exclusive content is not available on other sites.
            request.Headers.Add(CorsConstants.Origin, "http://example.com");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, method);
            request.Headers.Add(CorsConstants.AccessControlRequestHeaders, "Custom");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            // Since there are no response headers, the client should step in to block the content.
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(response.Headers);

            // Nothing gets executed for a pre-flight request.
            var content = await response.Content.ReadAsStringAsync();
            Assert.Empty(content);
        }

        [Fact]
        public async Task CorsFilter_RunsBeforeOtherAuthorizationFilters_UsesPolicySpecifiedOnController()
        {
            // Arrange
            var url = "http://localhost/api/store/actionusingcontrollercorssettings";
            var request = new HttpRequestMessage(new HttpMethod(CorsConstants.PreflightHttpMethod), url);

            // Adding a custom header makes it a non-simple request.
            request.Headers.Add(CorsConstants.Origin, "http://example.com");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, "GET");
            request.Headers.Add(CorsConstants.AccessControlRequestHeaders, "Custom");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseHeaders = response.Headers;
            Assert.Equal(
                new[] { "*" },
                responseHeaders.GetValues(CorsConstants.AccessControlAllowOrigin).ToArray());
            Assert.Equal(
               new[] { "true" },
               responseHeaders.GetValues(CorsConstants.AccessControlAllowCredentials).ToArray());
            Assert.Equal(
               new[] { "Custom" },
               responseHeaders.GetValues(CorsConstants.AccessControlAllowHeaders).ToArray());
            Assert.Equal(
               new[] { "GET" },
               responseHeaders.GetValues(CorsConstants.AccessControlAllowMethods).ToArray());

            var content = await response.Content.ReadAsStringAsync();
            Assert.Empty(content);
        }

        [Fact]
        public async Task CorsFilter_RunsBeforeOtherAuthorizationFilters_UsesPolicySpecifiedOnAction()
        {
            // Arrange
            var url = "http://localhost/api/store/actionwithcorssettings";
            var request = new HttpRequestMessage(new HttpMethod(CorsConstants.PreflightHttpMethod), url);

            // Adding a custom header makes it a non-simple request.
            request.Headers.Add(CorsConstants.Origin, "http://example.com");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, "GET");
            request.Headers.Add(CorsConstants.AccessControlRequestHeaders, "Custom");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseHeaders = response.Headers;
            Assert.Equal(
                new[] { "http://example.com" },
                responseHeaders.GetValues(CorsConstants.AccessControlAllowOrigin).ToArray());
            Assert.Equal(
               new[] { "true" },
               responseHeaders.GetValues(CorsConstants.AccessControlAllowCredentials).ToArray());
            Assert.Equal(
               new[] { "Custom" },
               responseHeaders.GetValues(CorsConstants.AccessControlAllowHeaders).ToArray());
            Assert.Equal(
               new[] { "GET" },
               responseHeaders.GetValues(CorsConstants.AccessControlAllowMethods).ToArray());

            var content = await response.Content.ReadAsStringAsync();
            Assert.Empty(content);
        }

        [Fact]
        public async Task DisableCorsFilter_RunsBeforeOtherAuthorizationFilters()
        {
            // Controller has an authorization filter and Cors filter and the action has a DisableCors filter
            // In this scenario, the CorsFilter should be executed before any other authorization filters
            // i.e irrespective of where the Cors filter is applied(controller or action), Cors filters must
            // always be executed before any other type of authorization filters.

            // Arrange
            var request = new HttpRequestMessage(
                new HttpMethod(CorsConstants.PreflightHttpMethod),
                "http://localhost/api/store/actionwithcorsdisabled");

            // Adding a custom header makes it a non-simple request.
            request.Headers.Add(CorsConstants.Origin, "http://example.com");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, "GET");
            request.Headers.Add(CorsConstants.AccessControlRequestHeaders, "Custom");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(response.Headers);

            // Nothing gets executed for a pre-flight request.
            var content = await response.Content.ReadAsStringAsync();
            Assert.Empty(content);
        }

        [Fact]
        public async Task CorsFilter_OnAction_PreferredOverController_AndAuthorizationFiltersRunAfterCors()
        {
            // Arrange
            var request = new HttpRequestMessage(
                new HttpMethod(CorsConstants.PreflightHttpMethod),
                "http://localhost/api/store/actionwithdifferentcorspolicy");
            request.Headers.Add(CorsConstants.Origin, "http://notexpecteddomain.com");
            request.Headers.Add(CorsConstants.AccessControlRequestMethod, "GET");
            request.Headers.Add(CorsConstants.AccessControlRequestHeaders, "Custom");

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(response.Headers);

            // Nothing gets executed for a pre-flight request.
            var content = await response.Content.ReadAsStringAsync();
            Assert.Empty(content);
        }
    }
}