﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
    /// <summary>
    /// Functional tests that verify if names returned by HtmlHelper.NameFor get model bound correctly.
    /// Each test works in three steps -
    /// 1) The result of an HtmlHelper.NameFor invocation for a specific expression is retrieved.
    /// 2) A form URL encoded value is posted for the retrieved expression.
    /// 3) The server returns the bounded object. We verify if the property specified by the expression in step 1
    /// has the right value.
    /// </summary>
    public class RoundTripTests
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices("ModelBindingWebSite");
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;

        // Uses the expression p => p.Name
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

        // Uses the expression p => p.Parent.Age
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

        // Uses the expression p => p.Dependents[0].Age
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

        // Uses the expression p => p.Parent.Attributes["height"]
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

        // Uses the expression p => p.Dependents[0].Dependents[0].Name
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