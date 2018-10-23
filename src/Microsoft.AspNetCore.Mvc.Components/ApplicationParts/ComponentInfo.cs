// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public class ComponentInfo
    {
        public ComponentInfo(Type componentType, string routeTemplate)
        {
            ComponentType = componentType;
            RouteTemplate = routeTemplate;
        }

        public Type ComponentType { get; }

        public string RouteTemplate { get; }
    }
}
