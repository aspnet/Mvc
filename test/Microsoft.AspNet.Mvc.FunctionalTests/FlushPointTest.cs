// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using RazorWebSite;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class FlushPointTest
    {
        private readonly IServiceProvider _provider = TestHelper.CreateServices("RazorWebSite");
        private readonly Action<IBuilder> _app = new Startup().Configure;

        [Fact]
        public async Task FlushPointsAreExecutedForPagesWithLayouts()
        {
            var expected =
@"<title>Page With Layout</title>


RenderBody content


    <span>Content that takes time to produce</span>";
            var server = TestServer.Create(_provider, _app);
            var client = server.Handler;

            // Act
            var result = await client.GetAsync("http://localhost/FlushPoint/PageWithLayout");

            // Assert
            var body = await result.HttpContext.Response.ReadBodyAsStringAsync();
            Assert.Equal(expected, body.Trim());
        }

        [Fact]
        public async Task FlushPointsAreExecutedForPagesWithoutLayouts()
        {
            var expected =
@"Initial content

Final content";
            var server = TestServer.Create(_provider, _app);
            var client = server.Handler;

            // Act
            var result = await client.GetAsync("http://localhost/FlushPoint/PageWithoutLayout");

            // Assert
            var body = await result.HttpContext.Response.ReadBodyAsStringAsync();
            Assert.Equal(expected, body.Trim());
        }
    }
}