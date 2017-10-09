// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ApiBehaviorApplicationModelProvider : IApplicationModelProvider
    {
        private readonly ApiBehaviorOptions _apiBehaviorOptions;
        private readonly ModelMetadataProvider _modelMetadataProvider;
        private readonly ModelStateInvalidFilter _modelStateInvalidFilter;
        private readonly ILogger _logger;

        public ApiBehaviorApplicationModelProvider(
            IOptions<ApiBehaviorOptions> apiBehaviorOptions,
            IModelMetadataProvider modelMetadataProvider,
            ILoggerFactory loggerFactory)
        {
            _apiBehaviorOptions = apiBehaviorOptions.Value;
            _modelMetadataProvider = modelMetadataProvider as ModelMetadataProvider;
            _logger = loggerFactory.CreateLogger<ApiBehaviorApplicationModelProvider>();

            if (_apiBehaviorOptions.EnableModelStateInvalidFilter && _apiBehaviorOptions.InvalidModelStateResponseFactory == null)
            {
                throw new ArgumentException(Resources.FormatPropertyOfTypeCannotBeNull(
                    typeof(ApiBehaviorOptions),
                    nameof(ApiBehaviorOptions.InvalidModelStateResponseFactory)));
            }

            _modelStateInvalidFilter = new ModelStateInvalidFilter(
                apiBehaviorOptions.Value,
                loggerFactory.CreateLogger<ModelStateInvalidFilter>());
        }

        /// <remarks>
        /// Order is set to execute after the <see cref="DefaultApplicationModelProvider"/> and allow any other user
        /// <see cref="IApplicationModelProvider"/> that configure routing to execute.
        /// </remarks>
        public int Order => -1000 + 100;

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            foreach (var controllerModel in context.Result.Controllers)
            {
                var isApiController = controllerModel.Attributes.OfType<IApiBehaviorMetadata>().Any();
                var controllerHasSelectorModel = controllerModel.Selectors.Any(s => s.AttributeRouteModel != null);

                foreach (var actionModel in controllerModel.Actions)
                {
                    if (!isApiController && !actionModel.Attributes.OfType<IApiBehaviorMetadata>().Any())
                    {
                        continue;
                    }

                    if (!controllerHasSelectorModel && !actionModel.Selectors.Any(s => s.AttributeRouteModel != null))
                    {
                        // Require attribute routing with controllers annotated with ApiControllerAttribute
                        throw new InvalidOperationException(Resources.FormatApiController_AttributeRouteRequired(nameof(ApiControllerAttribute)));
                    }

                    if (_apiBehaviorOptions.EnableModelStateInvalidFilter)
                    {
                        Debug.Assert(_apiBehaviorOptions.InvalidModelStateResponseFactory != null);
                        actionModel.Filters.Add(_modelStateInvalidFilter);
                    }

                    if (_modelMetadataProvider != null && _apiBehaviorOptions.InferBindingSourcesForParameters)
                    {
                        foreach (var parameter in actionModel.Parameters)
                        {
                            var bindingSource = InferBindingSourceForParameter(parameter);
                            if (bindingSource != null)
                            {
                                _logger.InferringParameterBindingSource(parameter, bindingSource);
                                parameter.BindingInfo = new BindingInfo
                                {
                                    BindingSource = bindingSource,
                                };
                            }
                        }
                    }
                }
            }
        }

        // Internal for unit testing.
        internal BindingSource InferBindingSourceForParameter(ParameterModel parameter)
        {
            if (parameter.BindingInfo != null)
            {
                return null;
            }

            if (ParameterExistsInAllRoutes(parameter.Action, parameter.ParameterName))
            {
                return BindingSource.Path;
            }
            else
            {
                var parameterMetadata = _modelMetadataProvider.GetMetadataForParameter(parameter.ParameterInfo);
                if (parameterMetadata != null)
                {
                    var bindingSource = parameterMetadata.IsComplexType ?
                        BindingSource.Body :
                        BindingSource.Query;

                    return bindingSource;
                }
            }

            return null;
        }

        private bool ParameterExistsInAllRoutes(ActionModel actionModel, string parameterName)
        {
            var parameterExistsInSomeRoute = false;
            foreach (var routeTemplate in GetAllRouteTemplates())
            {
                var parsedTemplate = TemplateParser.Parse(routeTemplate);
                if (parsedTemplate.GetParameter(parameterName) == null)
                {
                    return false;
                }

                // Ensure at least one route exists.
                parameterExistsInSomeRoute = true;
            }

            return parameterExistsInSomeRoute;

            IEnumerable<string> GetAllRouteTemplates()
            {
                foreach (var actionSelectorModel in actionModel.Selectors)
                {
                    var actionRouteModel = actionSelectorModel.AttributeRouteModel;
                    var actionRouteTemplate = actionRouteModel?.Template;
                    if (actionRouteModel != null && actionRouteModel.IsAbsoluteTemplate)
                    {
                        yield return AttributeRouteModel.CombineTemplates(
                            prefix: null,
                            template: actionRouteTemplate);
                    }
                    else if (actionModel.Controller.Selectors.Count > 0)
                    {
                        foreach (var controllerTemplate in actionModel.Controller.Selectors)
                        {
                            yield return AttributeRouteModel.CombineTemplates(
                                controllerTemplate.AttributeRouteModel?.Template,
                                actionRouteTemplate);
                        }
                    }
                    else
                    {
                        yield return AttributeRouteModel.CombineTemplates(
                            prefix: null,
                            template: actionRouteTemplate);
                    }
                }
            }
        }
    }
}
