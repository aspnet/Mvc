// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// An interface for configuring essential MVC services.
    /// </summary>
    public interface IMvcCoreBuilder
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> where essential MVC services are configured.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Gets a list of <see cref="ApplicationPart"/> instances that compose the application.
        /// </summary>
        ApplicationPartCollection ApplicationParts { get; }
    }
}
