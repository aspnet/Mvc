// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using RazorWebSite;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class ViewStartTest
    {
        private readonly IServiceProvider _provider = TestHelper.CreateServices("RazorWebSite");
        private readonly Action<IBuilder> _app = new Startup().Configure;

        [Fact]
        public async Task ViewStartsAreExecuted_AndSetLayoutPropertyOnViewsBeingExecuted()
        {
            var expected =
@"<layout>
View-with-viewstart
</layout>";

            var server = TestServer.Create(_provider, _app);

            var client = server.Handler;

            // Act
            var result = await client.GetAsync("http://localhost/ViewStart");

            // Assert
            var body = await result.HttpContext.Response.ReadBodyAsStringAsync();
            Assert.Equal(expected, body.Trim());
        }
    }
}