// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.Logging;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class JsonOutputFormatterTests
    {
        private HttpClient GetClient(TestSink sink)
        {
            var factory = new TestLoggerFactory(sink, true);
            var services = TestHelper.CreateServices("FormatterWebSite", factory);
            var app = (Action<IApplicationBuilder>)new FormatterWebSite.Startup(factory).Configure;
            var server = TestServer.Create(services, app);
            return server.CreateClient();
        }

        [Fact]
        public async Task JsonOutputFormatter_ReturnsIndentedJson_LogsCorrectValues()
        {
            // Arrange
            var user = new FormatterWebSite.User()
            {
                Id = 1,
                Alias = "john",
                description = "Administrator",
                Designation = "Administrator",
                Name = "John Williams"
            };

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Formatting = Formatting.Indented;
            var expectedBody = JsonConvert.SerializeObject(user, serializerSettings);

            var sink = new TestSink();
            var client = GetClient(sink);

            // Act
            var response = await client.GetAsync("http://localhost/JsonFormatter/ReturnsIndentedJson");

            // Assert
            var actualBody = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedBody, actualBody);
            var logs = sink.Writes.Where(w => string.Equals(w.LoggerName, "Microsoft.AspNet.Mvc.ObjectResult"));
            Assert.Single(logs);
            Assert.Equal(typeof(JsonOutputFormatter), ((ObjectResultValues)logs.First().State).SelectedFormatter.OutputFormatterType);
        }
    }
}