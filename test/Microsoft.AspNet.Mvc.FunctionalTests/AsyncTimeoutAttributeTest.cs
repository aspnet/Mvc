// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BasicWebSite;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class AsyncTimeoutAttributeTest
    {
        private readonly IServiceProvider _provider = TestHelper.CreateServices(nameof(BasicWebSite));
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;

        [Theory]
        [InlineData("http://localhost/AsyncTimeout/ActionWithTimeoutAttribute")]
        [InlineData("http://localhost/AsyncTimeoutOnController/ActionWithTimeoutAttribute")]
        public async Task AsyncTimeOutAttribute_IsDecorated_AndCancellationTokenIsBound(string url)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            var expected = "CancellationToken is present";

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var data = await response.Content.ReadAsStringAsync();
            Assert.Equal(expected, data);
        }

        [Fact]
        public async Task AsyncTimeOutAttribute_IsNotDecorated_AndCancellationToken_IsNone()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            var expected = "CancellationToken is not present";

            // Act
            var response = await client.GetAsync("http://localhost/AsyncTimeout/ActionWithNoTimeoutAttribute");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var data = await response.Content.ReadAsStringAsync();
            Assert.Equal(expected, data);
        }

        [Theory]
        [InlineData("http://localhost/AsyncTimeout")]
        [InlineData("http://localhost/AsyncTimeoutOnController")]
        public async Task TimeoutIsTriggered(string baseUrl)
        {
            // Arrange
            var expected = "Hello World!";
            var expectedCorrelationId = Guid.NewGuid().ToString();
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("CorrelationId", expectedCorrelationId);

            // Act
            var response = await client.GetAsync(string.Format("{0}/LongRunningAction", baseUrl));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var data = await response.Content.ReadAsStringAsync();
            Assert.Equal(expected, data);

            response = await client.GetAsync(string.Format("{0}/TimeoutTriggerLogs", baseUrl));
            data = await response.Content.ReadAsStringAsync();
            var timeoutTriggerLogs = JsonConvert.DeserializeObject<List<string>>(data);
            Assert.NotNull(timeoutTriggerLogs);
            Assert.Contains(expectedCorrelationId, timeoutTriggerLogs);
        }
    }
}