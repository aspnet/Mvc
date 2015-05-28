// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.AspNet.Mvc.IntegrationTests
{
    public static class ModelBindingTestHelper
    {
        public static HttpContext GetHttpContext(
            Action<HttpRequest> updateRequest = null,
            Action<MvcOptions> updateOptions = null)
        {
            var httpContext = new DefaultHttpContext();

            if (updateRequest != null)
            {
                updateRequest(httpContext.Request);
            }

            InitializeServices(httpContext, updateOptions);
            return httpContext;
        }

        public static OperationBindingContext GetOperationBindingContext(
            Action<HttpRequest> updateRequest = null,
            Action<MvcOptions> updateOptions = null)
        {
            var httpContext = GetHttpContext(updateRequest, updateOptions);

            var services = httpContext.RequestServices;
            var actionBindingContext = services.GetRequiredService<IScopedInstance<ActionBindingContext>>().Value;

            return new OperationBindingContext()
            {
                BodyBindingState = BodyBindingState.NotBodyBased,
                HttpContext = httpContext,
                InputFormatters = actionBindingContext.InputFormatters,
                MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                ValidatorProvider = actionBindingContext.ValidatorProvider,
                ValueProvider = actionBindingContext.ValueProvider,
                ModelBinder = actionBindingContext.ModelBinder
            };
        }

        public static DefaultControllerActionArgumentBinder GetArgumentBinder(MvcOptions options = null)
        {
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            return new DefaultControllerActionArgumentBinder(
                metadataProvider,
                GetObjectValidator(options));
        }

        public static IObjectModelValidator GetObjectValidator(MvcOptions options = null)
        {
            options = options ?? new TestMvcOptions().Options;
            options.MaxModelValidationErrors = 5;
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            return new DefaultObjectValidator(
                    options.ValidationExcludeFilters,
                    metadataProvider);
        }

        public static void UpdateOptionsToEnsureNothingFollows<TModelBinder>(MvcOptions options)
        {
            var index = 0;
            for (; index < options.ModelBinders.Count; index++)
            {
                if (typeof(TModelBinder) == options.ModelBinders[index].GetType())
                {
                    options.ModelBinders.Insert(index + 1, new AssertIfCalledModelBinder());
                    return;
                }
            }

            Assert.False(true, $"Unable to find { typeof(TModelBinder).FullName } in { nameof(MvcOptions) }.");
        }

        private static void InitializeServices(HttpContext httpContext, Action<MvcOptions> updateOptions = null)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMvc();

            httpContext.RequestServices = serviceCollection.BuildServiceProvider();

            var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());

            var actionContextAccessor =
                httpContext.RequestServices.GetRequiredService<IScopedInstance<ActionContext>>();
            actionContextAccessor.Value = actionContext;

            var options = new TestMvcOptions().Options;
            if (updateOptions != null)
            {
                updateOptions(options);
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

        private class AssertIfCalledModelBinder : IModelBinder
        {
            public Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
            {
                Assert.False(true, "This model binder should never be used");

                return Task.FromResult<ModelBindingResult>(result: null);
            }
        }
    }
}
