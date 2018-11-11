// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing
{
    public class AssemblyEndpointConventionBuilder : IEndpointConventionBuilder
    {
        private readonly List<EndpointDataSource> _dataSources;

        internal AssemblyEndpointConventionBuilder(List<EndpointDataSource> dataSources)
        {
            if (dataSources == null)
            {
                throw new ArgumentNullException(nameof(dataSources));
            }

            _dataSources = dataSources;
        }

        public void Apply(Action<EndpointModel> convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException(nameof(convention));
            }

            for (var i = 0; i < _dataSources.Count; i++)
            {
                if (_dataSources[i] is IEndpointConventionBuilder inner)
                {
                    inner.Apply(convention);
                }
            }
        }
    }
}
