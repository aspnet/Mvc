// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing.Template;
using Resources = Microsoft.AspNetCore.Mvc.Core.Resources;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// A <see cref="IControllerModelConvention"/> that infers binding sources for parameters.
    /// </summary>
    public class InferParameterBindingSourceConvention : IControllerModelConvention
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public InferParameterBindingSourceConvention(
            IModelMetadataProvider modelMetadataProvider)
        {
            _modelMetadataProvider = modelMetadataProvider ?? throw new ArgumentNullException(nameof(modelMetadataProvider));
        }

        protected virtual bool ShouldApply(ControllerModel controller) => true;

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

            foreach (var action in controller.Actions)
            {
                InferParameterBindingSources(action);
            }
        }

        internal void InferParameterBindingSources(ActionModel action)
        {
            var inferredBindingSources = new BindingSource[action.Parameters.Count];

            for (var i = 0; i < action.Parameters.Count; i++)
            {
                var parameter = action.Parameters[i];
                var bindingSource = parameter.BindingInfo?.BindingSource;
                if (bindingSource == null)
                {
                    bindingSource = InferBindingSourceForParameter(parameter);

                    parameter.BindingInfo = parameter.BindingInfo ?? new BindingInfo();
                    parameter.BindingInfo.BindingSource = bindingSource;
                }
            }

            var fromBodyParameters = action.Parameters.Where(p => p.BindingInfo.BindingSource == BindingSource.Body).ToList();
            if (fromBodyParameters.Count > 1)
            {
                var parameters = string.Join(Environment.NewLine, fromBodyParameters.Select(p => p.DisplayName));
                var message = Resources.FormatApiController_MultipleBodyParametersFound(
                    action.DisplayName,
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

        private bool ParameterExistsInAnyRoute(ActionModel action, string parameterName)
        {
            foreach (var (route, _, _) in ActionAttributeRouteModel.GetAttributeRoutes(action))
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
