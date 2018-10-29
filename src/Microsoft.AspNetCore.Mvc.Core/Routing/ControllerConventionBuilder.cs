// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Routing
{
    public class ControllerConventionBuilder : IEndpointConventionBuilder
    {
        private readonly Type _controllerType;
        private readonly Dictionary<MethodInfo, ActionMethodConventionBuilder> _actionMethods;

        public ControllerConventionBuilder(Type controllerType)
        {
            if (controllerType == null)
            {
                throw new ArgumentNullException(nameof(controllerType));
            }

            _controllerType = controllerType;
            _actionMethods = new Dictionary<MethodInfo, ActionMethodConventionBuilder>();
        }

        public ActionMethodConventionBuilder GetOrCreateActionMethod(MethodInfo actionMethod)
        {
            if (actionMethod == null)
            {
                throw new ArgumentNullException(nameof(actionMethod));
            }

            if (!_actionMethods.TryGetValue(actionMethod, out var builder))
            {
                builder = new ActionMethodConventionBuilder(actionMethod);
                _actionMethods.Add(actionMethod, builder);
            }

            return builder;
        }

        public void Apply(Action<EndpointModel> convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException(nameof(convention));
            }

            foreach (var kvp in _actionMethods)
            {
                kvp.Value.Apply(convention);
            }
        }
    }
}
