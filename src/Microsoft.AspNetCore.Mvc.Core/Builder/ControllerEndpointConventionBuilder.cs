// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Routing
{
    public class ControllerEndpointConventionBuilder : IEndpointConventionBuilder
    {
        private readonly List<ControllerEndpointDataSource> _dataSources;

        internal ControllerEndpointConventionBuilder(List<ControllerEndpointDataSource> dataSources)
        {
            if (dataSources == null)
            {
                throw new ArgumentNullException(nameof(dataSources));
            }

            _dataSources = dataSources;
        }

        public void Apply(Action<ControllerActionEndpointModel> convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException(nameof(convention));
            }

            for (var i = 0; i < _dataSources.Count; i++)
            {
                _dataSources[i].Apply(convention);
            }
        }

        void IEndpointConventionBuilder.Apply(Action<EndpointModel> convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException(nameof(convention));
            }

            Apply(convention);
        }

        public ControllerEndpointConventionBuilder MapControllerRoute(
            string name,
            string template,
            object defaults = null,
            object constraints = null,
            object dataTokens = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            for (var i = 0; i < _dataSources.Count; i++)
            {
                _dataSources[i].MapControllerRoute(name, template, defaults, constraints, dataTokens);
            }

            return this;
        }
    }
}
