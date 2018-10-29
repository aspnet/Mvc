// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Routing
{
    public class AssemblyConventionBuilder : IEndpointConventionBuilder
    {
        private readonly Assembly _assembly;
        private readonly Dictionary<Type, ControllerConventionBuilder> _controllers;

        public AssemblyConventionBuilder(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            _assembly = assembly;
            _controllers = new Dictionary<Type, ControllerConventionBuilder>();
        }

        public ControllerConventionBuilder GetOrCreateController(Type controllerType)
        {
            if (controllerType == null)
            {
                throw new ArgumentNullException(nameof(controllerType));
            }

            if (!_controllers.TryGetValue(controllerType, out var builder))
            {
                builder = new ControllerConventionBuilder(controllerType);
                _controllers.Add(controllerType, builder);
            }

            return builder;
        }

        public void Apply(Action<EndpointModel> convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException(nameof(convention));
            }

            foreach (var kvp in _controllers)
            {
                kvp.Value.Apply(convention);
            }
        }
    }
}
