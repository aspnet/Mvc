// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the state of an <see cref="ActionDescriptor"/> or <see cref="ControllerActionDescriptor"/>. 
    /// Logged during action discovery, this contains the name, parameters, constraints and filters, and if it is a
    /// <see cref="ControllerActionDescriptor"/>, the controller, method, and type
    /// </summary>
    public class ActionDescriptorValues : LoggerStructureBase
    {
        public ActionDescriptorValues(ActionDescriptor inner)
        {
            Name = inner.Name;
            Parameters = inner.Parameters.Select(p => new ParameterValues(p)).ToList();
            FilterDescriptors = inner.FilterDescriptors.Select(f => new FilterDescriptorValues(f)).ToList();
            RouteConstraints = inner.RouteConstraints.Select(r => new RouteDataActionConstraintValues(r)).ToList();
            AttributeRouteInfo = new AttributeRouteInfoValues(inner.AttributeRouteInfo);
            ActionConstraints = inner.ActionConstraints == null ? string.Empty : string.Join(", ", inner.ActionConstraints);
            var controllerActionDescriptor = inner as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                MethodInfo = controllerActionDescriptor.MethodInfo;
                ControllerName = controllerActionDescriptor.ControllerName;
                ControllerTypeInfo = controllerActionDescriptor.ControllerTypeInfo;
            }
        }

        public string Name { get; }

        public List<ParameterValues> Parameters { get; }

        public List<FilterDescriptorValues> FilterDescriptors { get; }

        public List<RouteDataActionConstraintValues> RouteConstraints { get; }

        public AttributeRouteInfoValues AttributeRouteInfo { get; }

        public string ActionConstraints { get; }

        public MethodInfo MethodInfo { get; }

        public string ControllerName { get; }

        public TypeInfo ControllerTypeInfo { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}