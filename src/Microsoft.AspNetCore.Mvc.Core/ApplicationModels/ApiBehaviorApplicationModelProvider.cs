﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    internal class ApiBehaviorApplicationModelProvider : IApplicationModelProvider
    {
        public ApiBehaviorApplicationModelProvider(
            IOptions<ApiBehaviorOptions> apiBehaviorOptions,
            IModelMetadataProvider modelMetadataProvider,
            IClientErrorFactory clientErrorFactory,
            ILoggerFactory loggerFactory)
        {
            var options = apiBehaviorOptions.Value;

            ActionModelConventions = new List<IActionModelConvention>()
            {
                new ApiVisibilityConvention(),
            };

            if (!options.SuppressMapClientErrors)
            {
                ActionModelConventions.Add(new ClientErrorResultFilterConvention());
            }

            if (!options.SuppressModelStateInvalidFilter)
            {
                ActionModelConventions.Add(new InvalidModelStateFilterConvention());
            }

            if (!options.SuppressConsumesConstraintForFormFileParameters)
            {
                ActionModelConventions.Add(new ConsumesConstraintForFormFileParameterConvention());
            }

            var defaultErrorType = options.SuppressMapClientErrors ? typeof(void) : typeof(ProblemDetails);
            var defaultErrorTypeAttribute = new ProducesErrorResponseTypeAttribute(defaultErrorType);
            ActionModelConventions.Add(new ApiConventionApplicationModelConvention(defaultErrorTypeAttribute));

            if (!options.SuppressInferBindingSourcesForParameters)
            {
                var convention = new InferParameterBindingInfoConvention(modelMetadataProvider)
                {
                    AllowInferringBindingSourceForCollectionTypesAsFromQuery = options.AllowInferringBindingSourceForCollectionTypesAsFromQuery,
                };

                ActionModelConventions.Add(convention);
            }
        }

        /// <remarks>
        /// Order is set to execute after the <see cref="DefaultApplicationModelProvider"/> and allow any other user
        /// <see cref="IApplicationModelProvider"/> that configure routing to execute.
        /// </remarks>
        public int Order => -1000 + 100;

        public List<IActionModelConvention> ActionModelConventions { get; }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            foreach (var controller in context.Result.Controllers)
            {
                if (!IsApiController(controller))
                {
                    continue;
                }

                foreach (var action in controller.Actions)
                {
                    // Ensure ApiController is set up correctly
                    EnsureActionIsAttributeRouted(action);

                    foreach (var convention in ActionModelConventions)
                    {
                        convention.Apply(action);
                    }
                }
            }
        }

        private static void EnsureActionIsAttributeRouted(ActionModel actionModel)
        {
            if (!IsAttributeRouted(actionModel.Controller.Selectors) &&
                !IsAttributeRouted(actionModel.Selectors))
            {
                // Require attribute routing with controllers annotated with ApiControllerAttribute
                var message = Resources.FormatApiController_AttributeRouteRequired(
                     actionModel.DisplayName,
                    nameof(ApiControllerAttribute));
                throw new InvalidOperationException(message);
            }

            bool IsAttributeRouted(IList<SelectorModel> selectorModel)
            {
                for (var i = 0; i < selectorModel.Count; i++)
                {
                    if (selectorModel[i].AttributeRouteModel != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private static bool IsApiController(ControllerModel controller)
        {
            if (controller.Attributes.OfType<IApiBehaviorMetadata>().Any())
            {
                return true;
            }

            var controllerAssembly = controller.ControllerType.Assembly;
            var assemblyAttributes = controllerAssembly.GetCustomAttributes();
            return assemblyAttributes.OfType<IApiBehaviorMetadata>().Any();
        }
    }
}
