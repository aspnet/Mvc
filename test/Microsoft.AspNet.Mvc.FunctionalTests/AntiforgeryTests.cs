// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class AntiforgeryTests
    {
        private const string SiteName = nameof(AntiforgeryTokenWebSite);
        private readonly Action<IApplicationBuilder> _app = new AntiforgeryTokenWebSite.Startup().Configure;
        private readonly Action<IServiceCollection> _configureServices = new AntiforgeryTokenWebSite.Startup().ConfigureServices;

        [Fact]
        public async Task MultipleAFTokensWithinTheSamePage_GeneratesASingleCookieToken()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Account/Login");

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var header = Assert.Single(response.Headers.GetValues("X-Frame-Options"));
            Assert.Equal("SAMEORIGIN", header);

            var setCookieHeader = response.Headers.GetValues("Set-Cookie").ToArray();

            // Even though there are two forms there should only be one response cookie,
            // as for the second form, the cookie from the first token should be reused.
            Assert.Single(setCookieHeader);
        }

        [Fact]
        public async Task MultipleFormPostWithingASingleView_AreAllowed()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            // do a get response.
            var getResponse = await client.GetAsync("http://localhost/Account/Login");
            var responseBody = await getResponse.Content.ReadAsStringAsync();

            // Get the AF token for the second login. If the cookies are generated twice(i.e are different),
            // this AF token will not work with the first cookie.
            var formToken = AntiforgeryTestHelper.RetrieveAntiforgeryToken(responseBody, "Account/UseFacebookLogin");
            var cookieToken = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getResponse);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Account/Login");
            request.Headers.Add("Cookie", cookieToken.Key + "=" + cookieToken.Value);
            var nameValueCollection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", formToken),
                new KeyValuePair<string,string>("UserName", "abra"),
                new KeyValuePair<string,string>("Password", "cadabra"),
            };

            request.Content = new FormUrlEncodedContent(nameValueCollection);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("OK", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task InvalidCookieToken_Throws()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            var getResponse = await client.GetAsync("http://localhost/Account/Login");
            var responseBody = await getResponse.Content.ReadAsStringAsync();
            var formToken = AntiforgeryTestHelper.RetrieveAntiforgeryToken(responseBody, "Account/Login");

            var cookieToken = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getResponse);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Account/Login");
            request.Headers.Add("Cookie", cookieToken.Key + "=invalidCookie");

            var nameValueCollection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", formToken),
                new KeyValuePair<string,string>("UserName", "abra"),
                new KeyValuePair<string,string>("Password", "cadabra"),
            };

            request.Content = new FormUrlEncodedContent(nameValueCollection);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var exception = response.GetServerException();
            Assert.Equal("The antiforgery token could not be decrypted.", exception.ExceptionMessage);
        }

        [Fact]
        public async Task InvalidFormToken_Throws()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            var getResponse = await client.GetAsync("http://localhost/Account/Login");
            var responseBody = await getResponse.Content.ReadAsStringAsync();
            var cookieToken = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getResponse);
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Account/Login");
            var formToken = "adsad";
            request.Headers.Add("Cookie", cookieToken.Key + "=" + cookieToken.Value);
            var nameValueCollection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", formToken),
                new KeyValuePair<string,string>("UserName", "abra"),
                new KeyValuePair<string,string>("Password", "cadabra"),
            };

            request.Content = new FormUrlEncodedContent(nameValueCollection);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var exception = response.GetServerException();
            Assert.Equal("The antiforgery token could not be decrypted.", exception.ExceptionMessage);
        }

        [Fact]
        public async Task IncompatibleCookieToken_Throws()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            // do a get response.
            // We do two requests to get two different sets of antiforgery cookie and token values.
            var getResponse1 = await client.GetAsync("http://localhost/Account/Login");
            var responseBody1 = await getResponse1.Content.ReadAsStringAsync();
            var formToken1 = AntiforgeryTestHelper.RetrieveAntiforgeryToken(responseBody1, "Account/Login");

            var getResponse2 = await client.GetAsync("http://localhost/Account/Login");
            var responseBody2 = await getResponse2.Content.ReadAsStringAsync();
            var cookieToken2 = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getResponse2);

            var cookieToken = cookieToken2.Value;
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Account/Login");
            request.Headers.Add("Cookie", string.Format("{0}={1}", cookieToken2.Key, cookieToken2.Value));
            var formToken = formToken1;
            var nameValueCollection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", formToken),
                new KeyValuePair<string,string>("UserName", "abra"),
                new KeyValuePair<string,string>("Password", "cadabra"),
            };

            request.Content = new FormUrlEncodedContent(nameValueCollection);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var exception = response.GetServerException();
            Assert.Equal("The antiforgery cookie token and form field token do not match.", exception.ExceptionMessage);
        }

        [Fact]
        public async Task MissingCookieToken_Throws()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            // do a get response.
            var getResponse = await client.GetAsync("http://localhost/Account/Login");
            var responseBody = await getResponse.Content.ReadAsStringAsync();
            var formToken = AntiforgeryTestHelper.RetrieveAntiforgeryToken(responseBody, "Account/Login");
            var cookieTokenKey = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getResponse).Key;

            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Account/Login");
            var nameValueCollection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", formToken),
                new KeyValuePair<string,string>("UserName", "abra"),
                new KeyValuePair<string,string>("Password", "cadabra"),
            };

            request.Content = new FormUrlEncodedContent(nameValueCollection);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var exception = response.GetServerException();
            Assert.Equal(
                "The required antiforgery cookie \"" + cookieTokenKey + "\" is not present.",
                exception.ExceptionMessage);
        }

        [Fact]
        public async Task MissingAFToken_Throws()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var getResponse = await client.GetAsync("http://localhost/Account/Login");
            var responseBody = await getResponse.Content.ReadAsStringAsync();
            var cookieToken = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getResponse);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Account/Login");
            request.Headers.Add("Cookie", cookieToken.Key + "=" + cookieToken.Value);
            var nameValueCollection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string,string>("UserName", "abra"),
                new KeyValuePair<string,string>("Password", "cadabra"),
            };

            request.Content = new FormUrlEncodedContent(nameValueCollection);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            var exception = response.GetServerException();
            Assert.Equal("The required antiforgery form field \"__RequestVerificationToken\" is not present.",
                         exception.ExceptionMessage);
        }

        [Fact]
        public async Task SetCookieAndHeaderBeforeFlushAsync_GeneratesCookieTokenAndHeader()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Account/FlushAsyncLogin");

            // Assert
            var header = Assert.Single(response.Headers.GetValues("X-Frame-Options"));
            Assert.Equal("SAMEORIGIN", header);

            var setCookieHeader = response.Headers.GetValues("Set-Cookie").ToArray();
            Assert.Single(setCookieHeader);
        }

        [Fact]
        public async Task SetCookieAndHeaderBeforeFlushAsync_PostToForm()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            // do a get response.
            var getResponse = await client.GetAsync("http://localhost/Account/FlushAsyncLogin");
            var responseBody = await getResponse.Content.ReadAsStringAsync();

            var formToken = AntiforgeryTestHelper.RetrieveAntiforgeryToken(responseBody, "Account/FlushAsyncLogin");
            var cookieToken = AntiforgeryTestHelper.RetrieveAntiforgeryCookie(getResponse);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Account/FlushAsyncLogin");
            request.Headers.Add("Cookie", cookieToken.Key + "=" + cookieToken.Value);
            var nameValueCollection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string,string>("__RequestVerificationToken", formToken),
                new KeyValuePair<string,string>("UserName", "test"),
                new KeyValuePair<string,string>("Password", "password"),
            };

            request.Content = new FormUrlEncodedContent(nameValueCollection);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("OK", await response.Content.ReadAsStringAsync());
        }
    }
}