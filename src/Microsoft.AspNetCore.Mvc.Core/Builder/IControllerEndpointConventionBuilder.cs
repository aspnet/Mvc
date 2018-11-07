// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder
{
    public interface IControllerEndpointConventionBuilder : IEndpointConventionBuilder
    {
        IControllerEndpointConventionBuilder ForControllerType(Type type);
        IControllerEndpointConventionBuilder ForAssemblyType(Type type);
    }
}
