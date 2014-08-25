// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;
using Xunit;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class MvcSampleTests
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices("MvcSample.Web", true);
        private readonly Action<IBuilder> _app = new MvcSample.Web.Startup().Configure;

        public MvcSampleTests()
        {
#if NET45
            // APPBASE is modified so that AddJsonFile in MVC sample looks in the correct location.
            // The MvcSample uses this to determine the base path. If we do not set this, the base path will
            // always point to the FunctionalTests folder - the current context,
            // while it is required to point to the WebSite's root directory.
            var originalProvider = CallContextServiceLocator.Locator.ServiceProvider;
            var appEnvironment = originalProvider.GetService<IApplicationEnvironment>();

            // APPBASE is global to a process. So if any other test starts using APPBASE, it might break.
            // So this is temporary. Once https://github.com/aspnet/Configuration/issues/109 is fixed,
            // this code should be modified to use AppEnvironment. - aspnet/Mvc#1067
            AppDomain.CurrentDomain.SetData("APPBASE", TestHelper.CalculateApplicationBasePath(appEnvironment, "MvcSample.Web", true));
#endif
        }

        [Fact]
        public async Task Home_Index_ReturnsSuccess()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.Handler;

            // Act
            var response = await client.GetAsync("http://localhost/Home/Index");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public async Task Home_NotFoundAction_Returns404()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.Handler;

            // Act
            var response = await client.GetAsync("http://localhost/Home/NotFound");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(404, response.StatusCode);
        }

        [Fact]
        public async Task Home_CreateUser_ShouldReturnXmlBasedOnAcceptHeader_ButOverridenByProducesAttribute()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.Handler;
            var headers = new Dictionary<string, string[]>();
            headers.Add("Accept", new string[] { "application/xml;charset=utf-8" });

            // Act
            var response = await client.SendAsync("GET", "http://localhost/Home/ReturnUser", headers, null, null);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("{\"Name\":\"My name\",\"Address\":\"My address\",\"Age\":13,\"GPA\":13.37," +
                "\"Dependent\":{\"Name\":\"Dependents name\",\"Address\":\"Dependents address\",\"Age\":0," +
                "\"GPA\":0.0,\"Dependent\":null,\"Alive\":false,\"Password\":null,\"Profession\":null,\"About" +
                "\":null,\"Log\":null,\"OwnedAddresses\":[],\"ParentsAges\":[]},\"Alive\":true,\"Password\":" +
                "\"Secure string\",\"Profession\":\"Software Engineer\",\"About\":\"I like playing Football\",\"" +
                "Log\":null,\"OwnedAddresses\":[],\"ParentsAges\":[]}",
                new StreamReader(response.Body, Encoding.UTF8).ReadToEnd());
        }

        [Theory]
        [InlineData("http://localhost/Filters/ChallengeUser", 401)]
        [InlineData("http://localhost/Filters/AllGranted", 401)]
        [InlineData("http://localhost/Filters/NotGrantedClaim", 401)]
        public async Task FiltersController_Tests(string url, int statusCode)
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.Handler;

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(statusCode, response.StatusCode);
        }

        [Fact]
        public async Task FiltersController_Crash_ThrowsException()
        {
            // Arrange
            var server = TestServer.Create(_services, _app);
            var client = server.Handler;

            // Act
            var response = await client.GetAsync("http://localhost/Filters/Crash?message=HelloWorld");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("Boom HelloWorld", new StreamReader(response.Body, Encoding.UTF8).ReadToEnd());
        }
    }
}