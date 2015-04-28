// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.IntegrationTests
{
    public static class ModelBindingTestHelper
    {
        public static OperationBindingContext GetOperationBindingContext()
        {
            var httpContext = ModelBindingTestHelper.GetHttpContext();
            return GetOperationBindingContext(httpContext);
        }

        public static OperationBindingContext GetOperationBindingContext(
            HttpContext httpContext,
            Action<MvcOptions> optionsSetup = null)
        {
            if (httpContext.RequestServices == null)
            {
                InitializeServices(httpContext, optionsSetup);
            }

            var actionBindingContextAccessor =
              httpContext.RequestServices.GetRequiredService<IScopedInstance<ActionBindingContext>>().Value;
            return new OperationBindingContext()
            {
                BodyBindingState = BodyBindingState.NotBodyBased,
                HttpContext = httpContext,
                MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                ValidatorProvider = actionBindingContextAccessor.ValidatorProvider,
                ValueProvider = actionBindingContextAccessor.ValueProvider,
                ModelBinder = actionBindingContextAccessor.ModelBinder
            };
        }

        public static DefaultControllerActionArgumentBinder GetArgumentBinder()
        {
            var options = new TestMvcOptions();
            options.Options.MaxModelValidationErrors = 5;
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            return new DefaultControllerActionArgumentBinder(
                metadataProvider,
                new DefaultObjectValidator(
                    options.Options.ValidationExcludeFilters,
                    metadataProvider));
        }

        public static HttpContext GetHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            InitializeServices(httpContext);
            return httpContext;
        }

        private static void InitializeServices(HttpContext httpContext, Action<MvcOptions> optionsSetup = null)
        {
            var serviceCollection = MvcServices.GetDefaultServices();
            httpContext.RequestServices = serviceCollection.BuildServiceProvider();

            var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());

            var actionContextAccessor =
                httpContext.RequestServices.GetRequiredService<IScopedInstance<ActionContext>>();
            actionContextAccessor.Value = actionContext;

            var options = new TestMvcOptions().Options;
            if (optionsSetup != null)
            {
                optionsSetup(options);
            }

            var actionBindingContextAccessor =
                httpContext.RequestServices.GetRequiredService<IScopedInstance<ActionBindingContext>>();
            actionBindingContextAccessor.Value = GetActionBindingContext(options, actionContext);
        }

        private static ActionBindingContext GetActionBindingContext(MvcOptions options, ActionContext actionContext)
        {
            var valueProviderFactoryContext = new ValueProviderFactoryContext(
                actionContext.HttpContext,
                actionContext.RouteData.Values);

            var valueProvider = CompositeValueProvider.Create(
                options.ValueProviderFactories,
                valueProviderFactoryContext);

            return new ActionBindingContext()
            {
                InputFormatters = options.InputFormatters,
                OutputFormatters = options.OutputFormatters, // Not required for model binding.
                ValidatorProvider = new TestModelValidatorProvider(options.ModelValidatorProviders),
                ModelBinder = new CompositeModelBinder(options.ModelBinders),
                ValueProvider = valueProvider
            };
        }
    }
}