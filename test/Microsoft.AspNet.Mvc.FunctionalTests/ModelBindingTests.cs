// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class ModelBindingTests
    {
        private readonly IServiceProvider _services;
        private readonly Action<IBuilder> _app = new ModelBindingWebSite.Startup().Configure;

        public ModelBindingTests()
        {
            _services = TestHelper.CreateServices("ModelBindingWebSite");
        }

        [Fact]
        public async Task CheckIfByteArrayModelBinderWorks()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.Handler;
            var input = "";

            // Act
            var response = await client.PostAsync("http://localhost/Home/Index?byteValues=Fys1", input, "application/json");

            //Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("23,43,53", await response.ReadBodyAsStringAsync());
        }
    }
}