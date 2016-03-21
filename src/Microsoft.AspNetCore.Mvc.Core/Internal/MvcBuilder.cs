// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    /// <summary>
    /// Allows fine grained configuration of MVC services.
    /// </summary>
    public class MvcBuilder : IMvcBuilder
    {
        /// <summary>
        /// Initializes a new <see cref="MvcBuilder"/> instance.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="applicationParts">The <see cref="ApplicationPartCollection"/> of the application.</param>
        public MvcBuilder(IServiceCollection services, ApplicationPartCollection applicationParts)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (applicationParts == null)
            {
                throw new ArgumentNullException(nameof(applicationParts));
            }

            Services = services;
            ApplicationParts = applicationParts;
        }

        /// <inheritdoc />
        public IServiceCollection Services { get; }

        /// <inheritdoc />
        public ApplicationPartCollection ApplicationParts { get; }
    }
}