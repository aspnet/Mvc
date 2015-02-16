// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// <see cref="IControllerActivator"/> that uses type activation to create controllers.
    /// </summary>
    public class DefaultControllerActivator : IControllerActivator
    {
        /// <inheritdoc />
        public object Create([NotNull] ActionContext actionContext, [NotNull] Type controllerType)
        {
            var serviceProvider = actionContext.HttpContext.RequestServices;
            var typeActivatorCache = serviceProvider.GetRequiredService<ITypeActivatorCache>();
            return typeActivatorCache.CreateInstance<object>(serviceProvider, controllerType);
        }
    }
}
