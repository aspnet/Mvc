// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// Encapsulates information that creates a <typeparamref name="T"/> instance.
    /// </summary>
    public class DefaultTypeActivatorCache : ITypeActivatorCache
    {
        private readonly Func<Type, ObjectFactory> _createFactory =
            (t) => ActivatorUtilities.CreateFactory(t, Type.EmptyTypes);
        private readonly ConcurrentDictionary<Type, ObjectFactory> _typeActivatorCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        /// <summary>
        /// Creates an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance that retrieves services from the
        /// service collection.</param>
        /// <param name="implementationType">The <see cref="Type"/> of the <typeparamref name="T"/> to create.</param>
        public T CreateInstance<T>([NotNull] IServiceProvider serviceProvider, [NotNull] Type implementationType)
        {
            var createFactory = _typeActivatorCache.GetOrAdd(implementationType, _createFactory);
            return (T)createFactory(serviceProvider, null);
        }
    }
}