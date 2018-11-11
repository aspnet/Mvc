// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Microsoft.AspNetCore.Routing
{
    internal class ApplicationPartApplicationAssemblyProvider : ApplicationAssemblyProvider
    {
        private readonly ApplicationPartManager _partManager;

        public ApplicationPartApplicationAssemblyProvider(ApplicationPartManager partManager)
        {
            if (partManager == null)
            {
                throw new ArgumentNullException(nameof(partManager));
            }
            
            _partManager = partManager;
        }

        public override IReadOnlyList<Assembly> GetApplicationAssemblies()
        {
            var parts = _partManager.ApplicationParts;

            var assemblies = new HashSet<Assembly>();
            for (var i = 0; i < parts.Count; i++)
            {
                if (parts[i] is AssemblyPart assemblyPart)
                {
                    assemblies.Add(assemblyPart.Assembly);
                }
            }

            return assemblies.ToArray();
        }
    }
}
