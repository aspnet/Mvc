// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using RazorExtensionWebSite;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class RazorExtensionTest
    {
        private readonly IServiceProvider _provider = TestHelper.CreateServices("RazorExtensionWebSite");
        private readonly Action<IBuilder> _app = new Startup().Configure;

        [Fact]
        public async Task ViewsWithDifferentExtensionAreProcessedByViewEngine()
        {
            // Arrange
            var server = TestServer.Create(_provider, _app);
            var client = server.Handler;
            var expectedMessage =
@"Layout says Hello world!";

            // Act
            var response = await client.GetAsync("http://localhost");

            // Assert
            var body = await response.ReadBodyAsStringAsync();
            Assert.Equal(expectedMessage, body.Trim());
        }
    }
}