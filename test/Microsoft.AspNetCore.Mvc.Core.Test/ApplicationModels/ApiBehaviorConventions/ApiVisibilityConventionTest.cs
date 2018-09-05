// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public class ApiVisibilityConventionTest
    {
        [Fact]
        public void Apply_SetsApiExplorerVisibility()
        {
            // Arrange
            var controller = new ControllerModel(typeof(object).GetTypeInfo(), new object[0]);
            var convention = new ApiVisibilityConvention();

            // Act
            convention.Apply(controller);

            // Assert
            Assert.True(controller.ApiExplorer.IsVisible);
        }

        [Fact]
        public void Apply_DoesNotSetApiExplorerVisibility_IfAlreadySpecified()
        {
            // Arrange
            var controller = new ControllerModel(typeof(object).GetTypeInfo(), new object[0])
            {
                ApiExplorer = { IsVisible = false, },
            };
            var convention = new ApiVisibilityConvention();

            // Act
            convention.Apply(controller);

            // Assert
            Assert.False(controller.ApiExplorer.IsVisible);
        }
    }
}
