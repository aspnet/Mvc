// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using ActivatorWebSite;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class ActivatorTests
    {
        private const string SiteName = nameof(ActivatorWebSite);
        private readonly Action<IApplicationBuilder> _app = new Startup().Configure;
        private readonly Action<IServiceCollection> _configureServices = new Startup().ConfigureServices;

        [Fact]
        public async Task ControllerThatCannotBeActivated_ThrowsWhenAttemptedToBeInvoked()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();

            var expectedMessage =
                "Unable to resolve service for type 'ActivatorWebSite.CannotBeActivatedController+FakeType' while " +
                "attempting to activate 'ActivatorWebSite.CannotBeActivatedController'.";

            // Act & Assert
            var response = await client.GetAsync("http://localhost/CannotBeActivated/Index");

            var exception = response.GetServerException();
            Assert.Equal(typeof(InvalidOperationException).FullName, exception.ExceptionType);
            Assert.Equal(expectedMessage, exception.ExceptionMessage);
        }

        [Fact]
        public async Task PropertiesForPocoControllersAreInitialized()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var expected = "4|some-text";

            // Act
            var response = await client.GetAsync("http://localhost/Plain?foo=some-text");

            // Assert
            var headerValue = Assert.Single(response.Headers.GetValues("X-Fake-Header"));
            Assert.Equal("Fake-Value", headerValue);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(expected, body);
        }

        [Fact]
        public async Task PropertiesForTypesDerivingFromControllerAreInitialized()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var expected = "Hello world";

            // Act
            var body = await client.GetStringAsync("http://localhost/Regular");

            // Assert
            Assert.Equal(expected, body);
        }

        [Fact]
        public async Task ViewActivator_ActivatesDefaultInjectedProperties()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var expected = @"<label for=""Hello"">Hello</label> world! /View/ConsumeServicesFromBaseType";

            // Act
            var body = await client.GetStringAsync("http://localhost/View/ConsumeDefaultProperties");

            // Assert
            Assert.Equal(expected, body.Trim());
        }

        [Fact]
        public async Task ViewActivator_ActivatesAndContextualizesInjectedServices()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var expected = "4 test-value";

            // Act
            var body = await client.GetStringAsync("http://localhost/View/ConsumeInjectedService?test=test-value");

            // Assert
            Assert.Equal(expected, body.Trim());
        }

        [Fact]
        public async Task ViewActivator_ActivatesServicesFromBaseType()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var expected = @"/content/scripts/test.js";

            // Act
            var body = await client.GetStringAsync("http://localhost/View/ConsumeServicesFromBaseType");

            // Assert
            Assert.Equal(expected, body.Trim());
        }

        [Fact]
        public async Task ViewComponentActivator_Activates()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var expected = @"Random Number:4";

            // Act
            var body = await client.GetStringAsync("http://localhost/View/ConsumeViewComponent");

            // Assert
            Assert.Equal(expected, body.Trim());
        }


        [Fact]
        public async Task ViewComponentThatCannotBeActivated_ThrowsWhenAttemptedToBeInvoked()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var expectedMessage =
                "Unable to resolve service for type 'ActivatorWebSite.CannotBeActivatedComponent+FakeType' " +
                "while attempting to activate 'ActivatorWebSite.CannotBeActivatedComponent'.";

            // Act & Assert
            var response = await client.GetAsync("http://localhost/View/ConsumeCannotBeActivatedComponent");

            var exception = response.GetServerException();
            Assert.Equal(typeof(InvalidOperationException).FullName, exception.ExceptionType);
            Assert.Equal(expectedMessage, exception.ExceptionMessage);
        }

        [Fact]
        public async Task TagHelperActivation_ConstructorInjection_RendersProperly()
        {
            // Arrange
            var server = TestHelper.CreateServer(_app, SiteName, _configureServices);
            var client = server.CreateClient();
            var expected = "<body><h2>Activation Test</h2>" +
                           Environment.NewLine +
                           "<div>FakeFakeFake</div>" +
                           Environment.NewLine +
                           "<span>" +
                           "<input id=\"foo\" name=\"foo\" type=\"hidden\" value=\"test content\" />" +
                           "</span>" +
                           Environment.NewLine +
                           "<footer>Footer from activated ViewData</footer>" +
                           "</body>";

            // Act
            var body = await client.GetStringAsync("http://localhost/View/UseTagHelper");

            // Assert
            Assert.Equal(expected, body.Trim(), ignoreLineEndingDifferences: true);
        }
    }
}