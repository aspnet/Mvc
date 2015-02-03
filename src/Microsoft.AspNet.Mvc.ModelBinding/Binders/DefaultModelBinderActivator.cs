// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class DefaultModelBinderActivator : IModelBinderActivator
    {
        private readonly IServiceProvider _provider;
        private static readonly Func<Type, ObjectFactory> CreateFactory =
            (t) => ActivatorUtilities.CreateFactory(t, Type.EmptyTypes);
        private static readonly ConcurrentDictionary<Type, ObjectFactory> _modelBinderActivatorCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        public DefaultModelBinderActivator([NotNull] IServiceProvider provider)
        {
            _provider = provider;
        }

        public object CreateInstance([NotNull] Type binderType)
        {
            var modelBinderFactory = _modelBinderActivatorCache.GetOrAdd(binderType, CreateFactory);
            return modelBinderFactory(_provider, null);
        }
    }
}