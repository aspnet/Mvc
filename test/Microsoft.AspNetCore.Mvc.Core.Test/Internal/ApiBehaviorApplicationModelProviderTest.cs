// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ApiBehaviorApplicationModelProviderTest
    {
        [Fact]
        public void OnProvidersExecuting_ThrowsIfControllerWithAttribute_HasActionsWithoutAttributeRouting()
        {
            // Arrange
            var actionName = $"{typeof(TestApiController).FullName}.{nameof(TestApiController.TestAction)} ({typeof(TestApiController).Assembly.GetName().Name})";
            var expected = $"Action '{actionName}' does not have an attribute route. Action methods on controllers annotated with ApiControllerAttribute must be attribute routed.";

            var controllerModel = new ControllerModel(typeof(TestApiController).GetTypeInfo(), new[] { new ApiControllerAttribute() });
            var method = typeof(TestApiController).GetMethod(nameof(TestApiController.TestAction));
            var actionModel = new ActionModel(method, Array.Empty<object>())
            {
                Controller = controllerModel,
            };
            controllerModel.Actions.Add(actionModel);

            var context = new ApplicationModelProviderContext(new[] { controllerModel.ControllerType });
            context.Result.Controllers.Add(controllerModel);

            var provider = GetProvider();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => provider.OnProvidersExecuting(context));
            Assert.Equal(expected, ex.Message);
        }

        [Fact]
        public void OnProvidersExecuting_AppliesConventions()
        {
            // Arrange
            var controllerModel = new ControllerModel(typeof(TestApiController).GetTypeInfo(), new[] { new ApiControllerAttribute() })
            {
                Selectors = { new SelectorModel { AttributeRouteModel = new AttributeRouteModel() } },
            };

            var method = typeof(TestApiController).GetMethod(nameof(TestApiController.TestAction));

            var actionModel = new ActionModel(method, Array.Empty<object>())
            {
                Controller = controllerModel,
            };
            controllerModel.Actions.Add(actionModel);

            var parameter = method.GetParameters()[0];
            var parameterModel = new ParameterModel(parameter, Array.Empty<object>())
            {
                Action = actionModel,
            };
            actionModel.Parameters.Add(parameterModel);

            var context = new ApplicationModelProviderContext(new[] { controllerModel.ControllerType });
            context.Result.Controllers.Add(controllerModel);

            var provider = GetProvider();

            // Act
            provider.OnProvidersExecuting(context);

            // Assert
            // Verify some of the side-effects of executing API behavior conventions.
            Assert.True(controllerModel.ApiExplorer.IsVisible);
            Assert.NotEmpty(actionModel.Filters.OfType<ModelStateInvalidFilter>());
            Assert.NotEmpty(actionModel.Filters.OfType<ClientErrorResultFilter>());
            Assert.Equal(BindingSource.Body, parameterModel.BindingInfo.BindingSource);
        }

        private static ApiBehaviorApplicationModelProvider GetProvider(
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
            return new ApiBehaviorApplicationModelProvider(
                optionsAccessor,
                modelMetadataProvider,
                Mock.Of<IClientErrorFactory>(),
                loggerFactory);
        }

        private static ApplicationModelProviderContext GetContext(
            Type type,
            IModelMetadataProvider modelMetadataProvider = null)
        {
            var context = new ApplicationModelProviderContext(new[] { type.GetTypeInfo() });
            var mvcOptions = Options.Create(new MvcOptions { AllowValidatingTopLevelNodes = true });
            modelMetadataProvider = modelMetadataProvider ?? new EmptyModelMetadataProvider();
            var provider = new DefaultApplicationModelProvider(mvcOptions, modelMetadataProvider);
            provider.OnProvidersExecuting(context);

            return context;
        }

        private class TestApiController : ControllerBase
        {
            public IActionResult TestAction(object value) => null;
        }
    }
}
