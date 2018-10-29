// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Routing
{
    public sealed class ActionMethodEndpointModel : EndpointModel
    {
        public ActionMethodEndpointModel(Type controllerType, MethodInfo actionMethod)
        {
            if (controllerType == null)
            {
                throw new ArgumentNullException(nameof(controllerType));
            }

            if (actionMethod == null)
            {
                throw new ArgumentNullException(nameof(actionMethod));
            }

            ControllerType = controllerType;
            ActionMethod = actionMethod;
        }

        public Type ControllerType { get; }

        public MethodInfo ActionMethod { get; }

        public override Endpoint Build()
        {
            throw new System.NotImplementedException();
        }
    }
}
