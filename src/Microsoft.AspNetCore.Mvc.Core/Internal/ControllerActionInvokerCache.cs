// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class ControllerActionInvokerCache
    {
        private readonly IFilterMetadata[] EmptyFilterArray = new IFilterMetadata[0];

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

        public EntryInfo GetCacheEntryInfo(ControllerContext controllerContext)
        {
            // Filter instances from statically defined filter descriptors + from filter providers
            IFilterMetadata[] allFilters;

            var cache = CurrentCache;
            var actionDescriptor = controllerContext.ActionDescriptor;

            Entry cacheEntry;
            if (cache.Entries.TryGetValue(actionDescriptor, out cacheEntry))
            {
                allFilters = GetAllFilters(controllerContext, cacheEntry.FilterItems);

                return new EntryInfo(allFilters, cacheEntry);
            }

            var executor = ObjectMethodExecutor.Create(
                actionDescriptor.MethodInfo,
                actionDescriptor.ControllerTypeInfo);

            var staticFilterItems = new List<FilterItem>(actionDescriptor.FilterDescriptors.Count);
            for (var i = 0; i < actionDescriptor.FilterDescriptors.Count; i++)
            {
                staticFilterItems.Add(new FilterItem(actionDescriptor.FilterDescriptors[i]));
            }

            allFilters = GetAllFilters(controllerContext, staticFilterItems);

            // Cache the filter items based on the following criteria
            // 1. Are created statically (ex: via filter attributes, added to global filter list etc.)
            // 2. Are re-usable
            for (var i = 0; i < staticFilterItems.Count; i++)
            {
                var item = staticFilterItems[i];
                if (!item.IsReusable)
                {
                    item.Filter = null;
                }
            }
            cacheEntry = new Entry(staticFilterItems, executor);
            cache.Entries.TryAdd(actionDescriptor, cacheEntry);

            return new EntryInfo(allFilters, cacheEntry);
        }

        private IFilterMetadata[] GetAllFilters(ActionContext actionContext, List<FilterItem> staticFilterItems)
        {
            // Create a separate collection as we want to hold onto the statically defined filter items
            // in order to cache them
            var filterItems = new List<FilterItem>(staticFilterItems);

            // Execute providers
            var context = new FilterProviderContext(actionContext, filterItems);

            for (var i = 0; i < _filterProviders.Length; i++)
            {
                _filterProviders[i].OnProvidersExecuting(context);
            }

            for (var i = _filterProviders.Length - 1; i >= 0; i--)
            {
                _filterProviders[i].OnProvidersExecuted(context);
            }

            // Extract filter instances from statically defined filters and filter providers
            var count = 0;
            for (var i = 0; i < filterItems.Count; i++)
            {
                if (filterItems[i].Filter != null)
                {
                    count++;
                }
            }

            if (count == 0)
            {
                return EmptyFilterArray;
            }
            else
            {
                var filters = new IFilterMetadata[count];
                var filterIndex = 0;
                for (int i = 0; i < filterItems.Count; i++)
                {
                    var filter = filterItems[i].Filter;
                    if (filter != null)
                    {
                        filters[filterIndex++] = filter;
                    }
                }

                return filters;
            }
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

        public struct Entry
        {
            public Entry(List<FilterItem> items, ObjectMethodExecutor executor)
            {
                FilterItems = items;
                ActionMethodExecutor = executor;
            }

            public List<FilterItem> FilterItems { get; }

            public ObjectMethodExecutor ActionMethodExecutor { get; }
        }

        public struct EntryInfo
        {
            public EntryInfo(
                IFilterMetadata[] allFilters,
                Entry cacheEntry)
            {
                AllFilters = allFilters;
                CacheEntry = cacheEntry;
            }

            public IFilterMetadata[] AllFilters { get; }

            public Entry CacheEntry { get; set; }
        }
    }
}
