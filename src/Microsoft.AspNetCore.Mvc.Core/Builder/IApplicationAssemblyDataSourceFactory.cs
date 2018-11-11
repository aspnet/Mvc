// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Routing
{
    public interface IApplicationAssemblyDataSourceFactory
    {
        EndpointDataSource GetOrCreateDataSource(ICollection<EndpointDataSource> dataSources, Assembly assembly);
    }
}
