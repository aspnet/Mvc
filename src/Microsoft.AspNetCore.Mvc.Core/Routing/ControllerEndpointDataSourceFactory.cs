// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Routing
{
    internal class ControllerEndpointDataSourceFactory : EndpointDataSourceFactory
    {
        public override EndpointDataSource GetOrCreateDataSource(IEndpointRouteBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var dataSource = builder.DataSources.OfType<ControllerDataSource>().FirstOrDefault();
            if (dataSource == null)
            {
                dataSource = builder.ServiceProvider.GetRequiredService<ControllerDataSource>();
                builder.DataSources.Add(dataSource);
            }

            return dataSource;
        }
    }
}
