// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class RazorViewEngineOptionsSetupTest
    {
        [Fact]
        public void RazorViewEngineOptionsSetup_SetsUpFileProvider()
        {
            // Arrange
            var options = new RazorViewEngineOptions();
            var expected = Mock.Of<IFileProvider>();
            var hostingEnv = new Mock<IHostingEnvironment>();
            hostingEnv.SetupGet(e => e.ContentRootFileProvider)
                .Returns(expected);

            var optionsSetup = GetSetup(hostingEnvironment: hostingEnv.Object);

            // Act
            optionsSetup.Configure(options);

            // Assert
            var fileProvider = Assert.Single(options.FileProviders);
            Assert.Same(expected, fileProvider);
        }

        private static RazorViewEngineOptionsSetup GetSetup(
            IHostingEnvironment hostingEnvironment = null)
        {
            hostingEnvironment = hostingEnvironment ?? Mock.Of<IHostingEnvironment>();

            return new RazorViewEngineOptionsSetup(hostingEnvironment);
        }
    }
}
