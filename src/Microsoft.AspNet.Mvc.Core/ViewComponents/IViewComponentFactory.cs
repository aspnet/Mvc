// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Encapsulates information that creates a ViewComponent.
    /// </summary>
    public interface IViewComponentFactory
    {
        /// <summary>
        /// Creates an instance of ViewComponent.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        /// <param name="componentType">The <see cref="Type"/> of the <see cref="ViewComponent"/> to create.</param>
        object CreateInstance(IServiceProvider serviceProvider, Type componentType);
    }
}