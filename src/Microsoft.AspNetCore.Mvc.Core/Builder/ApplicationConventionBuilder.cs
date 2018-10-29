// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder
{
    internal class ApplicationConventionBuilder : IEndpointConventionBuilder
    {
        private readonly List<Assembly> _assemblies;
        private readonly List<EndpointDataSource> _dataSources;

        public ApplicationConventionBuilder(List<EndpointDataSource> dataSources)
        {
            if (dataSources == null)
            {
                throw new ArgumentNullException(nameof(dataSources));
            }

            _dataSources = dataSources;
            _assemblies = new List<Assembly>();
        }

        public IReadOnlyList<Assembly> Assemblies => _assemblies;

        public IReadOnlyList<EndpointDataSource> DataSources => _dataSources;

        public void AddAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (!_assemblies.Contains(assembly))
            {
                _assemblies.Add(assembly);

                var dataSources = _dataSources;
                for (var i = 0; i < dataSources.Count; i++)
                {
                    var dataSource = dataSources[i];
                    if (dataSource is IEndpointConventionDataSource conventionDataSource)
                    {
                        conventionDataSource.AddAssembly(assembly);
                    }
                }
            }
        }

        public void Apply(Action<EndpointModel> convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException(nameof(convention));
            }

            var dataSources = _dataSources;
            for (var i = 0; i < dataSources.Count; i++)
            {
                var dataSource = dataSources[i];
                if (dataSource is IEndpointConventionDataSource conventionDataSource)
                {
                    conventionDataSource.Apply(convention);
                }
            }
        }
    }
}
