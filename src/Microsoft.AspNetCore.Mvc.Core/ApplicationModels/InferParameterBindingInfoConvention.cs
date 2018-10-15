// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing.Template;
using Resources = Microsoft.AspNetCore.Mvc.Core.Resources;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// A <see cref="IControllerModelConvention"/> that
    /// <list type="bullet">
    /// <item>infers binding sources for parameters</item>
    /// <item><see cref="BindingInfo.BinderModelName"/> for bound properties and parameters.</item>
    /// </list>
    /// </summary>
    public class InferParameterBindingInfoConvention : IControllerModelConvention
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;

        public InferParameterBindingInfoConvention(
            IModelMetadataProvider modelMetadataProvider)
        {
            _modelMetadataProvider = modelMetadataProvider ?? throw new ArgumentNullException(nameof(modelMetadataProvider));
        }

        /// <summary>
        /// Gets or sets a value that determines if model binding sources are inferred for action parameters on controllers is suppressed.
        /// </summary>
        public bool SuppressInferBindingSourcesForParameters { get; set; }

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

            InferBoundPropertyModelPrefixes(controller);

            foreach (var action in controller.Actions)
            {
                InferParameterBindingSources(action);
                InferParameterModelPrefixes(action);
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

            var bindingSource = IsComplexType(parameter) ?
                BindingSource.Body :
                BindingSource.Query;

            return bindingSource;
        }

        // For any types other than simple types or collections of simple types that are bound from value providers,
        // set the prefix to the empty prefix by default. This makes binding much more predictable.
        // and describable via ApiExplorer
        internal void InferBoundPropertyModelPrefixes(ControllerModel controllerModel)
        {
            foreach (var property in controllerModel.ControllerProperties)
            {
                // IEnumerable<IFormFile> properties are collections of complex types and must be excluded for
                // consistency with parameter handling and to avoid attempting to bind file fields with an empty name.
                var bindingInfo = property.BindingInfo;
                if (bindingInfo?.BindingSource != null &&
                    bindingInfo.BinderModelName == null &&
                    !bindingInfo.BindingSource.IsGreedy &&
                    !IsFormFile(property.ParameterType) &&
                    !IsSimpleTypeOrSimpleCollection(property))
                {
                    property.BindingInfo.BinderModelName = string.Empty;
                }
            }
        }

        internal void InferParameterModelPrefixes(ActionModel action)
        {
            foreach (var parameter in action.Parameters)
            {
                // IEnumerable<IFormFile> parameters are collections of complex types and must be excluded. Otherwise
                // FormFileModelBinder may attempt to bind file fields with an empty name (HTML disallows fields with
                // an empty name). Users have historically applied [FromForm] to such parameters.
                var bindingInfo = parameter.BindingInfo;
                if (bindingInfo?.BindingSource != null &&
                    bindingInfo.BinderModelName == null &&
                    !bindingInfo.BindingSource.IsGreedy &&
                    !IsFormFile(parameter.ParameterType) &&
                    !IsSimpleTypeOrSimpleCollection(parameter))
                {
                    parameter.BindingInfo.BinderModelName = string.Empty;
                }
            }
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

        private bool IsComplexType(ParameterModelBase parameter)
        {
            // No need for information from attributes on the parameter or property. Just use its type.
            var metadata = _modelMetadataProvider.GetMetadataForType(parameter.ParameterType);
            return metadata.IsComplexType;
        }

        private bool IsSimpleTypeOrSimpleCollection(ParameterModelBase parameter)
        {
            // No need for information from attributes on the parameter or property. Just use its type.
            //
            // This check treats IDictionary<TKey, TValue> as complex (because KeyValuePair<,> has no string
            // converter). Also treats nested collections e.g. int[][] as complex.
            var metadata = _modelMetadataProvider.GetMetadataForType(parameter.ParameterType);
            return !metadata.IsComplexType || metadata.ElementMetadata?.IsComplexType == false;
        }

        private static bool IsFormFile(Type parameterType)
        {
            return typeof(IFormFile).IsAssignableFrom(parameterType) ||
                typeof(IEnumerable<IFormFile>).IsAssignableFrom(parameterType);
        }
    }
}
