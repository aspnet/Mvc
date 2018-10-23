// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Internal;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.Components
{
    public class ComponentActionDescriptorProvider : IActionDescriptorProvider
    {
        private readonly ApplicationPartManager _partManager;
        private readonly IPageApplicationModelProvider[] _applicationModelProviders;
        private readonly PageConventionCollection _conventions;
        private readonly FilterCollection _globalFilters;

        public ComponentActionDescriptorProvider(
            ApplicationPartManager partManager,
            IEnumerable<IPageApplicationModelProvider> applicationModelProviders,
            IOptions<RazorPagesOptions> pageOptions,
            IOptions<MvcOptions> mvcOptions)
        {
            _partManager = partManager;
            _applicationModelProviders = applicationModelProviders.OrderBy(p => p.Order).ToArray();
            _conventions = pageOptions.Value.Conventions;
            _globalFilters = mvcOptions.Value.Filters;
        }

        public int Order => -1000;

        public void OnProvidersExecuting(ActionDescriptorProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var feature = new ComponentFeature();
            _partManager.PopulateFeature(feature);

            foreach (var component in feature.Components)
            {
                context.Results.Add(CreateActionDescriptor(component));
            }
        }

        public void OnProvidersExecuted(ActionDescriptorProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
        }

        private CompiledPageActionDescriptor CreateActionDescriptor(ComponentInfo component)
        {
            var @base = new PageActionDescriptor()
            {
                AttributeRouteInfo = new AttributeRouteInfo()
                {
                    Template = component.RouteTemplate,
                },
                RouteValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "page", component.RouteTemplate },
                },
                RelativePath = component.RouteTemplate,
                ViewEnginePath = component.RouteTemplate,
            };

            var modelProperty = component.ComponentType.GetProperty("Model", BindingFlags.Instance | BindingFlags.NonPublic);
            var type = modelProperty == null
                ? typeof(PageAdapter<>).MakeGenericType(component.ComponentType)
                : typeof(PageAdapter<,>).MakeGenericType(component.ComponentType, modelProperty.PropertyType);

            var context = new PageApplicationModelProviderContext(@base, type.GetTypeInfo());
            for (var i = 0; i < _applicationModelProviders.Length; i++)
            {
                _applicationModelProviders[i].OnProvidersExecuting(context);
            }

            for (var i = _applicationModelProviders.Length - 1; i >= 0; i--)
            {
                _applicationModelProviders[i].OnProvidersExecuted(context);
            }

            ApplyConventions(_conventions, context.PageApplicationModel);

            return CompiledPageActionDescriptorBuilder.Build(context.PageApplicationModel, _globalFilters);
        }

        internal static void ApplyConventions(
            PageConventionCollection conventions,
            PageApplicationModel pageApplicationModel)
        {
            var applicationModelConventions = GetConventions<IPageApplicationModelConvention>(pageApplicationModel.HandlerTypeAttributes);
            foreach (var convention in applicationModelConventions)
            {
                convention.Apply(pageApplicationModel);
            }

            var handlers = pageApplicationModel.HandlerMethods.ToArray();
            foreach (var handlerModel in handlers)
            {
                var handlerModelConventions = GetConventions<IPageHandlerModelConvention>(handlerModel.Attributes);
                foreach (var convention in handlerModelConventions)
                {
                    convention.Apply(handlerModel);
                }

                var parameterModels = handlerModel.Parameters.ToArray();
                foreach (var parameterModel in parameterModels)
                {
                    var parameterModelConventions = GetConventions<IParameterModelBaseConvention>(parameterModel.Attributes);
                    foreach (var convention in parameterModelConventions)
                    {
                        convention.Apply(parameterModel);
                    }
                }
            }

            var properties = pageApplicationModel.HandlerProperties.ToArray();
            foreach (var propertyModel in properties)
            {
                var propertyModelConventions = GetConventions<IParameterModelBaseConvention>(propertyModel.Attributes);
                foreach (var convention in propertyModelConventions)
                {
                    convention.Apply(propertyModel);
                }
            }

            IEnumerable<TConvention> GetConventions<TConvention>(
                IReadOnlyList<object> attributes)
            {
                return Enumerable.Concat(
                    conventions.OfType<TConvention>(),
                    attributes.OfType<TConvention>());
            }
        }
    }
}
