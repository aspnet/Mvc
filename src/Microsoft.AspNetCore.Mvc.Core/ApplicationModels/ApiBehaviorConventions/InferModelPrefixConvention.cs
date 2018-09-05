// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// An <see cref="IControllerModelConvention"/> that infers <see cref="BindingInfo.BinderModelName"/>
    /// for bound properties and parameters.
    /// </summary>
    public class InferModelPrefixConvention : IControllerModelConvention
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ApiBehaviorOptions _apiBehaviorOptions;

        public InferModelPrefixConvention(
            IOptions<ApiBehaviorOptions> apiBehaviorOptions,
            IModelMetadataProvider modelMetadataProvider)
        {
            _apiBehaviorOptions = apiBehaviorOptions?.Value ?? throw new ArgumentNullException(nameof(apiBehaviorOptions));
            _modelMetadataProvider = modelMetadataProvider ?? throw new ArgumentNullException(nameof(modelMetadataProvider));
        }

        public void Apply(ControllerModel controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (!ShouldApply(controller))
            {
                return;
            }

            InferBoundPropertyModelPrefixes(controller);

            foreach (var action in controller.Actions)
            {
                InferParameterModelPrefixes(action);
            }
        }

        protected virtual bool ShouldApply(ControllerModel controller) => true;

        // For any complex types that are bound from value providers, set the prefix
        // to the empty prefix by default. This makes binding much more predictable
        // and describable via ApiExplorer
        internal void InferBoundPropertyModelPrefixes(ControllerModel controllerModel)
        {
            foreach (var property in controllerModel.ControllerProperties)
            {
                if (property.BindingInfo != null &&
                    property.BindingInfo.BinderModelName == null &&
                    property.BindingInfo.BindingSource != null &&
                    !property.BindingInfo.BindingSource.IsGreedy)
                {
                    var metadata = _modelMetadataProvider.GetMetadataForProperty(
                        controllerModel.ControllerType,
                        property.PropertyInfo.Name);
                    if (metadata.IsComplexType && !metadata.IsCollectionType)
                    {
                        property.BindingInfo.BinderModelName = string.Empty;
                    }
                }
            }
        }

        internal void InferParameterModelPrefixes(ActionModel action)
        {
            foreach (var parameter in action.Parameters)
            {
                var bindingInfo = parameter.BindingInfo;
                if (bindingInfo?.BindingSource != null &&
                    bindingInfo.BinderModelName == null &&
                    !bindingInfo.BindingSource.IsGreedy &&
                    IsComplexTypeParameter(parameter))
                {
                    parameter.BindingInfo.BinderModelName = string.Empty;
                }
            }
        }

        private bool IsComplexTypeParameter(ParameterModel parameter)
        {
            // No need for information from attributes on the parameter. Just use its type.
            var metadata = _modelMetadataProvider
                .GetMetadataForType(parameter.ParameterInfo.ParameterType);
            return metadata.IsComplexType && !metadata.IsCollectionType;
        }
    }
}
