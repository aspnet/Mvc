// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.ApplicationModels;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the state of an <see cref="ActionModel"/>.
    /// Logged as a substructure of <see cref="ControllerModelValues"/>
    /// </summary>
    public class ActionModelValues : LoggerStructureBase
    {
        public ActionModelValues(ActionModel inner)
        {
            ActionName = inner.ActionName;
            ActionMethod = inner.ActionMethod;
            Parameters = inner.Parameters.Select(p => new ParameterValues(p)).ToList();
            Filters = inner.Filters.Select(f => new FilterValues(f)).ToList();
            if (inner.AttributeRouteModel != null)
            {
                AttributeRouteModel = new AttributeRouteModelValues(inner.AttributeRouteModel);
            }
            HttpMethods = inner.HttpMethods;
            ActionConstraints = inner.ActionConstraints ==
                null ? string.Empty : string.Join(", ", inner.ActionConstraints);
            IsActionNameMatchRequired = inner.IsActionNameMatchRequired;
        }

        // note: omit the controller as this structure is nested inside the ControllerModelValues it belongs to
        public string ActionName { get; }

        public MethodInfo ActionMethod { get; }

        public List<ParameterValues> Parameters { get; }

        public List<FilterValues> Filters { get; }

        public AttributeRouteModelValues AttributeRouteModel { get; }

        public List<string> HttpMethods { get; }

        public string ActionConstraints { get; }

        public bool IsActionNameMatchRequired { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}