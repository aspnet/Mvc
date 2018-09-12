﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public class ApiVisibilityConventionTest
    {
        [Fact]
        public void Apply_SetsApiExplorerVisibility()
        {
            // Arrange
            var action = GetActionModel();
            var convention = new ApiVisibilityConvention();

            // Act
            convention.Apply(action);

            // Assert
            Assert.True(action.ApiExplorer.IsVisible);
        }

        [Fact]
        public void Apply_DoesNotSetApiExplorerVisibility_IfAlreadySpecified()
        {
            // Arrange
            var action = GetActionModel();
            action.ApiExplorer.IsVisible = false;
            var convention = new ApiVisibilityConvention();

            // Act
            convention.Apply(action);

            // Assert
            Assert.False(action.ApiExplorer.IsVisible);
        }

        private static ActionModel GetActionModel()
        {
            return new ActionModel(typeof(object).GetMethods()[0], new object[0]);
        }
    }
}
