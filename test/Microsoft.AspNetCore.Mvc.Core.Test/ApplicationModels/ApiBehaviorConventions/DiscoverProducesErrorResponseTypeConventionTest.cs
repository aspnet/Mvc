// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Xunit;

[assembly: ProducesErrorResponseType(typeof(InvalidEnumArgumentException))]

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public class DiscoverProducesErrorResponseTypeConventionTest
    {
        [Fact]
        public void Apply_SetsProblemDetails_IfActionHasNoAttributes()
        {
            // Arrange
            var expected = typeof(ProblemDetails);
            var action = GetActionModel(controllerType: typeof(object));
            var convention = GetConvention();

            // Act
            convention.Apply(action);

            // Assert
            Assert.Collection(
                action.Properties,
                kvp =>
                {
                    Assert.Equal(typeof(ProducesErrorResponseTypeAttribute), kvp.Key);
                    var value = Assert.IsType<ProducesErrorResponseTypeAttribute>(kvp.Value);
                    Assert.Equal(expected, value.Type);
                });
        }

        [Fact]
        public void DiscoverErrorResponseType_DoesNotSetDefaultProblemDetailsResponse_IfSuppressMapClientErrorsIsSet()
        {
            // Arrange
            var action = GetActionModel(controllerType: typeof(object));
            var convention = GetConvention(new ApiBehaviorOptions
            {
                SuppressMapClientErrors = true,
            });

            // Act
            convention.Apply(action);

            // Assert
            Assert.Empty(action.Properties);
        }

        [Fact]
        public void DiscoverErrorResponseType_UsesValueFromApiErrorTypeAttribute_SpecifiedOnControllerAsssembly()
        {
            // Arrange
            var expected = typeof(InvalidEnumArgumentException);
            var action = GetActionModel();
            var convention = GetConvention();

            // Act
            convention.Apply(action);

            // Assert
            Assert.Collection(
                action.Properties,
                kvp =>
                {
                    Assert.Equal(typeof(ProducesErrorResponseTypeAttribute), kvp.Key);
                    var value = Assert.IsType<ProducesErrorResponseTypeAttribute>(kvp.Value);
                    Assert.Equal(expected, value.Type);
                });
        }

        [Fact]
        public void DiscoverErrorResponseType_UsesValueFromApiErrorTypeAttribute_SpecifiedOnController()
        {
            // Arrange
            var expected = typeof(InvalidTimeZoneException);
            var action = GetActionModel(controllerAttributes: new[] { new ProducesErrorResponseTypeAttribute(expected) });
            var convention = GetConvention();

            // Act
            convention.Apply(action);

            // Assert
            Assert.Collection(
                action.Properties,
                kvp =>
                {
                    Assert.Equal(typeof(ProducesErrorResponseTypeAttribute), kvp.Key);
                    var value = Assert.IsType<ProducesErrorResponseTypeAttribute>(kvp.Value);
                    Assert.Equal(expected, value.Type);
                });
        }

        [Fact]
        public void DiscoverErrorResponseType_UsesValueFromApiErrorTypeAttribute_SpecifiedOnAction()
        {
            // Arrange
            var expected = typeof(InvalidTimeZoneException);
            var action = GetActionModel(
                actionAttributes: new[] { new ProducesErrorResponseTypeAttribute(expected) },
                controllerAttributes: new[] { new ProducesErrorResponseTypeAttribute(typeof(Guid)) });
            var convention = GetConvention();

            // Act
            convention.Apply(action);

            // Assert
            Assert.Collection(
                action.Properties,
                kvp =>
                {
                    Assert.Equal(typeof(ProducesErrorResponseTypeAttribute), kvp.Key);
                    var value = Assert.IsType<ProducesErrorResponseTypeAttribute>(kvp.Value);
                    Assert.Equal(expected, value.Type);
                });
        }

        [Fact]
        public void DiscoverErrorResponseType_AllowsVoidsType()
        {
            // Arrange
            var expected = typeof(void);
            var action = GetActionModel(new[] { new ProducesErrorResponseTypeAttribute(expected) });
            var convention = GetConvention();

            // Act
            convention.Apply(action);

            // Assert
            Assert.Collection(
                action.Properties,
                kvp =>
                {
                    Assert.Equal(typeof(ProducesErrorResponseTypeAttribute), kvp.Key);
                    var value = Assert.IsType<ProducesErrorResponseTypeAttribute>(kvp.Value);
                    Assert.Equal(expected, value.Type);
                });
        }

        private static DiscoverProducesErrorResponseTypeConvention GetConvention(ApiBehaviorOptions options = null)
        {
            options = options ?? new ApiBehaviorOptions();
            return new DiscoverProducesErrorResponseTypeConvention(Options.Create(options));
        }

        private class TestApiConventionController
        {
            public IActionResult Action(int id) => null;
        }

        private static ActionModel GetActionModel(
            object[] actionAttributes = null,
            object[] controllerAttributes = null,
            Type controllerType = null)
        {
            var actionName = nameof(TestApiConventionController.Action);
            actionAttributes = actionAttributes ?? Array.Empty<object>();
            controllerAttributes = controllerAttributes ?? Array.Empty<object>();
            controllerType = controllerType ?? typeof(TestApiConventionController);

            var controllerModel = new ControllerModel(controllerType.GetTypeInfo(), controllerAttributes);
            var actionModel = new ActionModel(typeof(TestApiConventionController).GetMethod(actionName), actionAttributes)
            {
                Controller = controllerModel,
            };
            return actionModel;
        }
    }
}
