// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Logging;
using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class ControllerModelValues : LoggerStructureBase
    {
        public ControllerModelValues([NotNull] ControllerModel inner)
        {
            ControllerName = inner.ControllerName;
            Type = inner.ControllerType.Name;
            Attributes = string.Join(", ", inner.Attributes);
            Filters = string.Join(", ", inner.Filters);
            ActionContraints = string.Join(", ", inner.ActionConstraints);
            RouteConstraints = inner.RouteConstraints.Select(
                r => new RouteConstraintAttributeValues(r)).ToList();
            AttributeRoutes = inner.AttributeRoutes.Select(
                a => new AttributeRouteModelValues(a)).ToList();
        }

        public string ControllerName { get; set; }

        public string Type { get; set; }

        public string Attributes { get; set; }

        public string Filters { get; set; }

        public string ActionContraints { get; set; }

        public List<RouteConstraintAttributeValues> RouteConstraints { get; set; }

        public List<AttributeRouteModelValues> AttributeRoutes { get; set; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}