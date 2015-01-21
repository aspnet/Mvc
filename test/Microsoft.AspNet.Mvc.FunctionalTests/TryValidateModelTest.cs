// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class TryValidateModelTest
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices(nameof(ValidationWebSite));
        private readonly Action<IApplicationBuilder> _app = new ValidationWebSite.Startup().Configure;

        [Fact]
        public async Task TryValidateModel_ProductViewModelInvalidProperties()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            var input = "{ \"Price\": 2, \"ProductDetails\": {\"Field1\": \"f1\"}}";
            var content = new StringContent(input, Encoding.UTF8, "application/json");
            var url =
                "http://localhost/ModelMetadataTypeValidation/TryValidateModelProductViewModelWithErrorInParameter/0";

            // Act
            var response = await client.PostAsync(url, content);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
            Assert.Equal(6, json.Count);
            Assert.Equal("CompanyName cannot be null", json["product.CompanyName"]);
            Assert.Equal("The field Price must be between 20 and 100.", json["product.Price"]);
            Assert.Equal("The Category field is required.", json["product.Category"]);
            Assert.Equal("The ContactUs field is required.", json["product.Contact"]);
            Assert.Equal("The Field2 field is required.", json["product.ProductDetails.Field2"]);
            Assert.Equal("The Field3 field is required.", json["product.ProductDetails.Field3"]);
        }

        [Fact]
        public async Task TryValidateModel_DerivedModelInvalidType()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            var url =
                "http://localhost/ModelMetadataTypeValidation/TryValidateModelSoftwareViewModelNoPrefix";

            // Act
            var response = await client.GetAsync(url);

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
            Assert.Equal(1, json.Count);
            Assert.Equal("Country property does not have the right value", json["software"]);
        }

        [Fact]
        public async Task TryValidateModel_ValidDerivedModel()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            var url =
                "http://localhost/ModelMetadataTypeValidation/TryValidateModelValidModelNoPrefix";

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Admin user created successfully", await response.Content.ReadAsStringAsync());
        }
    }
}