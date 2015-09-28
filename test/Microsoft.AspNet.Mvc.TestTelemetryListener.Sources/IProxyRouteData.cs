﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc
{
    public interface IProxyRouteData
    {
        IReadOnlyList<object> Routers { get; }
        IDictionary<string, object> DataTokens { get; }
        IDictionary<string, object> Values { get; }
    }
}
