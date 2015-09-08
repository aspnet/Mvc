// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Testing;
using Microsoft.Framework.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class ModelMetadataAttributeTest
    {
        private const string SiteName = nameof(ValidationWebSite);
        private readonly Action<IApplicationBuilder> _app = new ValidationWebSite.Startup().Configure;
        private readonly Action<IServiceCollection> _configureServices = new ValidationWebSite.Startup().ConfigureServices;

        [Fact]
        public async Task ModelMetaDataTypeAttribute_ValidBaseClass_EmptyResponseBody()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var input = "{ \"Name\": \"MVC\", \"Contact\":\"4258959019\", \"Category\":\"Technology\"," +
                "\"CompanyName\":\"Microsoft\", \"Country\":\"USA\",\"Price\": 21, \"ProductDetails\": {\"Detail1\": \"d1\"," +
                " \"Detail2\": \"d2\", \"Detail3\": \"d3\"}}";
            var content = new StringContent(input, Encoding.UTF8, "application/json");

            var url = "http://localhost/ModelMetadataTypeValidation/ValidateProductViewModelIncludingMetadata";

            // Act
            var response = await client.PostAsync(url, content);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("{}", body);
        }

        [Fact]
        public async Task ModelMetaDataTypeAttribute_InvalidPropertiesAndSubPropertiesOnBaseClass_ReturnsErrors()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var input = "{ \"Price\": 2, \"ProductDetails\": {\"Detail1\": \"d1\"}}";
            var content = new StringContent(input, Encoding.UTF8, "application/json");

            var url = "http://localhost/ModelMetadataTypeValidation/ValidateProductViewModelIncludingMetadata";

            // Act
            var response = await client.PostAsync(url, content);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
            Assert.Equal(6, json.Count);
            Assert.Equal("CompanyName cannot be null or empty.", json["CompanyName"]);
            Assert.Equal("The field Price must be between 20 and 100.", json["Price"]);
            // Mono issue - https://github.com/aspnet/External/issues/19
            Assert.Equal(PlatformNormalizer.NormalizeContent("The Category field is required."), json["Category"]);
            Assert.Equal(PlatformNormalizer.NormalizeContent("The Contact Us field is required."), json["Contact"]);
            Assert.Equal(
                PlatformNormalizer.NormalizeContent("The Detail2 field is required."),
                json["ProductDetails.Detail2"]);
            Assert.Equal(
                PlatformNormalizer.NormalizeContent("The Detail3 field is required."),
                json["ProductDetails.Detail3"]);
        }

        [Fact]
        public async Task ModelMetaDataTypeAttribute_InvalidComplexTypePropertyOnBaseClass_ReturnsErrors()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var input = "{ \"Contact\":\"4255678765\", \"Category\":\"Technology\"," +
                "\"CompanyName\":\"Microsoft\", \"Country\":\"USA\",\"Price\": 21 }";
            var content = new StringContent(input, Encoding.UTF8, "application/json");

            var url = "http://localhost/ModelMetadataTypeValidation/ValidateProductViewModelIncludingMetadata";

            // Act
            var response = await client.PostAsync(url, content);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
            Assert.Equal(1, json.Count);
            // Mono issue - https://github.com/aspnet/External/issues/19
            Assert.Equal(
                PlatformNormalizer.NormalizeContent("The ProductDetails field is required."),
                json["ProductDetails"]);
        }

        [Fact]
        public async Task ModelMetaDataTypeAttribute_InvalidClassAttributeOnBaseClass_ReturnsErrors()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var input = "{ \"Contact\":\"4258959019\", \"Category\":\"Technology\"," +
                "\"CompanyName\":\"Microsoft\", \"Country\":\"UK\",\"Price\": 21, \"ProductDetails\": {\"Detail1\": \"d1\"," +
                " \"Detail2\": \"d2\", \"Detail3\": \"d3\"}}";

            var content = new StringContent(input, Encoding.UTF8, "application/json");

            var url = "http://localhost/ModelMetadataTypeValidation/ValidateProductViewModelIncludingMetadata";

            // Act
            var response = await client.PostAsync(url, content);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
            Assert.Equal(1, json.Count);
            Assert.Equal("Product must be made in the USA if it is not named.", json[""]);
        }

        [Fact]
        public async Task ModelMetaDataTypeAttribute_ValidDerivedClass_EmptyResponseBody()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var input = "{ \"Name\": \"MVC\", \"Contact\":\"4258959019\", \"Category\":\"Technology\"," +
                "\"CompanyName\":\"Microsoft\", \"Country\":\"USA\", \"Version\":\"2\"," +
                "\"DatePurchased\": \"/Date(1297246301973)/\", \"Price\" : \"110\" }";
            var content = new StringContent(input, Encoding.UTF8, "application/json");

            var url = "http://localhost/ModelMetadataTypeValidation/ValidateSoftwareViewModelIncludingMetadata";

            // Act
            var response = await client.PostAsync(url, content);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("{}", body);
        }

        [Fact]
        public async Task ModelMetaDataTypeAttribute_InvalidPropertiesOnDerivedClass_ReturnsErrors()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var input = "{ \"Name\": \"MVC\", \"Contact\":\"425-895-9019\", \"Category\":\"Technology\"," +
                "\"CompanyName\":\"Microsoft\", \"Country\":\"USA\",\"Price\": 2}";
            var content = new StringContent(input, Encoding.UTF8, "application/json");

            var url = "http://localhost/ModelMetadataTypeValidation/ValidateSoftwareViewModelIncludingMetadata";

            // Act
            var response = await client.PostAsync(url, content);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
            Assert.Equal(2, json.Count);
            Assert.Equal("The field Price must be between 100 and 200.", json["Price"]);
            Assert.Equal("The field Contact must be a string with a maximum length of 10.", json["Contact"]);
        }

        [Fact]
        public async Task ModelMetaDataTypeAttribute_InvalidClassAttributeOnBaseClassProduct_ReturnsErrors()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var input = "{ \"Contact\":\"4258959019\", \"Category\":\"Technology\"," +
                "\"CompanyName\":\"Microsoft\", \"Country\":\"UK\",\"Version\":\"2\"," +
                "\"DatePurchased\": \"/Date(1297246301973)/\", \"Price\" : \"110\" }";
            var content = new StringContent(input, Encoding.UTF8, "application/json");

            var url = "http://localhost/ModelMetadataTypeValidation/ValidateSoftwareViewModelIncludingMetadata";

            // Act
            var response = await client.PostAsync(url, content);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
            Assert.Equal(1, json.Count);
            Assert.Equal("Product must be made in the USA if it is not named.", json[""]);
        }
    }
}