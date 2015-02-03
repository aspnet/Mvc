// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultFilterActivator : IFilterActivator
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly Func<Type, ObjectFactory> CreateFactory =
            (t) => ActivatorUtilities.CreateFactory(t, Type.EmptyTypes);
        private static readonly ConcurrentDictionary<Type, ObjectFactory> _filterCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        public DefaultFilterActivator([NotNull] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFilter CreateInstance([NotNull] Type filterType)
        {

            var filterFactory = _filterCache.GetOrAdd(filterType, CreateFactory);
            var filter = filterFactory(_serviceProvider, null) as IFilter;

            if (filter == null)
            {
                throw new InvalidOperationException(Resources.FormatFilterFactoryAttribute_TypeMustImplementIFilter(
                    typeof(TypeFilterAttribute).Name,
                    typeof(IFilter).Name));
            }

            return filter;
        }
    }
}