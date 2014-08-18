// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ConnegWebsite;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class OutputFormatterTests
    {
        private readonly IServiceProvider _provider = TestHelper.CreateServices("ConnegWebsite");
        private readonly Action<IBuilder> _app = new Startup().Configure;

        [Theory]
        [InlineData("ReturnTaskOfString")]
        [InlineData("ReturnTaskOfObject_StringValue")]
        [InlineData("ReturnString")]
        [InlineData("ReturnObject_StringValue")]
        public async Task TextPlainFormatter_ForStringValues_GetsSelectedReturnsTextPlainContentType(string actionName)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.Handler;
            var expectedContentType = "text/plain;charset=utf-8";
            var expectedBody = actionName;

            // Act
            var result = await client.GetAsync("http://localhost/TextPlain/" + actionName);

            // Assert
            Assert.Equal(expectedContentType, result.HttpContext.Response.ContentType);
            var body = await result.HttpContext.Response.ReadBodyAsStringAsync();
            Assert.Equal(expectedBody, body);
        }

        [Theory]
        [InlineData("ReturnTaskOfObject_ObjectValue")]
        [InlineData("ReturnObject_ObjectValue")]
        public async Task JsonOutputFormatter_ForNonStringValue_GetsSelected(string actionName)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.Handler;
            var expectedContentType = "application/json;charset=utf-8";
            var expectedBody = actionName;

            // Act
            var result = await client.GetAsync("http://localhost/TextPlain/" + actionName);

            // Assert
            Assert.Equal(expectedContentType, result.HttpContext.Response.ContentType);
            var body = await result.HttpContext.Response.ReadBodyAsStringAsync();
        }

        [Theory]
        [InlineData("ReturnTaskOfString_NullValue")]
        [InlineData("ReturnTaskOfObject_NullValue")]    
        [InlineData("ReturnObject_NullValue")]
        public async Task NoContentFormatter_ForNullValue_GetsSelectedAndWritesResponse(string actionName)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.Handler;
            string expectedContentType = null;

            // ReadBodyAsString returns empty string instead of null.
            string expectedBody = "";

            // Act
            var result = await client.GetAsync("http://localhost/NoContent/" + actionName);

            // Assert
            Assert.Equal(expectedContentType, result.HttpContext.Response.ContentType);
            var body = await result.HttpContext.Response.ReadBodyAsStringAsync();
            Assert.Equal(expectedBody, body);
            Assert.Equal(204, result.HttpContext.Response.StatusCode);
            Assert.Equal(0, result.HttpContext.Response.ContentLength);
        }

        [Fact]
        public async Task XmlDataContractSerializerOutputFormatterIsCalled()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.Handler;
            var headers = new Dictionary<string, string[]>();
            headers.Add("Accept", new string[] { "application/xml;charset=utf-8" });

            // Act
            var response = await client.SendAsync("POST",
                "http://localhost/Home/GetDummyClass?sampleInput=10", headers, null, null);

            //Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("<DummyClass xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xmlns=\"http://schemas.datacontract.org/2004/07/ConnegWebsite\">" +
                "<SampleInt>10</SampleInt></DummyClass>",
                new StreamReader(response.Body, Encoding.UTF8).ReadToEnd());
        }

        [Fact]
        public async Task XmlSerializerOutputFormatterIsCalled()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.Handler;
            var headers = new Dictionary<string, string[]>();
            headers.Add("Accept", new string[] { "application/xml;charset=utf-8" });

            // Act
            var response = await client.SendAsync("POST",
                "http://localhost/XmlSerializer/GetDummyClass?sampleInput=10", headers, null, null);

            //Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("<DummyClass xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><SampleInt>10</SampleInt></DummyClass>",
                new StreamReader(response.Body, Encoding.UTF8).ReadToEnd());
        }
    }
}