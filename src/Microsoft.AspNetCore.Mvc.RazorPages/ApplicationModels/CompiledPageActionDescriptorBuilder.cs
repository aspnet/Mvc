﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    /// <summary>
    /// Constructs a <see cref="CompiledPageActionDescriptor"/> from an <see cref="PageApplicationModel"/>.
    /// </summary>
    internal static class CompiledPageActionDescriptorBuilder
    {
        /// <summary>
        /// Creates a <see cref="CompiledPageActionDescriptor"/> from the specified <paramref name="applicationModel"/>.
        /// </summary>
        /// <param name="applicationModel">The <see cref="PageApplicationModel"/>.</param>
        /// <param name="globalFilters">Global filters to apply to the page.</param>
        /// <returns>The <see cref="CompiledPageActionDescriptor"/>.</returns>
        public static CompiledPageActionDescriptor Build(
            PageApplicationModel applicationModel,
            FilterCollection globalFilters)
        {
            var boundProperties = CreateBoundProperties(applicationModel);
            var filters = Enumerable.Concat(
                    globalFilters.Select(f => new FilterDescriptor(f, FilterScope.Global)),
                    applicationModel.Filters.Select(f => new FilterDescriptor(f, FilterScope.Action)))
                .ToArray();
            var handlerMethods = CreateHandlerMethods(applicationModel);

            if (applicationModel.ModelType != null && applicationModel.DeclaredModelType != null &&
                !applicationModel.DeclaredModelType.IsAssignableFrom(applicationModel.ModelType))
            {
                var message = Resources.FormatInvalidActionDescriptorModelType(
                    applicationModel.ActionDescriptor.DisplayName,
                    applicationModel.ModelType.Name,
                    applicationModel.DeclaredModelType.Name);

                throw new InvalidOperationException(message);
            }

            var actionDescriptor = applicationModel.ActionDescriptor;
            return new CompiledPageActionDescriptor(actionDescriptor)
            {
                ActionConstraints = actionDescriptor.ActionConstraints,
                AttributeRouteInfo = actionDescriptor.AttributeRouteInfo,
                BoundProperties = boundProperties,
                EndpointMetadata = actionDescriptor.EndpointMetadata,
                FilterDescriptors = filters,
                HandlerMethods = handlerMethods,
                HandlerTypeInfo = applicationModel.HandlerType,
                DeclaredModelTypeInfo = applicationModel.DeclaredModelType,
                ModelTypeInfo = applicationModel.ModelType,
                RouteValues = actionDescriptor.RouteValues,
                PageTypeInfo = applicationModel.PageType,
                Properties = applicationModel.Properties,
            };
        }

        // Internal for unit testing
        internal static HandlerMethodDescriptor[] CreateHandlerMethods(PageApplicationModel applicationModel)
        {
            var handlerModels = applicationModel.HandlerMethods;
            var handlerDescriptors = new HandlerMethodDescriptor[handlerModels.Count];

            for (var i = 0; i < handlerDescriptors.Length; i++)
            {
                var handlerModel = handlerModels[i];

                handlerDescriptors[i] = new HandlerMethodDescriptor
                {
                    HttpMethod = handlerModel.HttpMethod,
                    Name = handlerModel.HandlerName,
                    MethodInfo = handlerModel.MethodInfo,
                    Parameters = CreateHandlerParameters(handlerModel),
                };
            }

            return handlerDescriptors;
        }

        // internal for unit testing
        internal static HandlerParameterDescriptor[] CreateHandlerParameters(PageHandlerModel handlerModel)
        {
            var methodParameters = handlerModel.Parameters;
            var parameters = new HandlerParameterDescriptor[methodParameters.Count];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameterModel = methodParameters[i];

                parameters[i] = new HandlerParameterDescriptor
                {
                    BindingInfo = parameterModel.BindingInfo,
                    Name = parameterModel.ParameterName,
                    ParameterInfo = parameterModel.ParameterInfo,
                    ParameterType = parameterModel.ParameterInfo.ParameterType,
                };
            }

            return parameters;
        }

        // internal for unit testing
        internal static PageBoundPropertyDescriptor[] CreateBoundProperties(PageApplicationModel applicationModel)
        {
            var results = new List<PageBoundPropertyDescriptor>();
            var properties = applicationModel.HandlerProperties;
            for (var i = 0; i < properties.Count; i++)
            {
                var propertyModel = properties[i];

                // Only add properties which are explicitly marked to bind.
                if (propertyModel.BindingInfo == null)
                {
                    continue;
                }

                var descriptor = new PageBoundPropertyDescriptor
                {
                    Property = propertyModel.PropertyInfo,
                    Name = propertyModel.PropertyName,
                    BindingInfo = propertyModel.BindingInfo,
                    ParameterType = propertyModel.PropertyInfo.PropertyType,
                };

                results.Add(descriptor);
            }

            return results.ToArray();
        }
    }
}