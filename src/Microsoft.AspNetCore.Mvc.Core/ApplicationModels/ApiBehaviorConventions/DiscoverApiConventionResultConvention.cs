// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// An <see cref="IActionModelConvention"/> that discovers <see cref="ApiConventionResult"/>
    /// from applied <see cref="ApiConventionTypeAttribute"/> or <see cref="ApiConventionMethodAttribute"/>.
    /// </summary>
    public class DiscoverApiConventionResultConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!ShouldApply(action))
            {
                return;
            }

            DiscoverApiConvention(action);            
        }

        protected virtual bool ShouldApply(ActionModel controller) => true;

        private static void DiscoverApiConvention(ActionModel action)
        {
            if (action.Filters.OfType<IApiResponseMetadataProvider>().Any())
            {
                // If an action already has providers, don't discover any from conventions.
                return;
            }

            var controller = action.Controller;
            var apiConventionAttributes = controller.Attributes.OfType<ApiConventionTypeAttribute>().ToArray();
            if (apiConventionAttributes.Length == 0)
            {
                var controllerAssembly = controller.ControllerType.Assembly;
                apiConventionAttributes = controllerAssembly.GetCustomAttributes<ApiConventionTypeAttribute>().ToArray();
            }

            if (ApiConventionResult.TryGetApiConvention(action.ActionMethod, apiConventionAttributes, out var result))
            {
                action.Properties[typeof(ApiConventionResult)] = result;
            }
        }
    }
}
