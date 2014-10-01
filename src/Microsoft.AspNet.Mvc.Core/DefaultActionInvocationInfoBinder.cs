// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;
using System.Linq;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultActionInvocationInfoBinder : IActionInvocationInfoBinder
    {
        public async Task<IDictionary<string, object>> GetActionInvocationInfoAsync(ActionBindingContext actionBindingContext)
        {
            var modelState = actionBindingContext.ActionContext.ModelState;
            var metadataProvider = actionBindingContext.MetadataProvider;

            var controller = actionBindingContext.ActionContext.Controller;
            var controllerMetadatas = metadataProvider.GetMetadataForProperties(controller, controller.GetType())
                                                      .Where(metadata => metadata.Marker != null);

            var parameters = actionBindingContext.ActionContext.ActionDescriptor.Parameters;
            var parameterMetadatas = parameters.Select(parameter =>
            {
                var parameterType = parameter.ParameterBindingInfo.ParameterType;

                // TODO: Artifitially annotate the parameter with a BindAlwaysAttribute
                // to ensure that the top level object is not null.
                // This is to have compat with mvc. 
                var parameterAttributes = parameter.ParameterBindingInfo.Attributes;
                return metadataProvider.GetMetadataForParameter(
                    modelAccessor: null,
                    parameterType: parameterType,
                    parameterAttributes: parameterAttributes,
                    parameterName: parameter.Name);
            });

            var parameterValues = new ActionRuntimeParameterInfo(actionBindingContext.ActionContext.Controller);
            await PopulateActionInvocationInfoAsync(controllerMetadatas, actionBindingContext, false, parameterValues);
            await PopulateActionInvocationInfoAsync(parameterMetadatas, actionBindingContext, true, parameterValues);
            return parameterValues;
        }

        private async Task PopulateActionInvocationInfoAsync(IEnumerable<ModelMetadata> modelMetadatas,
                                                             ActionBindingContext actionBindingContext, 
                                                             bool enableValueProviderBasedBinding, 
                                                             IDictionary<string, object> parameterValues)
        {
            foreach (var modelMetadata in modelMetadatas)
            {
                var parameterType = modelMetadata.ModelType;
                var modelBindingContext = new ModelBindingContext
                {
                    ModelName = modelMetadata.PropertyName,
                    ModelMetadata = modelMetadata,
                    ModelState = actionBindingContext.ActionContext.ModelState,
                    ModelBinder = actionBindingContext.ModelBinder,
                    OriginalValueProviders = actionBindingContext.ValueProviders,
                    ValidatorProvider = actionBindingContext.ValidatorProvider,
                    MetadataProvider = actionBindingContext.MetadataProvider,
                    HttpContext = actionBindingContext.ActionContext.HttpContext,
                    FallbackToEmptyPrefix = true,
                    EnableAutoValueBindingForUnmarkedModels = enableValueProviderBasedBinding,
                    ValueProviders = actionBindingContext.ValueProviders,
                };

                if (await actionBindingContext.ModelBinder.BindModelAsync(modelBindingContext))
                {
                    parameterValues[modelMetadata.PropertyName] = modelBindingContext.Model;
                }
            }
        }
    }
}
