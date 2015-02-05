﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class RespectBrowserAcceptHeaderTests
    {
        private readonly IServiceProvider _provider = TestHelper.CreateServices(nameof(FormatterWebSite));
        private readonly Action<IApplicationBuilder> _app = new FormatterWebSite.Startup().Configure;

        [Theory]
        [InlineData("application/xml,*/*;0.2")]
        [InlineData("application/xml,*/*")]
        public async Task AllMediaRangeAcceptHeader_FirstFormatterInListWritesResponse(string acceptHeader)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", acceptHeader);
            
            // Act
            var response = await client.GetAsync("http://localhost/RespectBrowserAcceptHeader/EmployeeInfo");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
            var responseData = await response.Content.ReadAsStringAsync();
            Assert.Equal("{\"Id\":10,\"Name\":\"John\"}", responseData);
        }

        [Theory]
        [InlineData("application/xml,*/*;0.2")]
        [InlineData("application/xml,*/*")]
        public async Task AllMediaRangeAcceptHeader_ProducesAttributeIsHonored(string acceptHeader)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", acceptHeader);
            var expectedResponseData = "<RespectBrowserAcceptHeaderController.Employee xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                                       " xmlns=\"http://schemas.datacontract.org/2004/07/FormatterWebSite.Controllers\"><Id>20</Id><Name>Mike" +
                                       "</Name></RespectBrowserAcceptHeaderController.Employee>";

            // Act
            var response = await client.GetAsync("http://localhost/RespectBrowserAcceptHeader/EmployeeInfoWithProduces");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("application/xml; charset=utf-8", response.Content.Headers.ContentType.ToString());
            var responseData = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponseData, responseData);
        }

        [Theory]
        [InlineData("application/xml,*/*;0.2")]
        [InlineData("application/xml,*/*")]
        public async Task AllMediaRangeAcceptHeader_WithContentTypeHeader_ContentTypeIsHonored(string acceptHeader)
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", acceptHeader);
            var requestData = "<RespectBrowserAcceptHeaderController.Employee xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                              " xmlns=\"http://schemas.datacontract.org/2004/07/FormatterWebSite.Controllers\"><Id>35</Id><Name>Jimmy" +
                              "</Name></RespectBrowserAcceptHeaderController.Employee>";
            
            // Act
            var response = await client.PostAsync("http://localhost/RespectBrowserAcceptHeader/CreateEmployee",
                                                    new StringContent(requestData, Encoding.UTF8, "application/xml"));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal("application/xml; charset=utf-8", response.Content.Headers.ContentType.ToString());
            var responseData = await response.Content.ReadAsStringAsync();
            Assert.Equal(requestData, responseData);
        }
    }
}