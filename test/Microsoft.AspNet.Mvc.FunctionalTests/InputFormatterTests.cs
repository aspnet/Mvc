﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class InputFormatterTests
    {
        private readonly IServiceProvider _services;
        private readonly Action<IBuilder> _app = new FormatterWebSite.Startup().Configure;

        public InputFormatterTests()
        {
            _services = TestHelper.CreateServices("FormatterWebSite");
        }

        [Fact]
        public async Task CheckIfXmlInputFormatterIsBeingCalled()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.Handler;
            var sampleInputInt = 10;
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<DummyClass><SampleInt>" + sampleInputInt.ToString() + "</SampleInt></DummyClass>";

            // Act
            var response = await client.PostAsync("http://localhost/Home/Index", input, "application/xml");

            //Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(sampleInputInt.ToString(), await response.ReadBodyAsStringAsync());
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("application/*")]
        [InlineData("*/*")]
        [InlineData("text/json")]
        [InlineData("text/*")]
        public async Task JsonInputFormatter_IsSelectedForJsonRequest(string requestContentType)
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.Handler;
            var sampleInputInt = 10;
            var input = "{\"SampleInt\":10}";

            // Act
            var response = await client.PostAsync("http://localhost/Home/Index", input, requestContentType);

            //Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(sampleInputInt.ToString(), await response.ReadBodyAsStringAsync());
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("invalid")]
        public async Task JsonInputFormatter_IsNotSelectedForNonJsonRequests(string requestContentType)
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.Handler;
            var input = "{\"SampleInt\":10}";

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>
                            (() => client.PostAsync("http://localhost/Home/CheckIfDummyIsNull", input, requestContentType));

            //Assert
            // TODO: Change the validation after https://github.com/aspnet/Mvc/issues/458 is fixed.
            Assert.Equal("415: Unsupported content type " + requestContentType, ex.Message);
        }

        // TODO: By default XmlSerializerInputFormatter is called because of the order in which
        // the formatters are registered. Add a test to call into DataContractSerializerInputFormatter.
    }
}