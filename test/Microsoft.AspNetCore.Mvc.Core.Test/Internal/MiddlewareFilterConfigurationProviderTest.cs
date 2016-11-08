﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class MiddlewareFilterConfigurationProviderTest
    {
        [Fact]
        public void CreateConfigureDelegate_ThrowsIfNoConstructorFoundForType()
        {
            // Arrange
            var provider = new MiddlewareFilterConfigurationProvider();

            // Act
            var exception = Assert.Throws<MissingMethodException>(() => provider.CreateConfigureDelegate(typeof(AbstractType_NoConstructor)));

            // Assert
            Assert.Equal($"Unable to create type {typeof(AbstractType_NoConstructor)}. The class is either abstract or no constructor was found.", exception.Message);
        }

        [Fact]
        public void ValidConfigure_DoesNotThrow()
        {
            // Arrange
            var provider = new MiddlewareFilterConfigurationProvider();

            // Act
            var configureDelegate = provider.CreateConfigureDelegate(typeof(ValidConfigure_WithNoEnvironment));

            // Assert
            Assert.NotNull(configureDelegate);
        }

        [Fact]
        public void ValidConfigure_AndAdditionalServices_DoesNotThrow()
        {
            // Arrange
            var loggerFactory = Mock.Of<ILoggerFactory>();
            var services = new ServiceCollection();
            services.AddSingleton(loggerFactory);
            services.AddSingleton(Mock.Of<IHostingEnvironment>());
            var applicationBuilder = GetApplicationBuilder(services);
            var provider = new MiddlewareFilterConfigurationProvider();

            // Act
            var configureDelegate = provider.CreateConfigureDelegate(typeof(ValidConfigure_WithNoEnvironment_AdditionalServices));

            // Assert
            Assert.NotNull(configureDelegate);
        }

        [Fact]
        public void InvalidType_NoConfigure_Throws()
        {
            // Arrange
            var type = typeof(InvalidType_NoConfigure);
            var provider = new MiddlewareFilterConfigurationProvider();
            var expected = $"A public method named 'Configure' could not be found in the '{type.FullName}' type.";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                provider.CreateConfigureDelegate(type);
            });
            Assert.Equal(expected, exception.Message);
        }

        [Fact]
        public void InvalidType_NoPublicConfigure_Throws()
        {
            // Arrange
            var type = typeof(InvalidType_NoPublic_Configure);
            var provider = new MiddlewareFilterConfigurationProvider();
            var expected = $"A public method named 'Configure' could not be found in the '{type.FullName}' type.";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                provider.CreateConfigureDelegate(type);
            });
            Assert.Equal(expected, exception.Message);
        }

        private IApplicationBuilder GetApplicationBuilder(ServiceCollection services = null)
        {
            if (services == null)
            {
                services = new ServiceCollection();
            }
            var serviceProvider = services.BuildServiceProvider();

            var applicationBuilder = new Mock<IApplicationBuilder>();
            applicationBuilder
                .SetupGet(a => a.ApplicationServices)
                .Returns(serviceProvider);

            return applicationBuilder.Object;
        }

        private class ValidConfigure_WithNoEnvironment
        {
            public void Configure(IApplicationBuilder appBuilder) { }
        }

        private class ValidConfigure_WithNoEnvironment_AdditionalServices
        {
            public void Configure(
                IApplicationBuilder appBuilder,
                IHostingEnvironment hostingEnvironment,
                ILoggerFactory loggerFactory)
            {
                if (hostingEnvironment == null)
                {
                    throw new ArgumentNullException(nameof(hostingEnvironment));
                }
                if (loggerFactory == null)
                {
                    throw new ArgumentNullException(nameof(loggerFactory));
                }
            }
        }

        private class ValidConfigure_WithEnvironment
        {
            public void ConfigureProduction(IApplicationBuilder appBuilder) { }
        }

        private class ValidConfigure_WithEnvironment_AdditionalServices
        {
            public void ConfigureProduction(
                IApplicationBuilder appBuilder,
                IHostingEnvironment hostingEnvironment,
                ILoggerFactory loggerFactory)
            {
                if (hostingEnvironment == null)
                {
                    throw new ArgumentNullException(nameof(hostingEnvironment));
                }
                if (loggerFactory == null)
                {
                    throw new ArgumentNullException(nameof(loggerFactory));
                }
            }
        }

        private class MultipleConfigureWithEnvironments
        {
            public void ConfigureDevelopment(IApplicationBuilder appBuilder)
            {

            }

            public void ConfigureProduction(IApplicationBuilder appBuilder)
            {

            }
        }

        private class InvalidConfigure_NoParameters
        {
            public void Configure()
            {

            }
        }

        private class InvalidType_NoConfigure
        {
            public void Foo(IApplicationBuilder appBuilder)
            {

            }
        }

        private class InvalidType_NoPublic_Configure
        {
            private void Configure(IApplicationBuilder appBuilder)
            {

            }
        }

        private abstract class AbstractType_NoConstructor
        {
        }
    }
}
