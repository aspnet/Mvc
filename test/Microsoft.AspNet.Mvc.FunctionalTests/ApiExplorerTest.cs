﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;
using Newtonsoft.Json;
using System.Net.Http;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class ApiExplorerTest
    {
        private readonly IServiceProvider _provider = TestHelper.CreateServices("ApiExplorerWebSite");
        private readonly Action<IApplicationBuilder> _app = new ApiExplorer.Startup().Configure;

        [Fact]
        public async Task ApiExplorer_IsVisible_EnabledWithConvention()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApiExplorerVisbilityEnabledByConvention");

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task ApiExplorer_IsVisible_DisabledWithConvention()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApiExplorerVisbilityDisabledByConvention");

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ApiExplorer_IsVisible_DisabledWithAttribute()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApiExplorerVisibilitySetExplicitly/Disabled");

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ApiExplorer_IsVisible_EnabledWithAttribute()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApiExplorerVisibilitySetExplicitly/Enabled");

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task ApiExplorer_GroupName_SetByConvention()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApiExplorerNameSetByConvention");

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);
            Assert.Equal(description.GroupName, "ApiExplorerNameSetByConvention");
        }

        [Fact]
        public async Task ApiExplorer_GroupName_SetByAttributeOnController()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApiExplorerNameSetExplicitly/SetOnController");

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);
            Assert.Equal(description.GroupName, "SetOnController");
        }

        [Fact]
        public async Task ApiExplorer_GroupName_SetByAttributeOnAction()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApiExplorerNameSetExplicitly/SetOnAction");

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);
            Assert.Equal(description.GroupName, "SetOnAction");
        }

        [Fact]
        public async Task ApiExplorer_HttpMethod_All()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApiExplorerHttpMethod/All");

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);
            Assert.Null(description.HttpMethod);
        }

        [Fact]
        public async Task ApiExplorer_HttpMethod_Single()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApiExplorerHttpMethod/Get");

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);
            Assert.Equal("GET", description.HttpMethod);
        }

        // This is hitting one action with two allowed methods (using [AcceptVerbs]). This should
        // return two api descriptions.
        [Theory]
        [InlineData("PUT")]
        [InlineData("POST")]
        public async Task ApiExplorer_HttpMethod_Single(string httpMethod)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            var request = new HttpRequestMessage(
                new HttpMethod(httpMethod),
                "http://localhost/ApiExplorerHttpMethod/Single");

            // Act
            var response = await client.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Single(result, d => d.HttpMethod == "PUT");
            Assert.Single(result, d => d.HttpMethod == "POST");
        }

        [Theory]
        [InlineData("GetVoid")]
        [InlineData("GetObject")]
        [InlineData("GetIActionResult")]
        [InlineData("GetDerivedActionResult")]
        [InlineData("GetTask")]
        [InlineData("GetTaskOfObject")]
        [InlineData("GetTaskOfIActionResult")]
        [InlineData("GetTaskOfDerivedActionResult")]
        public async Task ApiExplorer_ResponseType_UnknownWithoutAttribute(string action)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync(
                "http://localhost/ApiExplorerResponseTypeWithoutAttribute/" + action);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);
            Assert.Null(description.ResponseType);
        }

        [Theory]
        [InlineData("GetProduct", "ApiExplorer.Product")]
        [InlineData("GetInt", "System.Int32")]
        [InlineData("GetTaskOfProduct", "ApiExplorer.Product")]
        [InlineData("GetTaskOfInt", "System.Int32")]
        public async Task ApiExplorer_ResponseType_KnownWithoutAttribute(string action, string type)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync(
                "http://localhost/ApiExplorerResponseTypeWithoutAttribute/" + action);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);
            Assert.Equal(type, description.ResponseType);
        }

        [Theory]
        [InlineData("GetVoid", "ApiExplorer.Customer")]
        [InlineData("GetObject", "ApiExplorer.Product")]
        [InlineData("GetIActionResult", "System.String")]
        [InlineData("GetProduct", "ApiExplorer.Customer")]
        public async Task ApiExplorer_ResponseType_KnownWithAttribute(string action, string type)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync(
                "http://localhost/ApiExplorerResponseTypeWithAttribute/" + action);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);
            Assert.Equal(type, description.ResponseType);
        }

        [Theory]
        [InlineData("Controller", "ApiExplorer.Product")]
        [InlineData("Action", "ApiExplorer.Customer")]
        public async Task ApiExplorer_ResponseType_OverrideOnAction(string action, string type)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync(
                "http://localhost/ApiExplorerResponseTypeOverrideOnAction/" + action);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);
            Assert.Equal(type, description.ResponseType);
        }

        [Fact]
        public async Task ApiExplorer_ResponseContentType_Unset()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApiExplorerResponseContentType/Unset");

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);

            var formats = description.SupportedResponseFormats;
            Assert.Equal(4, formats.Count);

            var textXml = Assert.Single(formats, f => f.MediaType == "text/xml");
            var applicationXml = Assert.Single(formats, f => f.MediaType == "application/xml");
            var textJson = Assert.Single(formats, f => f.MediaType == "text/json");
            var applicationJson = Assert.Single(formats, f => f.MediaType == "application/json");
        }

        // uses [Produces("*/*")]
        [Fact]
        public async Task ApiExplorer_ResponseContentType_AllTypes()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApiExplorerResponseContentType/AllTypes");

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);

            var formats = description.SupportedResponseFormats;
            Assert.Equal(4, formats.Count);

            var textXml = Assert.Single(formats, f => f.MediaType == "text/xml");
            var applicationXml = Assert.Single(formats, f => f.MediaType == "application/xml");
            var textJson = Assert.Single(formats, f => f.MediaType == "text/json");
            var applicationJson = Assert.Single(formats, f => f.MediaType == "application/json");
        }

        [Fact]
        public async Task ApiExplorer_ResponseContentType_Range()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApiExplorerResponseContentType/Range");

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);

            var formats = description.SupportedResponseFormats;
            Assert.Equal(2, formats.Count);

            var textXml = Assert.Single(formats, f => f.MediaType == "text/xml");
            var textJson = Assert.Single(formats, f => f.MediaType == "text/json");
        }

        [Fact]
        public async Task ApiExplorer_ResponseContentType_Specific()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApiExplorerResponseContentType/Specific");

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);

            var formats = description.SupportedResponseFormats;
            Assert.Equal(1, formats.Count);

            var textJson = Assert.Single(formats, f => f.MediaType == "application/json");
        }

        [Fact]
        public async Task ApiExplorer_ResponseContentType_NoMatch()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApiExplorerResponseContentType/NoMatch");

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);

            var formats = description.SupportedResponseFormats;
            Assert.Empty(formats);
        }

        [Theory]
        [InlineData("Controller", "text/xml", "Microsoft.AspNet.Mvc.XmlDataContractSerializerOutputFormatter")]
        [InlineData("Action", "application/json", "Microsoft.AspNet.Mvc.JsonOutputFormatter")]
        public async Task ApiExplorer_ResponseContentType_OverrideOnAction(
            string action,
            string contentType,
            string formatterType)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync(
                "http://localhost/ApiExplorerResponseContentTypeOverrideOnAction/" + action);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<ApiExplorerData>>(body);

            // Assert
            var description = Assert.Single(result);

            var format = Assert.Single(description.SupportedResponseFormats);
            Assert.Equal(contentType, format.MediaType);
            Assert.Equal(formatterType, format.FormatterType);
        }

        // Used to serialize data between client and server
        private class ApiExplorerData
        {
            public string GroupName { get; set; }

            public string HttpMethod { get; set; }

            public List<ApiExplorerParameterData> ParameterDescriptions { get; } = new List<ApiExplorerParameterData>();

            public string RelativePath { get; set; }

            public string ResponseType { get; set; }

            public List<ApiExplorerResponseData> SupportedResponseFormats { get; } = new List<ApiExplorerResponseData>();
        }

        // Used to serialize data between client and server
        private class ApiExplorerParameterData
        {
            public bool IsOptional { get; set; }

            public string Name { get; set; }

            public string Source { get; set; }

            public string Type { get; set; }
        }

        // Used to serialize data between client and server
        private class ApiExplorerResponseData
        {
            public string MediaType { get; set; }

            public string FormatterType { get; set; }
        }
    }
}