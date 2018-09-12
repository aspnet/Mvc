﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// An <see cref="IActionModelConvention"/> that discovers
    /// <list type="bullet">
    /// <item><see cref="ApiConventionResult"/> from applied <see cref="ApiConventionTypeAttribute"/> or <see cref="ApiConventionMethodAttribute"/>.</item>
    /// <item><see cref="ProducesErrorResponseTypeAttribute"/> that applies to the action.</item>
    /// </list>
    /// </summary>
    public class ApiConventionApplicationModelConvention : IActionModelConvention
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ApiConventionApplicationModelConvention"/>.
        /// </summary>
        /// <param name="defaultErrorResponseType">The error type to be used. Use <see cref="void" />
        /// when no default error type is to be inferred.
        /// </param>
        public ApiConventionApplicationModelConvention(ProducesErrorResponseTypeAttribute defaultErrorResponseType)
        {
            DefaultErrorResponseType = defaultErrorResponseType ?? throw new ArgumentNullException(nameof(defaultErrorResponseType));
        }

        /// <summary>
        /// Gets the default <see cref="ProducesErrorResponseTypeAttribute"/> that is associated with an action
        /// when no attribute is discovered.
        /// </summary>
        public ProducesErrorResponseTypeAttribute DefaultErrorResponseType { get; }

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
            DiscoverErrorResponseType(action);
        }

        protected virtual bool ShouldApply(ActionModel action) => true;

        private static void DiscoverApiConvention(ActionModel action)
        {
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

        private void DiscoverErrorResponseType(ActionModel action)
        {
            var errorTypeAttribute =
                action.Attributes.OfType<ProducesErrorResponseTypeAttribute>().FirstOrDefault() ??
                action.Controller.Attributes.OfType<ProducesErrorResponseTypeAttribute>().FirstOrDefault() ??
                action.Controller.ControllerType.Assembly.GetCustomAttribute<ProducesErrorResponseTypeAttribute>() ??
                DefaultErrorResponseType;

            action.Properties[typeof(ProducesErrorResponseTypeAttribute)] = errorTypeAttribute;
        }
    }
}
