// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using ActivatorWebSite;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class ActivatorTests : TestBase
    {
        public ActivatorTests()
            : base("ActivatorWebSite", new Startup().Configure)
        {
        }

        [Fact]
        public async Task ControllerThatCannotBeActivated_ThrowsWhenAttemptedToBeInvoked()
        {
            // Arrange
            var expectedMessage = "TODO: No service for type 'ActivatorWebSite.CannotBeActivatedController+FakeType' " +
                                   "has been registered.";

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => Client.GetAsync("http://localhost/CannotBeActivated/Index"));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task PropertiesForPocoControllersAreInitialized()
        {
            // Arrange
            var expected = "4|some-text";

            // Act
            var result = await Client.GetAsync("http://localhost/Plain?foo=some-text");

            // Assert
            Assert.Equal("Fake-Value", result.HttpContext.Response.Headers["X-Fake-Header"]);
            var body = await result.HttpContext.Response.ReadBodyAsStringAsync();
            Assert.Equal(expected, body);
        }

        [Fact]
        public async Task PropertiesForTypesDerivingFromControllerAreInitialized()
        {
            // Arrange
            var expected = "Hello world";

            // Act
            var result = await Client.GetAsync("http://localhost/Regular");

            // Assert
            var body = await result.HttpContext.Response.ReadBodyAsStringAsync();
            Assert.Equal(expected, body);
        }
    }
}