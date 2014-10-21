// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using ModelBindingWebSite;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class RoundTripTests
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices("ModelBindingWebSite");
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;

        [Fact]
        public async Task RoundTrippedValues_GetsModelBound_ForSimpleExpressions()
        {
            // Arrange
            var expected = "test-name";
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var expression = await client.GetStringAsync("http://localhost/RoundTrip/GetPerson");
            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>(expression, expected)
            };
            var result = await GetPerson(client, keyValuePairs);

            // Assert
            Assert.Equal("Name", expression);
            Assert.Equal(expected, result.Name);
        }

        [Fact]
        public async Task RoundTrippedValues_GetsModelBound_ForSubPropertyExpressions()
        {
            // Arrange
            var expected = 40;
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var expression = await client.GetStringAsync("http://localhost/RoundTrip/GetPersonParentAge");
            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>(expression, expected.ToString())
            };
            var result = await GetPerson(client, keyValuePairs);

            // Assert
            Assert.Equal("Parent.Age", expression);
            Assert.Equal(expected, result.Parent.Age);
        }

        [Fact]
        public async Task RoundTrippedValues_GetsModelBound_ForNumericIndexedProperties()
        {
            // Arrange
            var expected = 12;
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var expression = await client.GetStringAsync("http://localhost/RoundTrip/GetPersonDependentAge");
            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>(expression, expected.ToString())
            };
            var result = await GetPerson(client, keyValuePairs);

            // Assert
            Assert.Equal("Dependents[0].Age", expression);
            Assert.Equal(expected, result.Dependents[0].Age);
        }

        [Fact(Skip = "Requires resolution for https://github.com/aspnet/Mvc/issues/1418")]
        public async Task RoundTrippedValues_GetsModelBound_ForStringIndexedProperties()
        {
            // Arrange
            var expected = "33";
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var expression = await client.GetStringAsync("http://localhost/RoundTrip/GetPersonParentHeightAttribute");
            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>(expression, expected.ToString())
            };
            var result = await GetPerson(client, keyValuePairs);

            // Assert
            Assert.Equal("Parent.Attributes[height]", expression);
            Assert.Equal(expected, result.Parent.Attributes["height"]);
        }

        [Fact]
        public async Task RoundTrippedValues_GetsModelBound_ForNestedNumericIndexedProperties()
        {
            // Arrange
            var expected = "test-nested-name";
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var expression = await client.GetStringAsync("http://localhost/RoundTrip/GetDependentPersonName");
            var keyValuePairs = new[]
            {
                new KeyValuePair<string, string>(expression, expected.ToString())
            };
            var result = await GetPerson(client, keyValuePairs);

            // Assert
            Assert.Equal("Dependents[0].Dependents[0].Name", expression);
            Assert.Equal(expected, result.Dependents[0].Dependents[0].Name);
        }

        private static async Task<Person> GetPerson(HttpClient client, KeyValuePair<string, string>[] keyValuePairs)
        {
            var content = new FormUrlEncodedContent(keyValuePairs);
            var response = await client.PostAsync("http://localhost/RoundTrip/Person", content);
            var result = JsonConvert.DeserializeObject<Person>(await response.Content.ReadAsStringAsync());
            return result;
        }
    }
}