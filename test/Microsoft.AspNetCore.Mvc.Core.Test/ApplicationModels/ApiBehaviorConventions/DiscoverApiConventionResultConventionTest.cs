// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public class DiscoverApiConventionResultConventionTest
    {
        [Fact]
        public void Apply_DoesNotAddConventionItem_IfActionHasProducesResponseTypeAttribute()
        {
            // Arrange
            var actionModel = CreateModel(nameof(TestApiConventionController.Delete));
            actionModel.Filters.Add(new ProducesResponseTypeAttribute(200));

            var convention = GetConvention();

            // Act
            convention.Apply(actionModel);

            // Assert
            Assert.Empty(actionModel.Properties);
        }

        [Fact]
        public void Apply_DoesNotAddConventionItem_IfActionHasProducesAttribute()
        {
            // Arrange
            var actionModel = CreateModel(nameof(TestApiConventionController.Delete));
            actionModel.Filters.Add(new ProducesAttribute(typeof(object)));

            var convention = GetConvention();

            // Act
            convention.Apply(actionModel);

            // Assert
            Assert.Empty(actionModel.Properties);
        }

        [Fact]
        public void Apply_DoesNotAddConventionItem_IfNoConventionMatches()
        {
            // Arrange
            var actionModel = CreateModel(nameof(TestApiConventionController.NoMatch));
            var convention = GetConvention();

            // Act
            convention.Apply(actionModel);

            // Assert
            Assert.Empty(actionModel.Properties);
        }

        [Fact]
        public void Apply_AddsConventionItem_IfConventionMatches()
        {
            // Arrange
            var actionModel = CreateModel(nameof(TestApiConventionController.Delete));
            var convention = GetConvention();

            // Act
            convention.Apply(actionModel);

            // Assert
            Assert.Collection(
                actionModel.Properties,
                kvp =>
                {
                    Assert.Equal(typeof(ApiConventionResult), kvp.Key);
                    Assert.NotNull(kvp.Value);
                });
        }

        [Fact]
        public void DiscoverApiConvention_AddsConventionItem_IfActionHasNonConventionBasedFilters()
        {
            // Arrange
            var actionModel = CreateModel(nameof(TestApiConventionController.Delete));
            actionModel.Filters.Add(new AuthorizeFilter());
            var convention = GetConvention();

            // Act
            convention.Apply(actionModel);

            // Assert
            Assert.Collection(
                actionModel.Properties,
                kvp =>
                {
                    Assert.Equal(typeof(ApiConventionResult), kvp.Key);
                    Assert.NotNull(kvp.Value);
                });
        }

        private DiscoverApiConventionResultConvention GetConvention()
        {
            return new DiscoverApiConventionResultConvention();
        }

        private static ActionModel CreateModel(
            string actionName,
            object[] actionAttributes = null, 
            object[] controllerAttributes = null)
        {
            actionAttributes = actionAttributes ?? Array.Empty<object>();
            controllerAttributes = controllerAttributes ?? new[] { new ApiConventionTypeAttribute(typeof(DefaultApiConventions)) };

            var controllerModel = new ControllerModel(typeof(TestApiConventionController).GetTypeInfo(), controllerAttributes);
            var actionModel = new ActionModel(typeof(TestApiConventionController).GetMethod(actionName), actionAttributes)
            {
                Controller = controllerModel,
            };
            return actionModel;
        }

        private class TestApiConventionController
        {
            public IActionResult NoMatch() => null;

            public IActionResult Delete(int id) => null;
        }
    }
}
