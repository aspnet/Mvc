// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Blazor.Components;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public class ComponentFeatureProvider : IApplicationFeatureProvider<ComponentFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ComponentFeature feature)
        {
            if (parts == null)
            {
                throw new ArgumentNullException(nameof(parts));
            }

            if (feature == null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            foreach (var part in parts)
            {
                if (part is IApplicationPartTypeProvider types)
                {
                    foreach (var type in types.Types)
                    {
                        if (!type.IsAbstract &&
                            !type.IsInterface &&
                            typeof(IComponent).IsAssignableFrom(type) && 
                            type.GetCustomAttribute(typeof(Blazor.Components.RouteAttribute), inherit: true) is Blazor.Components.RouteAttribute route)
                        {
                            feature.Components.Add(new ComponentInfo(type.AsType(), route.Template));
                        }
                    }
                }
            }
        }
    }
}
