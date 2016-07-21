// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public class PrecompiledViewsFeature
    {
        public IDictionary<string, Type> PrecompiledViews { get; } =
            new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
    }
}
