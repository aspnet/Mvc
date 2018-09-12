// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public class InvalidModelStateFilterConventionTest
    {
        [Fact]
        public void Apply_AddsFilter()
        {
            // Arrange
            var controller = GetControllerModel();
            var convention = GetConvention();

            // Act
            convention.Apply(controller);

            // Assert
            var action = controller.Actions[0];
            Assert.Single(action.Filters.OfType<ModelStateInvalidFilterFactory>());
        }

        private InvalidModelStateFilterConvention GetConvention()
        {
            return new InvalidModelStateFilterConvention();
        }

        private static ControllerModel GetControllerModel()
        {
            var controllerModel = new ControllerModel(typeof(object).GetTypeInfo(), new object[0])
            {
                Actions =
                {
                    new ActionModel(typeof(object).GetMethods()[0], new object[0]),
                }
            };

            return controllerModel;
        }
    }
}
