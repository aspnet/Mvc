// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Routing
{
    public abstract class ApplicationAssemblyProvider
    {
        public abstract IReadOnlyList<Assembly> GetApplicationAssemblies();
    }
}
