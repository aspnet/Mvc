﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class RazorPagesViewSearchTest : IClassFixture<MvcTestFixture<RazorPagesWebSite.Startup>>
    {
        public RazorPagesViewSearchTest(MvcTestFixture<RazorPagesWebSite.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Fact]
        public async Task Page_CanFindPartial_InCurrentDirectory()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/Pages/ViewSearch?partial=_Sibling");

            // Assert
            Assert.Equal("Hello from sibling", content.Trim());
        }
        
        [Fact]
        public async Task Page_CanFindPartial_InParentDirectory()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/Pages/ViewSearch?partial=_Parent");

            // Assert
            Assert.Equal("Hello from parent", content.Trim());
        }
        
        [Fact]
        public async Task Page_CanFindPartial_InRootDirectory()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/Pages/ViewSearch?partial=_Root");

            // Assert
            Assert.Equal("Hello from root", content.Trim());
        }

        [Fact]
        public async Task Page_CanFindPartial_InViewsSharedDirectory()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/Pages/ViewSearch?partial=_Shared");

            // Assert
            Assert.Equal("Hello from shared", content.Trim());
        }
    }
}
