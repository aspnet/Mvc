// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Logging;
using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the state of a <see cref="ControllerModel"/>. Logged during controller discovery,
    /// this contains the name, type, actions, constraints, filters, and routes of the
    /// given <see cref="ControllerModel"/>.
    /// </summary>
    public class ControllerModelValues : LoggerStructureBase
    {
        public ControllerModelValues([NotNull] ControllerModel inner)
        {
            ControllerName = inner.ControllerName;
            ControllerType = inner.ControllerType.AsType();
            Actions = inner.Actions.Select(a => new ActionModelValues(a)).ToList();
            Attributes = string.Join(", ", inner.Attributes);
            Filters = inner.Filters.Select(f => new FilterValues(f)).ToList();
            // TODO: better representation of IActionContraintMetadata
            ActionConstraints = string.Join(", ", inner.ActionConstraints);
            RouteConstraints = inner.RouteConstraints.Select(
                r => new RouteConstraintAttributeValues(r)).ToList();
            AttributeRoutes = inner.AttributeRoutes.Select(
                a => new AttributeRouteModelValues(a)).ToList();
        }

        public string ControllerName { get; }

        public Type ControllerType { get; }

        public List<ActionModelValues> Actions { get; }

        public string Attributes { get; }

        public List<FilterValues> Filters { get; }

        public string ActionConstraints { get; }

        public List<RouteConstraintAttributeValues> RouteConstraints { get; set; }

        public List<AttributeRouteModelValues> AttributeRoutes { get; set; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}