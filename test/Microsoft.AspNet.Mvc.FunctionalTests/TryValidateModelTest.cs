﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class TryValidateModelTest
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices("FormatterWebSite");
        private readonly Action<IApplicationBuilder> _app = new VersioningWebSite.Startup().Configure;

        [Fact]
        public async Task TryValidateModel_SimpleModelInvalidProperties()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/TryValidateModel/GetInvalidUser");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("The field Id must be between 1 and 2000.," +
                "The field Name must be a string or array type with a minimum length of '5'.",
                await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TryValidateModel_DerivedModelInvalidType()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/TryValidateModel/GetInvalidAdminWithPrefix");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("AdminAccessCode property does not have the right value",
                await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TryValidateModel_ValidDerivedModel()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/TryValidateModel/GetValidAdminWithPrefix");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Admin user created successfully", await response.Content.ReadAsStringAsync());
        }
    }
}