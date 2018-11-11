// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Microsoft.AspNetCore.Routing
{
    public sealed class ControllerActionEndpointModel : EndpointModel
    {
        public ControllerActionEndpointModel(Type controllerType, MethodInfo actionMethod, string controllerName, string actionName)
        {
            if (controllerType == null)
            {
                throw new ArgumentNullException(nameof(controllerType));
            }

            if (actionMethod == null)
            {
                throw new ArgumentNullException(nameof(actionMethod));
            }

            if (controllerName == null)
            {
                throw new ArgumentNullException(nameof(controllerName));
            }

            if (actionName == null)
            {
                throw new ArgumentNullException(nameof(actionName));
            }

            ControllerType = controllerType;
            ActionMethod = actionMethod;
            ControllerName = controllerName;
            ActionName = actionName;

            EndpointMetadata = new List<object>();
            Filters = new List<FilterDescriptor>();
            Parameters = new List<ControllerActionParameterModel>();
            Properties = new Dictionary<object, object>();
            RequiredValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public ControllerActionEndpointModel(ControllerActionEndpointModel other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            ActionName = other.ActionName;
            ActionMethod = other.ActionMethod;
            ControllerName = other.ControllerName;
            ControllerType = other.ControllerType;
            DisplayName = other.DisplayName;
            Order = other.Order;
            RequestDelegate = other.RequestDelegate;
            RouteName = other.RouteName;
            RoutePattern = other.RoutePattern;

            // Other models, DEEP COPY
            Parameters = new List<ControllerActionParameterModel>(other.Parameters.Count);
            foreach (var parameter in other.Parameters)
            {
                Parameters.Add(new ControllerActionParameterModel(parameter));
            }

            // SHALLOW COPY
            EndpointMetadata = new List<object>(other.EndpointMetadata);
            Filters = new List<FilterDescriptor>(other.Filters);
            Properties = new Dictionary<object, object>(other.Properties);
            RequiredValues = new Dictionary<string, string>(other.RequiredValues, StringComparer.OrdinalIgnoreCase);
        }

        public string ActionName { get; set; }

        public MethodInfo ActionMethod { get; }

        public string ControllerName { get; set; }

        public Type ControllerType { get; }

        public IList<object> EndpointMetadata { get; }

        public ICollection<FilterDescriptor> Filters { get; }

        public int Order { get; set; }

        public ICollection<ControllerActionParameterModel> Parameters { get; }

        public IDictionary<object, object> Properties { get; }

        public string RouteName { get; set; }

        public IDictionary<string, string> RequiredValues { get; }

        public RoutePattern RoutePattern { get; set; }
        
        public override Endpoint Build()
        {
            var filters = Filters.OrderBy(f => f, FilterDescriptorOrderComparer.Comparer).ToList();
            var metadata = EndpointMetadata.ToList();
            var requiredValues = GetRouteValues();

            metadata.Add(new RouteValuesAddressMetadata(RouteName, requiredValues.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value, StringComparer.OrdinalIgnoreCase)));

            // Add filter descriptors to endpoint metadata
            for (var i = 0; i < filters.Count; i++)
            {
                metadata.Add(filters[i].Filter);
            }

            // TODO action constraints

            // TODO suppress link generation / suppress path matching

            var actionDescriptor = new ControllerActionDescriptor()
            {
                ActionConstraints = null, // TODO something
                ActionName = ActionName,
                AttributeRouteInfo = new AttributeRouteInfo()
                {
                    Name = RouteName,
                    Order = Order,
                    SuppressLinkGeneration = false, // TODO something
                    SuppressPathMatching = false, // TODO something
                    Template = RoutePattern.RawText,
                },
                BoundProperties = Parameters
                    .Where(p => p.Property != null)
                    .Select(p => new ControllerBoundPropertyDescriptor()
                    {
                        BindingInfo = p.BindingInfo,
                        Name = p.Name,
                        ParameterType = p.Property.PropertyType,
                        PropertyInfo = p.Property,
                    })
                    .ToList<ParameterDescriptor>(),
                ControllerName = ControllerName,
                ControllerTypeInfo = ControllerType.GetTypeInfo(),
                EndpointMetadata = metadata,
                FilterDescriptors = filters,
                MethodInfo = ActionMethod,
                Parameters = Parameters
                    .Where(p => p.Parameter != null)
                    .Select(p => new ControllerParameterDescriptor()
                    {
                        BindingInfo = p.BindingInfo,
                        Name = p.Name,
                        ParameterType = p.Parameter.ParameterType,
                        ParameterInfo = p.Parameter,
                    })
                    .ToList<ParameterDescriptor>(),
                Properties = Properties,
                RouteValues = GetRouteValues(),
            };

            metadata.Add(actionDescriptor);

            return new RouteEndpoint(
                RequestDelegate,
                RoutePattern,
                Order,
                new EndpointMetadataCollection(metadata),
                DisplayName);
        }

        private Dictionary<string, string> GetRouteValues()
        {
            var values = new Dictionary<string, string>(RequiredValues, StringComparer.OrdinalIgnoreCase);
            if (!values.ContainsKey("action"))
            {
                values.Add("action", ActionName);
            }
            if (!values.ContainsKey("controller"))
            {
                values.Add("controller", ControllerName);
            }

            return values;
        }
    }
}
