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
        /// <summary>
        /// Initializes a new <see cref="MvcCoreBuilder"/> instance.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="manager">The <see cref="ApplicationPartManager"/> of the application.</param>
        public MvcCoreBuilder(
            IServiceCollection services,
            ApplicationPartManager manager)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            Services = services;
            PartManager = manager;
        }

        /// <inheritdoc />
        public ApplicationPartManager PartManager { get; }

        /// <inheritdoc />
        public IServiceCollection Services { get; }
    }
}