// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Routing
{
    internal class ControllerDataSource : EndpointDataSource, IEndpointConventionDataSource
    {
        private readonly HashSet<Assembly> _assemblies;
        private readonly List<ActionMethodEndpointModel> _models;
        
        public ControllerDataSource()
        {
            _assemblies = new HashSet<Assembly>();
            _models = new List<ActionMethodEndpointModel>();
        }

        public override IReadOnlyList<Endpoint> Endpoints => throw new NotImplementedException();

        public void AddAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (_assemblies.Add(assembly))
            {
                var types = assembly.DefinedTypes.Select(ControllerDiscovery.IsControllerType);
            }
        }

        public void Apply(Action<EndpointModel> convention)
        {
            throw new NotImplementedException();
        }

        public override IChangeToken GetChangeToken()
        {
            throw new NotImplementedException();
        }   
    }
}
