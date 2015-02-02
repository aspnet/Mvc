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
        private readonly IServiceProvider _provider;
        private static readonly ConcurrentDictionary<Type, ObjectFactory> _filterCache =
               new ConcurrentDictionary<Type, ObjectFactory>();

        public DefaultFilterActivator([NotNull] IServiceProvider provider)
        {
            _provider = provider;
        }

        public IFilter CreateInstance([NotNull] Type filterType)
        {

            var filterFact = _filterCache.GetOrAdd(filterType, ActivatorUtilities.CreateFactory(filterType, Type.EmptyTypes));
            //var filter = ActivatorUtilities.CreateInstance(_provider, filterType) as IFilter;
            var filter = filterFact(_provider, null) as IFilter;

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