// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Logging;
using System.Linq;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the state of an <see cref="ActionDescriptor"/> or <see cref="ControllerActionDescriptor"/>.
    /// </summary>
    public class ActionDescriptorValues : LoggerStructureBase
    {
        public ActionDescriptorValues(ActionDescriptor inner)
        {
            ActionName = inner.Name;
            Parameters = inner.Parameters.Select(p => new ParameterDescriptorValues(p)).ToList();
            FilterDescriptors = inner.FilterDescriptors.Select(f => new FilterDescriptorValues(f)).ToList();
            RouteConstraints = inner.RouteConstraints.Select(r => new RouteDataActionConstraintValues(r)).ToList();
            AttributeRouteInfo = new AttributeRouteInfoValues(inner.AttributeRouteInfo);
            ActionConstraints = inner.ActionConstraints == null ? string.Empty : string.Join(", ", inner.ActionConstraints);
            var controllerActionDescriptor = inner as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                Method = controllerActionDescriptor.MethodInfo.Name;
                Controller = controllerActionDescriptor.ControllerName;
                Type = controllerActionDescriptor.ControllerDescriptor.ControllerTypeInfo.FullName;
            }
        }

        public string ActionName { get; }

        public List<ParameterDescriptorValues> Parameters { get; }

        public List<FilterDescriptorValues> FilterDescriptors { get; }

        public List<RouteDataActionConstraintValues> RouteConstraints { get; }

        public AttributeRouteInfoValues AttributeRouteInfo { get; }

        public string ActionConstraints { get; }

        public string Method { get; }

        public string Controller { get; }

        public string Type { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}