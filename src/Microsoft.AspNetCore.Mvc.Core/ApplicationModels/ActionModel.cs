// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    [DebuggerDisplay("{Controller.ControllerType.Name}.{ActionMethod.Name}")]
    public class ActionModel : ICommonModel, IFilterModel, IApiExplorerModel
    {
        public ActionModel(
            MethodInfo actionMethod,
            IReadOnlyList<object> attributes)
        {
            if (actionMethod == null)
            {
                throw new ArgumentNullException(nameof(actionMethod));
            }

            if (attributes == null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            ActionMethod = actionMethod;

            ApiExplorer = new ApiExplorerModel();
            Attributes = new List<object>(attributes);
            Filters = new List<IFilterMetadata>();
            Parameters = new List<ParameterModel>();
            RouteConstraints = new List<IRouteConstraintProvider>();
            Properties = new Dictionary<object, object>();
            Selectors = new List<SelectorModel>();
        }

        public ActionModel(ActionModel other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            ActionMethod = other.ActionMethod;
            ActionName = other.ActionName;

            // Not making a deep copy of the controller, this action still belongs to the same controller.
            Controller = other.Controller;

            // These are just metadata, safe to create new collections
            Attributes = new List<object>(other.Attributes);
            Filters = new List<IFilterMetadata>(other.Filters);
            Properties = new Dictionary<object, object>(other.Properties);

            // Make a deep copy of other 'model' types.
            ApiExplorer = new ApiExplorerModel(other.ApiExplorer);
            Parameters = new List<ParameterModel>(other.Parameters.Select(p => new ParameterModel(p)));
            RouteConstraints = new List<IRouteConstraintProvider>(other.RouteConstraints);
            Selectors = new List<SelectorModel>(other.Selectors.Select(s => new SelectorModel(s)));
        }

        public MethodInfo ActionMethod { get; }

        public string ActionName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ApiExplorerModel"/> for this action.
        /// </summary>
        /// <remarks>
        /// <see cref="ActionModel.ApiExplorer"/> allows configuration of settings for ApiExplorer
        /// which apply to the action.
        ///
        /// Settings applied by <see cref="ActionModel.ApiExplorer"/> override settings from
        /// <see cref="ApplicationModel.ApiExplorer"/> and <see cref="ControllerModel.ApiExplorer"/>.
        /// </remarks>
        public ApiExplorerModel ApiExplorer { get; set; }

        public IReadOnlyList<object> Attributes { get; }

        public ControllerModel Controller { get; set; }

        public IList<IFilterMetadata> Filters { get; }

        public IList<ParameterModel> Parameters { get; }

        public IList<IRouteConstraintProvider> RouteConstraints { get; }

        /// <summary>
        /// Gets a set of properties associated with the action.
        /// These properties will be copied to <see cref="Abstractions.ActionDescriptor.Properties"/>.
        /// </summary>
        /// <remarks>
        /// Entries will take precedence over entries with the same key in
        /// <see cref="ApplicationModel.Properties"/> and <see cref="ControllerModel.Properties"/>.
        /// </remarks>
        public IDictionary<object, object> Properties { get; }

        MemberInfo ICommonModel.MemberInfo => ActionMethod;

        string ICommonModel.Name => ActionName;

        public IList<SelectorModel> Selectors { get; }
    }
}
