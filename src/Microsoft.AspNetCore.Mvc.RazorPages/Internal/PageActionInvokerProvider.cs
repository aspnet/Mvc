// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class PageActionInvokerProvider : IActionInvokerProvider
    {
        private readonly ConcurrentDictionary<PageActionDescriptor, PageActionInvokerCacheEntry> _pageCache;
        private readonly IPageLoader _loader;
        private readonly IPageFactoryProvider _pageFactoryProvider;

        public PageActionInvokerProvider(
            IPageLoader loader,
            IPageFactoryProvider pageFactoryProvider,
            IActionDescriptorCollectionProvider collectionProvider,
            IEnumerable<IFilterProvider> filterProviders)
        {
            _loader = loader;
            _pageFactoryProvider = pageFactoryProvider;
            _pageCache = new ConcurrentDictionary<PageActionDescriptor, PageActionInvokerCacheEntry>();
        }

        public int Order { get; } = -1000;

        public void OnProvidersExecuted(ActionInvokerProviderContext context)
        {
            var actionDescriptor = context.ActionContext.ActionDescriptor as PageActionDescriptor;
            if (actionDescriptor == null)
            {
                return;
            }

            PageActionInvokerCacheEntry cacheEntry;
            if (!_pageCache.TryGetValue(actionDescriptor, out cacheEntry))
            {
                cacheEntry = _pageCache.GetOrAdd(actionDescriptor, CreateCacheEntry(actionDescriptor));
            }

            context.Result = new PageActionInvoker(cacheEntry, context.ActionContext);
        }

        private PageActionInvokerCacheEntry CreateCacheEntry(PageActionDescriptor actionDescriptor)
        {
            var compiledType = _loader.Load(actionDescriptor).GetTypeInfo();
            var modelType = compiledType.GetProperty("Model")?.PropertyType.GetTypeInfo();

            var compiledActionDescriptor = new CompiledPageActionDescriptor(actionDescriptor)
            {
                ModelTypeInfo = modelType,
                PageTypeInfo = compiledType,
            };

            return new PageActionInvokerCacheEntry(
                compiledActionDescriptor,
                _pageFactoryProvider.CreatePage(compiledActionDescriptor),
                new IFilterMetadata[0]);
        }

        public void OnProvidersExecuting(ActionInvokerProviderContext context)
        {
        }
    }
}
