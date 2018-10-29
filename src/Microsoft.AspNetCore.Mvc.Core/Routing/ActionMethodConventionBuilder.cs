// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Routing
{
    public class ActionMethodConventionBuilder
    {
        private readonly MethodInfo _actionMethod;

        public ActionMethodConventionBuilder(MethodInfo actionMethod)
        {
            if (actionMethod == null)
            {
                throw new ArgumentNullException(nameof(actionMethod));
            }

            _actionMethod = actionMethod;
        }

        internal List<ActionMethodEndpointModel> Endpoints { get; } = new List<ActionMethodEndpointModel>();

        public void Apply(Action<EndpointModel> convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException(nameof(convention));
            }

            for (var i = 0; i < Endpoints.Count; i++)
            {
                convention(Endpoints[i]);
            }
        }
    }
}
