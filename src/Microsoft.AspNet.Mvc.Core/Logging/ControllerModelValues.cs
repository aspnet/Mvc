// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Logging;
using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class ControllerModelValues : ILoggerStructure
    {
        public ControllerModel Inner { get; }

        public Dictionary<string, object> _values { get; private set; }

        public ControllerModelValues(ControllerModel inner)
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
                    { "ControllerName", Inner.ControllerName },
                    { "Type", Inner.ControllerType.Name },
                    { "Attributes", string.Join(", ", Inner.Attributes) },
                    { "Filters", string.Join(", ", Inner.Filters) },
                    { "ActionConstraints", string.Join(", ", Inner.ActionConstraints) },
                    { "RouteConstraints", Inner.RouteConstraints.Select(
                        r => new RouteConstraintAttributeValues(r)).ToList() },
                    { "AttributeRoutes", Inner.AttributeRoutes.Select(
                        a => new AttributeRouteModelValues(a)).ToList() }
                };
            }
            return _values;
        }
    }
}