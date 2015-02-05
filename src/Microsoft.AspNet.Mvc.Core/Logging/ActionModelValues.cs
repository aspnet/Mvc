﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
        // note: omit the controller as this structure is nested inside the ControllerModelValues it belongs to
        public ActionModelValues(ActionModel inner)
        {
            if (inner != null)
            {
                ActionName = inner.ActionName;
                ActionMethod = inner.ActionMethod;
                ApiExplorer = new ApiExplorerModelValues(inner.ApiExplorer);
                Parameters = inner.Parameters.Select(p => new ParameterModelValues(p)).ToList();
                RouteConstraints = inner.RouteConstraints.Select(
                    r => new RouteConstraintProviderValues(r)).ToList();
                Filters = inner.Filters.Select(f => new FilterValues(f)).ToList();
                if (inner.AttributeRouteModel != null)
                {
                    AttributeRouteModel = new AttributeRouteModelValues(inner.AttributeRouteModel);
                }
                HttpMethods = inner.HttpMethods;
                ActionConstraints = inner.ActionConstraints?.Select(a => new ActionConstraintValues(a))?.ToList();
                Properties = new Dictionary<object, object>(inner.Properties);
            }
        }

        /// <summary>
        /// The name of the action. See <see cref="ActionModel.ActionName"/>.
        /// </summary>
        public string ActionName { get; }

        /// <summary>
        /// The method info of the action. See <see cref="ActionModel.ActionMethod"/>.
        /// </summary>
        public MethodInfo ActionMethod { get; }

        /// <summary>
        /// See <see cref="ActionModel.ApiExplorer"/>.
        /// </summary>
        public ApiExplorerModelValues ApiExplorer { get; }

        /// <summary>
        /// The parameters of the action as <see cref="ParameterModelValues"/>. 
        /// See <see cref="ActionModel.Parameters"/>.
        /// </summary>
        public IList<ParameterModelValues> Parameters { get; }

        /// <summary>
        /// The filters of the action as <see cref="FilterValues"/>. 
        /// See <see cref="ActionModel.Filters"/>.
        /// </summary>
        public IList<FilterValues> Filters { get; }
        
        /// <summary>
        /// The route constraints on the controller as <see cref="RouteConstraintProviderValues"/>.
        /// See <see cref="ControllerModel.RouteConstraints"/>.
        /// </summary>
        public IList<RouteConstraintProviderValues> RouteConstraints { get; set; }

        /// <summary>
        /// The attribute route model of the action as <see cref="AttributeRouteModelValues"/>.
        /// See <see cref="ActionModel.AttributeRouteModel"/>.
        /// </summary>
        public AttributeRouteModelValues AttributeRouteModel { get; }

        /// <summary>
        /// The http methods this action supports. See <see cref="ActionModel.HttpMethods"/>.
        /// </summary>
        public IList<string> HttpMethods { get; }

        /// <summary>
        /// The action constraints of the action as <see cref="ActionConstraintValues"/>.
        /// See <see cref="ActionModel.ActionConstraints"/>.
        /// </summary>
        public IList<ActionConstraintValues> ActionConstraints { get; }

        /// <summary>
        /// Gets the set of properties associated with the action <see cref="ActionModel.Properties"/>.
        /// </summary>
        public IDictionary<object, object> Properties { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}