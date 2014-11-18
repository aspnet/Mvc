// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Threading.Tasks;
using InlineConstraints;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;
using Newtonsoft.Json;
using Microsoft.AspNet.Routing;
using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class InlineConstraintTests
    {
        private readonly IServiceProvider _provider = TestHelper.CreateServices("InlineConstraintsWebSite");
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;

        [Fact]
        public async Task RoutingToANonExistantArea_WithExistConstraint_RoutesToCorrectAction()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/area-exists/Users");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var returnValue = await response.Content.ReadAsStringAsync();
            Assert.Equal("Users.Index", returnValue);
        }

        [Fact]
        public async Task RoutingToANonExistantArea_WithoutExistConstraint_RoutesToIncorrectAction()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetAsync("http://localhost/area-withoutexists/Users"));

            Assert.Equal("The view 'Index' was not found." +
                         " The following locations were searched:\r\n/Areas/Users/Views/Home/Index.cshtml\r\n" +
                         "/Areas/Users/Views/Shared/Index.cshtml\r\n/Views/Shared/Index.cshtml.",
                         ex.Message);
        }

        [Fact]
        public async Task InlineConstraint_GetProductById()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductById/5");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await GetResponseValues(response);
            Assert.Equal(result["id"], "5");
            Assert.Equal(result["controller"], "Products");
            Assert.Equal(result["action"], "GetProductById");
        }

        [Fact]
        public async Task InlineConstraint_GetProductById_NoId()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductById");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await GetResponseValues(response);
            Assert.Equal(result["controller"], "Products");
            Assert.Equal(result["action"], "GetProductById");
        }

        [Fact]
        public async Task InlineConstraint_GetProductById_NotIntId()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductById/asdf");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task InlineConstraint_GetProductByName_Valid()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductByName/asdf");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await GetResponseValues(response);
            Assert.Equal(result["name"], "asdf");
            Assert.Equal(result["controller"], "Products");
            Assert.Equal(result["action"], "GetProductByName");
        }

        [Fact]
        public async Task InlineConstraint_GetProductByName_Invalid()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductByName/asd123");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task InlineConstraint_GetProductByName_NoName()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductByName");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task InlineConstraint_GetProductByManufacturingDate()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync(@"http://localhost/products/GetProductByManufacturingDate/2014-10-11T13:45:30");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await GetResponseValues(response);
            Assert.Equal(result["dateTime"], new DateTime(2014, 10, 11, 13, 45, 30));
            Assert.Equal(result["controller"], "Products");
            Assert.Equal(result["action"], "GetProductByManufacturingDate");
        }

        [Fact]
        public async Task InlineConstraint_GetProductByCategoryName_ValidCatName()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            
            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductByCategoryName/Sports");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await GetResponseValues(response);
            Assert.Equal(result["name"], "Sports");
            Assert.Equal(result["controller"], "Products");
            Assert.Equal(result["action"], "GetProductByCategoryName");
        }

        [Fact]
        public async Task InlineConstraint_GetProductByCategoryName_InvalidCatName()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductByCategoryName/SportsSportsSportsSports");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task InlineConstraint_GetProductByCategoryName_NoCatName()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductByCategoryName");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await GetResponseValues(response);
            Assert.Equal(result["controller"], "Products");
            Assert.Equal(result["action"], "GetProductByCategoryName");
        }

        [Fact]
        public async Task InlineConstraint_GetProductByCategoryId_ValidId()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            
            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductByCategoryId/40");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await GetResponseValues(response);
            Assert.Equal(result["catId"], "40");
            Assert.Equal(result["controller"], "Products");
            Assert.Equal(result["action"], "GetProductByCategoryId");
        }

        [Fact]
        public async Task InlineConstraint_GetProductByCategoryId_InvalidId()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductByCategoryId/5");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task InlineConstraint_GetProductByCategoryId_NotIntId()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductByCategoryId/asdf");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task InlineConstraint_GetProductByPrice_Valid()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductByPrice/4023.23423");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await GetResponseValues(response);
            Assert.Equal(result["price"], "4023.23423");
            Assert.Equal(result["controller"], "Products");
            Assert.Equal(result["action"], "GetProductByPrice");
        }

        [Fact]
        public async Task InlineConstraint_GetProductByPrice_NoPrice()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductByPrice");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await GetResponseValues(response);
            Assert.Equal(result["controller"], "Products");
            Assert.Equal(result["action"], "GetProductByPrice");
        }

        [Fact]
        public async Task InlineConstraint_GetProductByManufacturarId_Valid()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductByManufacturarId/57");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await GetResponseValues(response);
            Assert.Equal(result["manId"], "57");
            Assert.Equal(result["controller"], "Products");
            Assert.Equal(result["action"], "GetProductByManufacturarId");
        }

        [Fact]
        public async Task InlineConstraint_GetProductByManufacturarId_NoId()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();
            
            // Act
            var response = await client.GetAsync("http://localhost/products/GetProductByManufacturarId");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await GetResponseValues(response);
            Assert.Equal(result["controller"], "Products");
            Assert.Equal(result["action"], "GetProductByManufacturarId");
        }

        [Fact]
        public async Task InlineConstraint_GetStoreById_Valid()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Store/GetStoreById/691cf17a-791b-4af8-99fd-e739e168170f");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await GetResponseValues(response);
            Assert.Equal(result["id"], "691cf17a-791b-4af8-99fd-e739e168170f");
            Assert.Equal(result["controller"], "Store");
            Assert.Equal(result["action"], "GetStoreById");
        }

        [Fact]
        public async Task InlineConstraint_GetStoreById_NoId()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Store/GetStoreById");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await GetResponseValues(response);
            Assert.Equal(result["controller"], "Store");
            Assert.Equal(result["action"], "GetStoreById");
        }

        [Fact]
        public async Task InlineConstraint_GetStoreById_NotGuidId()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Store/GetStoreById/691cf17a-791b");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task InlineConstraint_GetStoreByLocation_Valid()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Store/GetStoreByLocation/Bellevue");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await GetResponseValues(response);
            Assert.Equal(result["location"], "Bellevue");
            Assert.Equal(result["controller"], "Store");
            Assert.Equal(result["action"], "GetStoreByLocation");
        }

        [Fact]
        public async Task InlineConstraint_GetStoreByLocation_MoreLength()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Store/GetStoreByLocation/BellevueRedmond");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task InlineConstraint_GetStoreByLocation_LessLength()
        {
            var server = TestServer.Create(_provider, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Store/GetStoreByLocation/Be");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<IDictionary<string, object>> GetResponseValues(HttpResponseMessage response)
        {
            var returnValue = await response.Content.ReadAsStringAsync();
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<IDictionary<string, object>>(body);
            return result;
        }
    }
}
