// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Encapsulates information that creates a modelbinder.
    /// </summary>
    public class DefaultModelBinderActivator : IModelBinderActivator
    {
        private readonly ConcurrentDictionary<Type, ObjectFactory> _modelBinderActivatorCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        /// <summary>
        /// Creates an instance of modelbinder.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        /// <param name="binderType">The <see cref="Type"/> modelbinder to create.</param>
        public object CreateInstance([NotNull] IServiceProvider provider, [NotNull] Type binderType)
        {
            var modelBinderFactory = _modelBinderActivatorCache.GetOrAdd(binderType,
                ActivatorUtilitiesHelper.CreateFactory);
            return modelBinderFactory(provider, null);
        }
    }
}