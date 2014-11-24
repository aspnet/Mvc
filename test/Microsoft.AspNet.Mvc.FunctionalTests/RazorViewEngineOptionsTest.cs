// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.TestHost;
using RazorViewEngineOptionsWebsite;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class RazorViewEngineOptionsTest
    {
        private readonly IServiceProvider _services = TestHelper.CreateServices("RazorViewEngineOptionsWebsite");
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;

        [Fact]
        public async Task RazorViewEngine_UsesFileSystemOnViewEngineOptionsToLocateViews()
        {
            // Arrange
            var expectedMessage = "The time is " + DateTime.UtcNow;
            var viewsDir = Path.Combine(Startup.ViewFileSystemRoot, "Views", "RazorViewEngineOptions_Home");
            Directory.CreateDirectory(viewsDir);
            File.WriteAllText(Path.Combine(viewsDir, "Index.cshtml"), expectedMessage);

            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();

            try
            {
                // Act
                var response = await client.GetStringAsync("http://localhost/RazorViewEngineOptions_Home");

                // Assert
                Assert.Equal(expectedMessage, response);
            }
            finally
            {
                TryDeleteDirectory(Startup.ViewFileSystemRoot);
            }
        }

        [Fact]
        public async Task RazorViewEngine_UsesFileSystemOnViewEngineOptionsToLocateAreaViews()
        {
            // Arrange
            var expectedMessage = "The time is " + DateTime.UtcNow;
            var viewsDir = Path.Combine(Startup.ViewFileSystemRoot, 
                                        "Areas", 
                                        "Restricted", 
                                        "Views", 
                                        "RazorViewEngineOptions_Admin");
            Directory.CreateDirectory(viewsDir);
            File.WriteAllText(Path.Combine(viewsDir, "Login.cshtml"), expectedMessage);

            var server = TestServer.Create(_services, _app);
            var client = server.CreateClient();
            var target = "http://localhost/Restricted/RazorViewEngineOptions_Admin/Login";

            try
            {
                // Act
                var response = await client.GetStringAsync(target);

                // Assert
                Assert.Equal(expectedMessage, response);
            }
            finally
            {
                TryDeleteDirectory(Startup.ViewFileSystemRoot);
            }
        }

        private static void TryDeleteDirectory(string viewsDir)
        {
            try
            {
                Directory.Delete(viewsDir, recursive: true);
            }
            catch
            {
                // Ignore failures
            }
        }
    }
}