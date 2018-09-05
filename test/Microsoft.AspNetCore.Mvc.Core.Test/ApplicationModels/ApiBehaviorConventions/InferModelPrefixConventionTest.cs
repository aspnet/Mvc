// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public class InferModelPrefixConventionTest
    {
        [Fact]
        public void InferBoundPropertyModelPrefixes_SetsModelPrefix_ForComplexTypeFromValueProvider()
        {
            // Arrange
            var controller = GetControllerModel(typeof(ControllerWithBoundProperty));
            var convention = GetConvention();

            // Act
            convention.InferBoundPropertyModelPrefixes(controller);

            // Assert
            var property = Assert.Single(controller.ControllerProperties);
            Assert.Equal(string.Empty, property.BindingInfo.BinderModelName);
        }

        [Fact]
        public void InferBoundPropertyModelPrefixes_SetsModelPrefix_ForCollectionTypeFromValueProvider()
        {
            // Arrange
            var controller = GetControllerModel(typeof(ControllerWithBoundCollectionProperty));
            var convention = GetConvention();

            // Act
            convention.InferBoundPropertyModelPrefixes(controller);

            // Assert
            var property = Assert.Single(controller.ControllerProperties);
            Assert.Null(property.BindingInfo.BinderModelName);
        }

        [Fact]
        public void InferParameterModelPrefixes_SetsModelPrefix_ForComplexTypeFromValueProvider()
        {
            // Arrange
            var action = GetActionModel(typeof(ControllerWithBoundProperty), nameof(ControllerWithBoundProperty.SomeAction));
            var convention = GetConvention();

            // Act
            convention.InferParameterModelPrefixes(action);

            // Assert
            var parameter = Assert.Single(action.Parameters);
            Assert.Equal(string.Empty, parameter.BindingInfo.BinderModelName);
        }

        private static InferModelPrefixConvention GetConvention(
            ApiBehaviorOptions options = null,
            IModelMetadataProvider modelMetadataProvider = null)
        {
            options = options ?? new ApiBehaviorOptions
            {
                InvalidModelStateResponseFactory = _ => null,
            };
            var optionsAccessor = Options.Create(options);

            var loggerFactory = NullLoggerFactory.Instance;
            modelMetadataProvider = modelMetadataProvider ?? new EmptyModelMetadataProvider();
            return new InferModelPrefixConvention(Options.Create(options), modelMetadataProvider);
        }

        private static ApplicationModelProviderContext GetContext(
            Type type,
            IModelMetadataProvider modelMetadataProvider = null)
        {
            var context = new ApplicationModelProviderContext(new[] { type.GetTypeInfo() });
            var mvcOptions = Options.Create(new MvcOptions { AllowValidatingTopLevelNodes = true });
            modelMetadataProvider = modelMetadataProvider ?? new EmptyModelMetadataProvider();
            var convention = new DefaultApplicationModelProvider(mvcOptions, modelMetadataProvider);
            convention.OnProvidersExecuting(context);

            return context;
        }

        private static ControllerModel GetControllerModel(Type controllerType)
        {
            var context = GetContext(controllerType);
            return Assert.Single(context.Result.Controllers);
        }

        private static ActionModel GetActionModel(Type controllerType, string actionName)
        {
            var context = GetContext(controllerType);
            var controller = Assert.Single(context.Result.Controllers);
            return Assert.Single(controller.Actions, m => m.ActionName == actionName);
        }
        
        private class TestModel { }

        private class ControllerWithBoundProperty
        {
            [FromQuery]
            public TestModel TestProperty { get; set; }

            public IActionResult SomeAction([FromQuery] TestModel test) => null;
        }

        private class ControllerWithBoundCollectionProperty
        {
            [FromQuery]
            public List<int> TestProperty { get; set; }

            public IActionResult SomeAction([FromQuery] List<int> test) => null;
        }
    }
}
