// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SecurityWebSite;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class AuthorizeTests : IClassFixture<MvcTestFixture<Startup>>
    {
        public AuthorizeTests(MvcTestFixture<Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Fact]
        public async Task AutomaticAuthenticationBeforeAntiforgery()
        {
            // Arrange & Act
            var response = await Client.PostAsync("http://localhost/Home/AutoAntiforgery", null);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Home/Login", response.Headers.Location.AbsolutePath, StringComparer.OrdinalIgnoreCase);
        }
    }
}
