// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ApiControllerApplicationModelProvider : IApplicationModelProvider
    {
        private readonly ApiBehaviorOptions _apiConventions;
        private readonly ModelStateInvalidFilter _modelStateInvalidFilter;

        public ApiControllerApplicationModelProvider(IOptions<MvcOptions> mvcOptions, ILoggerFactory loggerFactory)
        {
            _apiConventions = mvcOptions.Value.ApiBehavior;
            _modelStateInvalidFilter = new ModelStateInvalidFilter(
                mvcOptions.Value,
                loggerFactory.CreateLogger<ModelStateInvalidFilter>());
        }

        /// <remarks>
        /// Order is set to execute after the <see cref="DefaultApplicationModelProvider"/>.
        /// </remarks>
        public int Order => -1000 + 10;

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            foreach (var controllerModel in context.Result.Controllers)
            {
                if (controllerModel.Attributes.OfType<IApiBehaviorMetadata>().Any())
                {
                    // Skip adding the filter if the feature is disabled.
                    if (_apiConventions.InvalidModelStateResponseFactory != null)
                    {
                        controllerModel.Filters.Add(_modelStateInvalidFilter);
                    }
                }

                foreach (var actionModel in controllerModel.Actions)
                {
                    if (actionModel.Attributes.OfType<IApiBehaviorMetadata>().Any())
                    {
                        // Skip adding the filter if the feature is disabled.
                        if (_apiConventions.InvalidModelStateResponseFactory != null)
                        {
                            actionModel.Filters.Add(_modelStateInvalidFilter);
                        }
                    }
                }
            }
        }
    }
}
