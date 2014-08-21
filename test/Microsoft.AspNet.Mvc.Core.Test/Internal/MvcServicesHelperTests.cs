﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Core;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class MvcServicesHelperTests
    {
        [Fact]
        public void MvcServicesHelperThrowsIfServiceIsAbsent()
        {
            // Arrange
            var services = new Mock<IServiceProvider>();
            services.Setup(o => o.GetService(typeof(IEnumerable<MvcMarkerService>)))
                .Returns(new List<MvcMarkerService>());
            var expectedMessage = "Unable to find the required services. Please add all the required " +
                "services by calling 'IServiceCollection.AddMvc()' inside the call to 'IBuilder.UseServices(...)' " +
                "or 'IBuilder.UseMvc(...)' in the application startup code.";

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => MvcServicesHelper.ThrowIfServiceDoesNotExist(services.Object, typeof(MvcMarkerService)));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void MvcServicesHelperDoesNotThrowIfServiceExists()
        {
            // Arrange
            var services = new Mock<IServiceProvider>();
            var expectedOutput = new MvcMarkerService();
            services.Setup(o => o.GetService(typeof(IEnumerable<MvcMarkerService>)))
                .Returns(new List<MvcMarkerService> { expectedOutput });

            // Act
            var markerService = MvcServicesHelper.ThrowIfServiceDoesNotExist(services.Object, typeof(MvcMarkerService));

            // Assert
            Assert.NotNull(markerService);
            Assert.Equal(markerService, expectedOutput);
        }
    }
}