// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ApiBehaviorApplicationModelProvider : IApplicationModelProvider
    {
        private readonly IControllerModelConvention[] _controllerModelConventions;
        private readonly IActionModelConvention[] _actionModelConventions;

        public ApiBehaviorApplicationModelProvider(
            IOptions<ApiBehaviorOptions> apiBehaviorOptions,
            IModelMetadataProvider modelMetadataProvider,
            IClientErrorFactory clientErrorFactory,
            ILoggerFactory loggerFactory)
        {
            _controllerModelConventions = new IControllerModelConvention[]
            {
                new ApiVisibilityConvention(),
                new InferModelPrefixConvention(apiBehaviorOptions, modelMetadataProvider),
            };

            _actionModelConventions = new IActionModelConvention[]
            {
                new ClientErrorResultFilterConvention(apiBehaviorOptions, clientErrorFactory, loggerFactory),
                new InvalidModelStateFilterConvention(apiBehaviorOptions, loggerFactory),
                new ConsumesConstraintForFormFileParameterConvention(apiBehaviorOptions),
                new DiscoverApiConventionResultConvention(),
                new DiscoverProducesErrorResponseTypeConvention(apiBehaviorOptions),
                new InferParameterBindingSourceConvention(apiBehaviorOptions, modelMetadataProvider),
            };
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
                if (!controllerModel.Attributes.OfType<IApiBehaviorMetadata>().Any())
                {
                    continue;
                }

                foreach (var actionModel in controllerModel.Actions)
                {
                    // Ensure ApiController is set up correctly
                    EnsureActionIsAttributeRouted(actionModel);

                    foreach (var convention in _actionModelConventions)
                    {
                        convention.Apply(actionModel);
                    }
                }

                foreach (var convention in _controllerModelConventions)
                {
                    convention.Apply(controllerModel);
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
    }
}
