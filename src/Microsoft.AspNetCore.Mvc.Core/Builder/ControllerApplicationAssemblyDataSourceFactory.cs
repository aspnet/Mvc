// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Routing
{
    internal class ControllerApplicationAssemblyDataSourceFactory : IApplicationAssemblyDataSourceFactory
    {
        private readonly IApplicationModelProvider[] _applicationModelProviders;
        private readonly IEnumerable<IApplicationModelConvention> _conventions;

        public ControllerApplicationAssemblyDataSourceFactory(
            IEnumerable<IApplicationModelProvider> applicationModelProviders,
            IOptions<MvcOptions> optionsAccessor)
        {
            if (applicationModelProviders == null)
            {
                throw new ArgumentNullException(nameof(applicationModelProviders));
            }

            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            _applicationModelProviders = applicationModelProviders.OrderBy(p => p.Order).ToArray();
            _conventions = optionsAccessor.Value.Conventions;
        }

        public EndpointDataSource GetOrCreateDataSource(ICollection<EndpointDataSource> dataSources, Assembly assembly)
        {
            if (dataSources == null)
            {
                throw new ArgumentNullException(nameof(dataSources));
            }

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var dataSource = GetDataSource(dataSources, assembly);
            if (dataSource == null)
            {
                // OK this is a new assembly, so lets create a data source.
                var controllerTypes = assembly.DefinedTypes.Where(t => ControllerDiscovery.IsControllerType(t));

                var context = new ApplicationModelProviderContext(controllerTypes);

                for (var i = 0; i < _applicationModelProviders.Length; i++)
                {
                    _applicationModelProviders[i].OnProvidersExecuting(context);
                }

                for (var i = _applicationModelProviders.Length - 1; i >= 0; i--)
                {
                    _applicationModelProviders[i].OnProvidersExecuted(context);
                }

                ApplicationModelConventions.ApplyConventions(context.Result, _conventions);
                var (attributeRouteModels, conventionalRouteModels) = Flatten(context.Result);

                dataSource = new ControllerEndpointDataSource(assembly, attributeRouteModels, conventionalRouteModels);
                dataSources.Add(dataSource);
            }
            
            return dataSource;
        }

        private EndpointDataSource GetDataSource(ICollection<EndpointDataSource> dataSources, Assembly assembly)
        {
            foreach (var dataSource in dataSources)
            {
                if (dataSource is ControllerEndpointDataSource controllerDataSource && controllerDataSource.Assembly == assembly)
                {
                    return controllerDataSource;
                }
            }

            return null;
        }

        private (List<ControllerActionEndpointModel> attributeRouteModels, List<ControllerActionEndpointModel> conventionalRouteModels) Flatten(ApplicationModel application)
        {
            var attributeRouteModels = new List<ControllerActionEndpointModel>();
            var conventionalRouteModels = new List<ControllerActionEndpointModel>();

            foreach (var controller in application.Controllers)
            {
                // Only add properties which are explicitly marked to bind.
                // The attribute check is required for ModelBinder attribute.
                var controllerProperties = controller.ControllerProperties
                    .Where(p => p.BindingInfo != null)
                    .ToList();

                foreach (var action in controller.Actions)
                {
                    // A controller-level selector model and action-level selector model can cross-product.
                    foreach (var group in ActionAttributeRouteModel.GetAttributeRoutes(action))
                    {
                        var model = new ControllerActionEndpointModel(
                            controller.ControllerType, 
                            action.ActionMethod,
                            controller.ControllerName,
                            action.ActionName);

                        model.DisplayName = action.DisplayName;

                        model.RequestDelegate = (context) =>
                        {
                            var routeData = context.GetRouteData();
                            var actionDescriptor = context.GetEndpoint().Metadata.GetMetadata<ActionDescriptor>();
                            var actionContext = new ActionContext(context, routeData, actionDescriptor);

                            var invokerFactory = context.RequestServices.GetRequiredService<IActionInvokerFactory>();
                            var invoker = invokerFactory.CreateInvoker(actionContext);
                            return invoker.InvokeAsync();
                        };

                        foreach (var parameter in action.Parameters)
                        {
                            model.Parameters.Add(new ControllerActionParameterModel(parameter.ParameterInfo, parameter.ParameterName, parameter.BindingInfo));
                        }

                        foreach (var property in controller.ControllerProperties)
                        {
                            if (property.BindingInfo != null)
                            {
                                model.Parameters.Add(new ControllerActionParameterModel(property.PropertyInfo, property.Name, property.BindingInfo));
                            }
                        }

                        AddActionFilters(model, action.Filters, controller.Filters, application.Filters);
                        AddActionConstraints(model, group.actionSelector, group.controllerSelector);

                        foreach (var metadata in group.controllerSelector?.EndpointMetadata ?? Array.Empty<object>())
                        {
                            model.EndpointMetadata.Add(metadata);
                        }

                        foreach (var metadata in group.actionSelector.EndpointMetadata)
                        {
                            model.EndpointMetadata.Add(metadata);
                        }

                        AddApiExplorerInfo(model, application, controller, action);
                        AddRouteValues(model, controller, action);
                        AddProperties(model, action, controller, application);

                        if (group.route?.Template == null)
                        {
                            conventionalRouteModels.Add(model);
                        }
                        else
                        {
                            // TODO: handle errors
                            //
                            // This needs to be done after AddRouteValues 
                            ReplaceAttributeRouteTokens(model, group.route.Template, new List<string>());
                            attributeRouteModels.Add(model);
                        }
                    }
                }
            }

            return (attributeRouteModels, conventionalRouteModels);
        }

        private static void AddApiExplorerInfo(
            ControllerActionEndpointModel model,
            ApplicationModel application,
            ControllerModel controller,
            ActionModel action)
        {
            var isVisible =
                action.ApiExplorer?.IsVisible ??
                controller.ApiExplorer?.IsVisible ??
                application.ApiExplorer?.IsVisible ??
                false;

            if (isVisible && IsAttributeRouted(model))
            {
                var apiExplorerActionData = new ApiDescriptionActionData()
                {
                    GroupName = action.ApiExplorer?.GroupName ?? controller.ApiExplorer?.GroupName,
                };

                model.Properties[typeof(ApiDescriptionActionData)] = apiExplorerActionData;
            }
        }

        private static void AddProperties(
            ControllerActionEndpointModel model,
            ActionModel action,
            ControllerModel controller,
            ApplicationModel application)
        {
            foreach (var item in application.Properties)
            {
                model.Properties[item.Key] = item.Value;
            }

            foreach (var item in controller.Properties)
            {
                model.Properties[item.Key] = item.Value;
            }

            foreach (var item in action.Properties)
            {
                model.Properties[item.Key] = item.Value;
            }
        }

        private static void AddActionFilters(
            ControllerActionEndpointModel model,
            IEnumerable<IFilterMetadata> actionFilters,
            IEnumerable<IFilterMetadata> controllerFilters,
            IEnumerable<IFilterMetadata> globalFilters)
        {
            foreach (var filter in globalFilters)
            {
                model.Filters.Add(new FilterDescriptor(filter, FilterScope.Global));
            }

            foreach (var filter in controllerFilters)
            {
                model.Filters.Add(new FilterDescriptor(filter, FilterScope.Controller));
            }

            foreach (var filter in actionFilters)
            {
                model.Filters.Add(new FilterDescriptor(filter, FilterScope.Action));
            }
        }

        private static AttributeRouteInfo CreateAttributeRouteInfo(AttributeRouteModel routeModel)
        {
            if (routeModel == null)
            {
                return null;
            }

            return new AttributeRouteInfo
            {
                Template = routeModel.Template,
                Order = routeModel.Order ?? 0,
                Name = routeModel.Name,
                SuppressLinkGeneration = routeModel.SuppressLinkGeneration,
                SuppressPathMatching = routeModel.SuppressPathMatching,
            };
        }

        private static void AddActionConstraints(
            ControllerActionEndpointModel actionDescriptor,
            SelectorModel actionSelector,
            SelectorModel controllerSelector)
        {
            var controllerConstraints = controllerSelector?.ActionConstraints.Where(constraint => !(constraint is IRouteTemplateProvider));
            if (controllerSelector?.AttributeRouteModel?.Attribute is IActionConstraintMetadata actionConstraint)
            {
                // Use the attribute route as a constraint if the controller selector participated in creating this route.
                controllerConstraints = controllerConstraints.Concat(new[] { actionConstraint });
            }

            var constraints = new List<IActionConstraintMetadata>();

            if (actionSelector.ActionConstraints != null)
            {
                constraints.AddRange(actionSelector.ActionConstraints);
            }

            if (controllerConstraints != null)
            {
                constraints.AddRange(controllerConstraints);
            }

            if (constraints.Count > 0)
            {
                // TODO something
                //actionDescriptor.ActionConstraints = constraints;
            }
        }

        public static void AddRouteValues(
            ControllerActionEndpointModel model,
            ControllerModel controller,
            ActionModel action)
        {
            // Apply all the constraints defined on the action, then controller (for example, [Area])
            // to the actions. Also keep track of all the constraints that require preventing actions
            // without the constraint to match. For example, actions without an [Area] attribute on their
            // controller should not match when a value has been given for area when matching a url or
            // generating a link.
            foreach (var kvp in action.RouteValues)
            {
                // Skip duplicates
                if (!model.RequiredValues.ContainsKey(kvp.Key))
                {
                    model.RequiredValues.Add(kvp.Key, kvp.Value);
                }
            }

            foreach (var kvp in controller.RouteValues)
            {
                // Skip duplicates - this also means that a value on the action will take precedence
                if (!model.RequiredValues.ContainsKey(kvp.Key))
                {
                    model.RequiredValues.Add(kvp.Key, kvp.Value);
                }
            }
        }

        private static void ReplaceAttributeRouteTokens(
            ControllerActionEndpointModel model,
            string routeTemplate,
            IList<string> routeTemplateErrors)
        {
            try
            {
                var routeValues = new Dictionary<string, string>(model.RequiredValues, StringComparer.OrdinalIgnoreCase);
                if (!routeValues.ContainsKey("action"))
                {
                    routeValues.Add("action", model.ActionName);
                }
                if (!routeValues.ContainsKey("controller"))
                {
                    routeValues.Add("controller", model.ControllerName);
                }

                model.Properties.TryGetValue(typeof(IOutboundParameterTransformer), out var transformer);
                var routeTokenTransformer = transformer as IOutboundParameterTransformer;

                routeTemplate = AttributeRouteModel.ReplaceTokens(
                    routeTemplate,
                    routeValues,
                    routeTokenTransformer);

                if (model.RouteName != null)
                {
                    model.RouteName = AttributeRouteModel.ReplaceTokens(
                        model.RouteName,
                        routeValues,
                        routeTokenTransformer);
                }

                model.RoutePattern = RoutePatternFactory.Parse(routeTemplate, routeValues, parameterPolicies: null);
            }
            catch (InvalidOperationException ex)
            {
                // Routing will throw an InvalidOperationException here if we can't parse/replace tokens
                // in the template.
                var message = Mvc.Core.Resources.FormatAttributeRoute_IndividualErrorMessage(
                    model.DisplayName,
                    Environment.NewLine,
                    ex.Message);

                routeTemplateErrors.Add(message);
            }
        }

        private static bool IsAttributeRouted(ControllerActionEndpointModel model)
        {
            return model.RoutePattern != null;
        }
    }
}
