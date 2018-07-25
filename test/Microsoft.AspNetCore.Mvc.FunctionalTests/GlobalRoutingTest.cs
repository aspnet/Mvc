// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class GlobalRoutingTest : RoutingTestsBase<RoutingWebSite.StartupWithGlobalRouting>
    {
        public GlobalRoutingTest(MvcTestFixture<RoutingWebSite.StartupWithGlobalRouting> fixture)
            : base(fixture)
        {
        }

        [Fact]
        public async override Task HasEndpointMatch()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/Routing/HasEndpointMatch");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<bool>(body);

            Assert.True(result);
        }

        [Fact(Skip = "Link generation issue in global routing. Need to fix - https://github.com/aspnet/Routing/issues/590")]
        public override Task AttributeRoutedAction_InArea_StaysInArea_ActionDoesntExist()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "Link generation issue in global routing. Need to fix - https://github.com/aspnet/Routing/issues/590")]
        public override Task ConventionalRoutedAction_InArea_StaysInArea()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async override Task RouteData_Routers_ConventionalRoute()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/RouteData/Conventional");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultData>(body);

            Assert.Equal(
                Array.Empty<string>(),
                result.Routers);
        }

        [Fact]
        public async override Task RouteData_Routers_AttributeRoute()
        {
            // Arrange & Act
            var response = await Client.GetAsync("http://localhost/RouteData/Attribute");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultData>(body);

            Assert.Equal(
                Array.Empty<string>(),
                result.Routers);
        }
    }
}