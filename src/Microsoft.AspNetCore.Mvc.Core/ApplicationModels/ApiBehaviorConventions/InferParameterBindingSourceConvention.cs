// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;
using Resources = Microsoft.AspNetCore.Mvc.Core.Resources;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// A <see cref="IActionModelConvention"/> that infers binding sources for parameters.
    /// </summary>
    public class InferParameterBindingSourceConvention : IActionModelConvention
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ApiBehaviorOptions _apiBehaviorOptions;

        public InferParameterBindingSourceConvention(
            IOptions<ApiBehaviorOptions> apiBehaviorOptions,
            IModelMetadataProvider modelMetadataProvider)
        {
            _apiBehaviorOptions = apiBehaviorOptions?.Value ?? throw new ArgumentNullException(nameof(apiBehaviorOptions));
            _modelMetadataProvider = modelMetadataProvider ?? throw new ArgumentNullException(nameof(modelMetadataProvider));
        }

        protected virtual bool ShouldApply(ActionModel action) =>
            !_apiBehaviorOptions.SuppressInferBindingSourcesForParameters;

        public void Apply(ActionModel action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (ShouldApply(action))
            {
                InferParameterBindingSources(action);
            }
        }

        // For any complex types that are bound from value providers, set the prefix
        // to the empty prefix by default. This makes binding much more predictable
        // and describable via ApiExplorer

        // internal for testing
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

        internal void InferParameterBindingSources(ActionModel actionModel)
        {
            var inferredBindingSources = new BindingSource[actionModel.Parameters.Count];

            for (var i = 0; i < actionModel.Parameters.Count; i++)
            {
                var parameter = actionModel.Parameters[i];
                var bindingSource = parameter.BindingInfo?.BindingSource;
                if (bindingSource == null)
                {
                    bindingSource = InferBindingSourceForParameter(parameter);

                    parameter.BindingInfo = parameter.BindingInfo ?? new BindingInfo();
                    parameter.BindingInfo.BindingSource = bindingSource;
                }
            }

            var fromBodyParameters = actionModel.Parameters.Where(p => p.BindingInfo.BindingSource == BindingSource.Body).ToList();
            if (fromBodyParameters.Count > 1)
            {
                var parameters = string.Join(Environment.NewLine, fromBodyParameters.Select(p => p.DisplayName));
                var message = Resources.FormatApiController_MultipleBodyParametersFound(
                    actionModel.DisplayName,
                    nameof(FromQueryAttribute),
                    nameof(FromRouteAttribute),
                    nameof(FromBodyAttribute));

                message += Environment.NewLine + parameters;
                throw new InvalidOperationException(message);
            }
        }

        // Internal for unit testing.
        internal BindingSource InferBindingSourceForParameter(ParameterModel parameter)
        {
            if (ParameterExistsInAnyRoute(parameter.Action, parameter.ParameterName))
            {
                return BindingSource.Path;
            }

            var bindingSource = IsComplexTypeParameter(parameter) ?
                BindingSource.Body :
                BindingSource.Query;

            return bindingSource;
        }

        private bool ParameterExistsInAnyRoute(ActionModel actionModel, string parameterName)
        {
            foreach (var (route, _, _) in ActionAttributeRouteModel.GetAttributeRoutes(actionModel))
            {
                if (route == null)
                {
                    continue;
                }

                var parsedTemplate = TemplateParser.Parse(route.Template);
                if (parsedTemplate.GetParameter(parameterName) != null)
                {
                    return true;
                }
            }

            return false;
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
