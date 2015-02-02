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
        private static readonly ConcurrentDictionary<Type, ObjectFactory> _modelBinderActivatorCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        public DefaultModelBinderActivator([NotNull] IServiceProvider provider)
        {
            _provider = provider;
        }

        public object CreateInstance([NotNull] Type binderType)
        {
            var modelBindeFactory = _modelBinderActivatorCache.GetOrAdd(binderType, ActivatorUtilities.CreateFactory(binderType, Type.EmptyTypes));
            return modelBindeFactory(_provider, null);
            //return ActivatorUtilities.CreateInstance(_provider, binderType);
        }
    }
}