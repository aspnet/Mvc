// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BasicWebSite.Models;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class ConsumesAttributeTests : IClassFixture<MvcTestFixture<BasicWebSite.Startup>>
    {
        public ConsumesAttributeTests(MvcTestFixture<BasicWebSite.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Fact]
        public async Task NoRequestContentType_SelectsActionWithoutConstraint()
        {
            // Arrange
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "http://localhost/ConsumesAttribute_Company/CreateProduct");

            // Act
            var response = await Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("CreateProduct_Product_Text", body);
        }

        [Fact]
        public async Task NoRequestContentType_Selects_IfASingleActionWithConstraintIsPresent()
        {
            // Arrange
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "http://localhost/ConsumesAttribute_PassThrough/CreateProduct");

            // Act
            var response = await Client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("ConsumesAttribute_PassThrough_Product_Json", body);
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("text/json")]
        public async Task Selects_Action_BasedOnRequestContentType(string requestContentType)
        {
            // Arrange
            var input = "{SampleString:\""+requestContentType+"\"}";
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "http://localhost/ConsumesAttribute_AmbiguousActions/CreateProduct");
            request.Content = new StringContent(input, Encoding.UTF8, requestContentType);

            // Act
            var response = await Client.SendAsync(request);
            var product = JsonConvert.DeserializeObject<Product>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(requestContentType, product.SampleString);
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("text/json")]
        public async Task ActionLevelAttribute_OveridesClassLevel(string requestContentType)
        {
            // Arrange
            var input = "{SampleString:\"" + requestContentType + "\"}";
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "http://localhost/ConsumesAttribute_OverridesBase/CreateProduct");
            request.Content = new StringContent(input, Encoding.UTF8, requestContentType);
            var expectedString = "ConsumesAttribute_OverridesBaseController_" + requestContentType;

            // Act
            var response = await Client.SendAsync(request);
            var product = JsonConvert.DeserializeObject<Product>(await response.Content.ReadAsStringAsync());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedString, product.SampleString);
        }

        [Fact]
        public async Task DerivedClassLevelAttribute_OveridesBaseClassLevel()
        {
            // Arrange
            var input = "<Product xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                "xmlns=\"http://schemas.datacontract.org/2004/07/BasicWebSite.Models\">" +
                "<SampleString>application/xml</SampleString></Product>";
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "http://localhost/ConsumesAttribute_Overrides/CreateProduct");
            request.Content = new StringContent(input, Encoding.UTF8, "application/xml");
            var expectedString = "ConsumesAttribute_OverridesController_application/xml";

            // Act
            var response = await Client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var product = JsonConvert.DeserializeObject<Product>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedString, product.SampleString);
        }
    }
}