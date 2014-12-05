// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;
using Newtonsoft.Json;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class ModelBindingFromHeaderTest
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices(nameof(ModelBindingWebSite));
        private readonly Action<IApplicationBuilder> _app = new ModelBindingWebSite.Startup().Configure;

        // The action that this test hits will echo back the model-bound value
        [Theory]
        [InlineData("transactionId", "1e331f25-0869-4c87-8a94-64e6e40cb5a0")]
        [InlineData("TransaCtionId", "1e331f25-0869-4c87-8a94-64e6e40cb5a0")] // Case-Insensitive
        [InlineData("TransaCtionId", "1e331f25-0869-4c87-8a94-64e6e40cb5a0,abcd")] // Binding to string doesn't split values
        public async Task FromHeader_BindHeader_ToString_OnParameter(string headerName, string headerValue)
        {
            // Arrange
            var expected = headerValue;

            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Blog/BindToStringParameter");
            request.Headers.TryAddWithoutValidation(headerName, headerValue);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(body);
            Assert.Equal(expected, result.HeaderValue);
        }

        // The action that this test hits will echo back the model-bound value
        [Theory]
        [InlineData("transactionId1234", "1e331f25-0869-4c87-8a94-64e6e40cb5a0")]
        public async Task FromHeader_BindHeader_ToString_OnParameter_NoValues(string headerName, string headerValue)
        {
            // Arrange
            var expected = headerValue;

            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Blog/BindToStringParameter?q=123");
            request.Headers.TryAddWithoutValidation(headerName, headerValue);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(body);
            Assert.Null(result.HeaderValue);
        }

        // The action that this test hits will echo back the model-bound values
        [Theory]
        [InlineData("transactionIds", "1e331f25-0869-4c87-8a94-64e6e40cb5a0")]
        [InlineData("transactionIds", "1e331f25-0869-4c87-8a94-64e6e40cb5a0,abcd,efg")]
        public async Task FromHeader_BindHeader_ToStringArray_OnParameter(string headerName, string headerValue)
        {
            // Arrange
            var expected = headerValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/Blog/BindToStringArrayParameter");
            request.Headers.TryAddWithoutValidation(headerName, headerValue);

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(body);
            Assert.Equal<string>(expected, result.HeaderValues);
        }

        private class Result
        {
            public string HeaderValue { get; set; }

            public string[] HeaderValues { get; set; }

            public string[] ModelStateErrors { get; set; }
        }
    }
}