// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    public interface IApplicationFeatureProvider<T>
    {
        T GetFeature(Assembly assembly);
    }
}