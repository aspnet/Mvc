// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Framework.Logging;
using System.Linq;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the state of a <see cref="ControllerActionDescriptor"/>
    /// </summary>
    public class ControllerActionDescriptorValues : ILoggerStructure
    {
        public ControllerActionDescriptor Inner { get; }

        private Dictionary<string, object> _values;

        public ControllerActionDescriptorValues(ControllerActionDescriptor inner)
        {
            Inner = inner;
        }

        public string Format()
        {
            return LogFormatter.FormatStructure(this);
        }

        public IEnumerable<KeyValuePair<string, object>> GetValues()
        {
            if (_values == null)
            {
                _values = new Dictionary<string, object> {
                    { "ActionName", Inner.Name },
                    { "Method", Inner.MethodInfo.Name },
                    { "Parameters", Inner.Parameters.Select(p => new ParameterDescriptorValues(p)).ToList() },
                    { "Controller", Inner.ControllerName },
                    { "Type", Inner.ControllerDescriptor.ControllerTypeInfo.FullName },
                    { "FilterDescriptors", Inner.FilterDescriptors.Select(
                        f => new FilterDescriptorValues(f)).ToList() },
                    { "RouteConstraints", Inner.RouteConstraints.Select(
                        r => new RouteDataActionConstraintValues(r)).ToList() },
                    { "AttributeRouteInfo", new AttributeRouteInfoValues(Inner.AttributeRouteInfo) },
                    { "ActionConstraints", Inner.ActionConstraints == null ? string.Empty : string.Join(
                        ", ", Inner.ActionConstraints) }
                };
            }
            return _values;
        }
    }
}