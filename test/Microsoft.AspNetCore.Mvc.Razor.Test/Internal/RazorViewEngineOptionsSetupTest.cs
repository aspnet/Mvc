// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
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

            var optionsSetup = GetSetup(hostingEnv.Object);

            // Act
            optionsSetup.Configure(options);

            // Assert
            var fileProvider = Assert.Single(options.FileProviders);
            Assert.Same(expected, fileProvider);
        }

        [Fact]
        public void PostConfigure_SetsAllowRecompilingViewsOnFileChange_For21()
        {
            // Arrange
            var options = new RazorViewEngineOptions();
            var compatibilityVersion = new MvcCompatibilityOptions { CompatibilityVersion = CompatibilityVersion.Version_2_1 };
            var optionsSetup = GetSetup(compatibilityOptions: compatibilityVersion);

            // Act
            optionsSetup.Configure(options);
            optionsSetup.PostConfigure(string.Empty, options);

            // Assert
            Assert.True(options.AllowRecompilingViewsOnFileChange);
        }

        [Theory]
        [InlineData(CompatibilityVersion.Version_2_2)]
        [InlineData(CompatibilityVersion.Latest)]
        public void PostConfigure_SetsAllowRecompilingViewsOnFileChange_InDevelopmentMode(CompatibilityVersion version)
        {
            // Arrange
            var options = new RazorViewEngineOptions();
            var hostingEnv = Mock.Of<IHostingEnvironment>(env => env.EnvironmentName == EnvironmentName.Development);
            var compatibilityVersion = new MvcCompatibilityOptions { CompatibilityVersion = version };
            var optionsSetup = GetSetup(hostingEnv, compatibilityOptions: compatibilityVersion);

            // Act
            optionsSetup.Configure(options);
            optionsSetup.PostConfigure(string.Empty, options);

            // Assert
            Assert.True(options.AllowRecompilingViewsOnFileChange);
        }

        [Theory]
        [InlineData(CompatibilityVersion.Version_2_2)]
        [InlineData(CompatibilityVersion.Latest)]
        public void PostConfigure_DoesNotSetAllowRecompilingViewsOnFileChange_WhenNotInDevelopment(CompatibilityVersion version)
        {
            // Arrange
            var options = new RazorViewEngineOptions();
            var hostingEnv = Mock.Of<IHostingEnvironment>(env => env.EnvironmentName == EnvironmentName.Staging);
            var compatibilityVersion = new MvcCompatibilityOptions { CompatibilityVersion = version };
            var optionsSetup = GetSetup(hostingEnv, compatibilityOptions: compatibilityVersion);

            // Act
            optionsSetup.Configure(options);
            optionsSetup.PostConfigure(string.Empty, options);

            // Assert
            Assert.False(options.AllowRecompilingViewsOnFileChange);
        }

        [Fact]
        public void RazorViewEngineOptionsSetup_DoesNotOverwriteAllowRecompilingViewsOnFileChange_In21CompatMode()
        {
            // Arrange
            var hostingEnv = Mock.Of<IHostingEnvironment>(env => env.EnvironmentName == EnvironmentName.Staging);
            var compatibilityVersion = new MvcCompatibilityOptions { CompatibilityVersion = CompatibilityVersion.Version_2_1 };
            var optionsSetup = GetSetup(hostingEnv, compatibilityOptions: compatibilityVersion);
            var serviceProvider = new ServiceCollection()
                .AddOptions()
                .AddSingleton<IConfigureOptions<RazorViewEngineOptions>>(optionsSetup)
                .Configure<RazorViewEngineOptions>(o => o.AllowRecompilingViewsOnFileChange = false)
                .BuildServiceProvider();

            // Act
            var options = serviceProvider.GetRequiredService<IOptions<RazorViewEngineOptions>>();

            // Assert
            Assert.False(options.Value.AllowRecompilingViewsOnFileChange);
        }

        [Fact]
        public void RazorViewEngineOptionsSetup_ConfiguresAllowRecompilingViewsOnFileChange()
        {
            // Arrange
            var hostingEnv = Mock.Of<IHostingEnvironment>(env => env.EnvironmentName == EnvironmentName.Production);
            var compatibilityVersion = new MvcCompatibilityOptions { CompatibilityVersion = CompatibilityVersion.Version_2_2 };
            var optionsSetup = GetSetup(hostingEnv, compatibilityOptions: compatibilityVersion);
            var serviceProvider = new ServiceCollection()
                .AddOptions()
                .AddSingleton<IConfigureOptions<RazorViewEngineOptions>>(optionsSetup)
                .BuildServiceProvider();

            // Act
            var options = serviceProvider.GetRequiredService<IOptions<RazorViewEngineOptions>>();

            // Assert
            Assert.False(options.Value.AllowRecompilingViewsOnFileChange);
        }

        [Fact]
        public void RazorViewEngineOptionsSetup_DoesNotOverwriteAllowRecompilingViewsOnFileChange()
        {
            // Arrange
            var hostingEnv = Mock.Of<IHostingEnvironment>(env => env.EnvironmentName == EnvironmentName.Production);
            var compatibilityVersion = new MvcCompatibilityOptions { CompatibilityVersion = CompatibilityVersion.Version_2_2 };
            var optionsSetup = GetSetup(hostingEnv, compatibilityOptions: compatibilityVersion);
            var serviceProvider = new ServiceCollection()
                .AddOptions()
                .AddSingleton<IConfigureOptions<RazorViewEngineOptions>>(optionsSetup)
                .Configure<RazorViewEngineOptions>(o => o.AllowRecompilingViewsOnFileChange = true)
                .BuildServiceProvider();

            // Act
            var options = serviceProvider.GetRequiredService<IOptions<RazorViewEngineOptions>>();

            // Assert
            Assert.True(options.Value.AllowRecompilingViewsOnFileChange);
        }

        private static RazorViewEngineOptionsSetup GetSetup(
            IHostingEnvironment hostingEnvironment = null, 
            MvcCompatibilityOptions compatibilityOptions = null)
        {
            hostingEnvironment = hostingEnvironment ?? Mock.Of<IHostingEnvironment>();
            compatibilityOptions = compatibilityOptions ?? new MvcCompatibilityOptions();

            return new RazorViewEngineOptionsSetup(
                hostingEnvironment,
                NullLoggerFactory.Instance,
                Options.Create(compatibilityOptions));
        }

    }
}
