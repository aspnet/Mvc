// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class RazorPagesNamespaceTest : IClassFixture<MvcTestFixture<RazorPagesWebSite.Startup>>
    {
        public RazorPagesNamespaceTest(MvcTestFixture<RazorPagesWebSite.Startup> fixture)
        {
            fixture.WebHostBuilder?.UseStartup<RazorPagesWebSite.Startup>();
            Client = fixture.CreatePlainClient();
        }

        public HttpClient Client { get; }

        [Fact]
        public async Task Page_DefaultNamespace_IfUnset()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/DefaultNamespace");

            // Assert
            Assert.Equal("AspNetCore", content.Trim());
        }
        
        [Fact]
        public async Task Page_ImportedNamespace_UsedFromViewImports()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/Pages/Namespace/Nested/Folder");

            // Assert
            Assert.Equal("CustomNamespace.Nested.Folder", content.Trim());
        }
        
        [Fact]
        public async Task Page_OverrideNamespace_SetByPage()
        {
            // Arrange & Act
            var content = await Client.GetStringAsync("http://localhost/Pages/Namespace/Nested/Override");

            // Assert
            Assert.Equal("Override", content.Trim());
        }
    }
}
