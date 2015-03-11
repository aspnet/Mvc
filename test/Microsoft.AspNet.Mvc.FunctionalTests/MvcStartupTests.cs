﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Builder;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class MvcStartupTests
    {
        private const string SiteName = nameof(AddServicesWebSite);
        private readonly Action<IApplicationBuilder> _app = new AddServicesWebSite.Startup().Configure;

        [Fact]
        public void MvcThrowsWhenRequiredServicesAreNotAdded()
        {
            // Arrange
            var expectedMessage = "Unable to find the required services. Please add all the required " +
                "services by calling 'IServiceCollection.AddMvc()' inside the call to 'IApplicationBuilder.UseServices(...)' " +
                "or 'IApplicationBuilder.UseMvc(...)' in the application startup code.";

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => TestHelper.CreateServer(_app, SiteName));
            Assert.Equal(expectedMessage, ex.Message);
        }
    }
}