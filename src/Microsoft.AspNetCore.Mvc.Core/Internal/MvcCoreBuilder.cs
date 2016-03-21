// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    /// <summary>
    /// Allows fine grained configuration of essential MVC services.
    /// </summary>
    public class MvcCoreBuilder : IMvcCoreBuilder
    {
        public MvcCoreBuilder(
            IServiceCollection services,
            ApplicationPartCollection collection)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            Services = services;
            ApplicationParts = collection;
        }

        /// <inheritdoc />
        public ApplicationPartCollection ApplicationParts { get; }

        /// <inheritdoc />
        public IServiceCollection Services { get; }
    }
}