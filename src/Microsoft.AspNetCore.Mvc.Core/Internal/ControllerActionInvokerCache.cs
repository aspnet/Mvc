// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ControllerActionInvokerCache
    {
        private readonly IActionDescriptorCollectionProvider _collectionProvider;
        private readonly IFilterProvider[] _filterProviders;

        private volatile InnerCache _currentCache;

        public ControllerActionInvokerCache(
            IActionDescriptorCollectionProvider collectionProvider,
            IEnumerable<IFilterProvider> filterProviders)
        {
            _collectionProvider = collectionProvider;
            _filterProviders = filterProviders.OrderBy(item => item.Order).ToArray();
        }

        private InnerCache CurrentCache
        {
            get
            {
                var current = _currentCache;
                var actionDescriptors = _collectionProvider.ActionDescriptors;

                if (current == null || current.Version != actionDescriptors.Version)
                {
                    current = new InnerCache(actionDescriptors.Version);
                    _currentCache = current;
                }

                return current;
            }
        }

        public ControllerActionInvokerState GetState(ControllerContext controllerContext)
        {
            var cache = CurrentCache;
            var actionDescriptor = controllerContext.ActionDescriptor;

            Entry cacheEntry;
            if (!cache.Entries.TryGetValue(actionDescriptor, out cacheEntry))
            {
                var executor = ObjectMethodExecutor.Create(
                    actionDescriptor.MethodInfo,
                    actionDescriptor.ControllerTypeInfo);
                var filterFactory = FilterFactoryProvider.GetFilterFactory(_filterProviders, controllerContext);
                cacheEntry = new Entry(filterFactory, executor);
                cacheEntry = cache.Entries.GetOrAdd(actionDescriptor, cacheEntry);
            }

            // Filter instances from statically defined filter descriptors + from filter providers
            var filters = cacheEntry.FilterFactory(controllerContext);
            return new ControllerActionInvokerState(filters, cacheEntry.ActionMethodExecutor);
        }

        private class InnerCache
        {
            public InnerCache(int version)
            {
                Version = version;
            }

            public ConcurrentDictionary<ActionDescriptor, Entry> Entries { get; } =
                new ConcurrentDictionary<ActionDescriptor, Entry>();

            public int Version { get; }
        }

        private struct Entry
        {
            public Entry(Func<ActionContext, IFilterMetadata[]> filterFactory, ObjectMethodExecutor executor)
            {
                FilterFactory = filterFactory;
                ActionMethodExecutor = executor;
            }

            public Func<ActionContext, IFilterMetadata[]> FilterFactory { get; }

            public ObjectMethodExecutor ActionMethodExecutor { get; }
        }

        public struct ControllerActionInvokerState
        {
            public ControllerActionInvokerState(
                IFilterMetadata[] filters,
                ObjectMethodExecutor actionMethodExecutor)
            {
                Filters = filters;
                ActionMethodExecutor = actionMethodExecutor;
            }

            public IFilterMetadata[] Filters { get; }

            public ObjectMethodExecutor ActionMethodExecutor { get; set; }
        }
    }
}
