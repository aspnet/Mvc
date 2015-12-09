// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc.Controllers;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;

namespace Microsoft.AspNet.Mvc.IntegrationTests
{
    public static class ModelBindingTestHelper
    {
        public static OperationBindingContext GetOperationBindingContext(
            Action<HttpRequest> updateRequest = null,
            Action<MvcOptions> updateOptions = null)
        {
            var httpContext = GetHttpContext(updateRequest, updateOptions);
            var services = httpContext.RequestServices;

            var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
            var controllerContext = GetControllerContext(
                services.GetRequiredService<IOptions<MvcOptions>>().Value,
                actionContext);

            return new OperationBindingContext()
            {
                ActionContext = controllerContext,
                InputFormatters = controllerContext.InputFormatters,
                MetadataProvider = TestModelMetadataProvider.CreateDefaultProvider(),
                ValidatorProvider = new CompositeModelValidatorProvider(controllerContext.ValidatorProviders),
                ValueProvider = new CompositeValueProvider(controllerContext.ValueProviders),
                ModelBinder = new CompositeModelBinder(controllerContext.ModelBinders),
            };
        }

        public static DefaultControllerActionArgumentBinder GetArgumentBinder(MvcOptions options = null)
        {
            if (options == null)
            {
                var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
                return GetArgumentBinder(metadataProvider);
            }
            else
            {
                var metadataProvider = TestModelMetadataProvider.CreateProvider(options.ModelMetadataDetailsProviders);
                return GetArgumentBinder(metadataProvider);
            }
        }

        public static DefaultControllerActionArgumentBinder GetArgumentBinder(IModelMetadataProvider metadataProvider)
        {
            return new DefaultControllerActionArgumentBinder(
                metadataProvider,
                GetObjectValidator(metadataProvider));
        }

        public static IObjectModelValidator GetObjectValidator(IModelMetadataProvider metadataProvider)
        {
            return new DefaultObjectValidator(metadataProvider);
        }

        private static HttpContext GetHttpContext(
            Action<HttpRequest> updateRequest = null,
            Action<MvcOptions> updateOptions = null)
        {
            var httpContext = new DefaultHttpContext();

            if (updateRequest != null)
            {
                updateRequest(httpContext.Request);
            }

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMvc();
            serviceCollection.AddTransient<ILoggerFactory, LoggerFactory>();

            if (updateOptions != null)
            {
                serviceCollection.Configure(updateOptions);
            }

            httpContext.RequestServices = serviceCollection.BuildServiceProvider();
            return httpContext;
        }

        private static ControllerContext GetControllerContext(MvcOptions options, ActionContext context)
        {
            var valueProviders = new List<IValueProvider>();
            foreach (var factory in options.ValueProviderFactories)
            {
                var valueProvider = factory.GetValueProviderAsync(context).Result;
                if (valueProvider != null)
                {
                    valueProviders.Add(valueProvider);
                }
            }

            return new ControllerContext(context)
            {
                InputFormatters = options.InputFormatters,
                ValidatorProviders = options.ModelValidatorProviders,
                ModelBinders = options.ModelBinders,
                ValueProviders = valueProviders
            };
        }
    }
}
