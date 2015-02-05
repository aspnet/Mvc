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
    public class ApplicationModelTest
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices(nameof(ApplicationModelWebSite));
        private readonly Action<IApplicationBuilder> _app = new ApplicationModelWebSite.Startup().Configure;

        [Fact]
        public async Task ControllerModel_CustomizedWithAttribute()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/CoolController/GetControllerName");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("CoolController", body);
        }

        [Fact]
        public async Task ActionModel_CustomizedWithAttribute()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ActionModel/ActionName");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("ActionName", body);
        }

        [Fact]
        public async Task ParameterModel_CustomizedWithAttribute()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ParameterModel/GetParameterMetadata");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("CoolMetadata", body);
        }

        [Fact]
        public async Task ApplicationModel_AddPropertyToActionDescriptor_FromApplicationModel()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/Home/GetCommonDescription");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("Common Application Description", body);
        }

        [Fact]
        public async Task ApplicationModel_AddPropertyToActionDescriptor_ControllerModelOverwritesCommonApplicationProperty()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApplicationModel/GetControllerDescription");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("Common Controller Description", body);
        }

        [Fact]
        public async Task ApplicationModel_ProvidesMetadataToActionDescriptor_ActionModelOverwritesControllerModelProperty()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            // Act
            var response = await client.GetAsync("http://localhost/ApplicationModel/GetActionSpecificDescription");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("Specific Action Description", body);
       }
    }
}