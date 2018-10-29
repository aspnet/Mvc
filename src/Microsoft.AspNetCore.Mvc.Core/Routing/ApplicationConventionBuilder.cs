// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Routing
{
    public class ApplicationConventionBuilder : IEndpointConventionBuilder
    {
        private readonly Dictionary<Assembly, AssemblyConventionBuilder> _assemblies;

        public ApplicationConventionBuilder()
        {
            _assemblies = new Dictionary<Assembly, AssemblyConventionBuilder>();
        }

        public AssemblyConventionBuilder GetOrCreateAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (!_assemblies.TryGetValue(assembly, out var builder))
            {
                builder = new AssemblyConventionBuilder(assembly);
                _assemblies.Add(assembly, builder);
            }

            return builder;
        }

        public void Apply(Action<EndpointModel> convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException(nameof(convention));
            }

            foreach (var kvp in _assemblies)
            {
                kvp.Value.Apply(convention);
            }
        }
    }
}
