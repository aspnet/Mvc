﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class PageActionInvokerProvider : IActionInvokerProvider
    {
        private const string ViewStartFileName = "_ViewStart.cshtml";

        private readonly IPageLoader _loader;
        private readonly IPageFactoryProvider _pageFactoryProvider;
        private readonly IPageModelFactoryProvider _modelFactoryProvider;
        private readonly IRazorPageFactoryProvider _razorPageFactoryProvider;
        private readonly IActionDescriptorCollectionProvider _collectionProvider;
        private readonly IFilterProvider[] _filterProviders;
        private readonly IReadOnlyList<IValueProviderFactory> _valueProviderFactories;
        private readonly ParameterBinder _parameterBinder;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ITempDataDictionaryFactory _tempDataFactory;
        private readonly HtmlHelperOptions _htmlHelperOptions;
        private readonly RazorPagesOptions _razorPagesOptions;
        private readonly IPageHandlerMethodSelector _selector;
        private readonly RazorProject _razorProject;
        private readonly DiagnosticSource _diagnosticSource;
        private readonly ILogger<PageActionInvoker> _logger;
        private volatile InnerCache _currentCache;

        public PageActionInvokerProvider(
            IPageLoader loader,
            IPageFactoryProvider pageFactoryProvider,
            IPageModelFactoryProvider modelFactoryProvider,
            IRazorPageFactoryProvider razorPageFactoryProvider,
            IActionDescriptorCollectionProvider collectionProvider,
            IEnumerable<IFilterProvider> filterProviders,
            ParameterBinder parameterBinder,
            IModelMetadataProvider modelMetadataProvider,
            ITempDataDictionaryFactory tempDataFactory,
            IOptions<MvcOptions> mvcOptions,
            IOptions<HtmlHelperOptions> htmlHelperOptions,
            IOptions<RazorPagesOptions> razorPagesOptions,
            IPageHandlerMethodSelector selector,
            RazorProject razorProject,
            DiagnosticSource diagnosticSource,
            ILoggerFactory loggerFactory)
        {
            _loader = loader;
            _pageFactoryProvider = pageFactoryProvider;
            _modelFactoryProvider = modelFactoryProvider;
            _razorPageFactoryProvider = razorPageFactoryProvider;
            _collectionProvider = collectionProvider;
            _filterProviders = filterProviders.ToArray();
            _valueProviderFactories = mvcOptions.Value.ValueProviderFactories.ToArray();
            _parameterBinder = parameterBinder;
            _modelMetadataProvider = modelMetadataProvider;
            _tempDataFactory = tempDataFactory;
            _htmlHelperOptions = htmlHelperOptions.Value;
            _razorPagesOptions = razorPagesOptions.Value;
            _selector = selector;
            _razorProject = razorProject;
            _diagnosticSource = diagnosticSource;
            _logger = loggerFactory.CreateLogger<PageActionInvoker>();
        }

        public int Order { get; } = -1000;

        public void OnProvidersExecuting(ActionInvokerProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var actionContext = context.ActionContext;
            var actionDescriptor = actionContext.ActionDescriptor as PageActionDescriptor;
            if (actionDescriptor == null)
            {
                return;
            }

            var cache = CurrentCache;
            PageActionInvokerCacheEntry cacheEntry;

            IFilterMetadata[] filters;
            if (!cache.Entries.TryGetValue(actionDescriptor, out cacheEntry))
            {
                var filterFactoryResult = FilterFactory.GetAllFilters(_filterProviders, actionContext);
                filters = filterFactoryResult.Filters;
                cacheEntry = CreateCacheEntry(context, filterFactoryResult.CacheableFilters);
                cacheEntry = cache.Entries.GetOrAdd(actionDescriptor, cacheEntry);
            }
            else
            {
                filters = FilterFactory.CreateUncachedFilters(
                    _filterProviders,
                    actionContext,
                    cacheEntry.CacheableFilters);
            }

            context.Result = CreateActionInvoker(actionContext, cacheEntry, filters);
        }

        public void OnProvidersExecuted(ActionInvokerProviderContext context)
        {

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

        private PageActionInvoker CreateActionInvoker(
            ActionContext actionContext,
            PageActionInvokerCacheEntry cacheEntry,
            IFilterMetadata[] filters)
        {
            var tempData = _tempDataFactory.GetTempData(actionContext.HttpContext);
            var pageContext = new PageContext(
                actionContext,
                new ViewDataDictionary(_modelMetadataProvider, actionContext.ModelState),
                tempData,
                _htmlHelperOptions);

            pageContext.ActionDescriptor = cacheEntry.ActionDescriptor;

            return new PageActionInvoker(
                _selector,
                _diagnosticSource,
                _logger,
                pageContext,
                filters,
                new CopyOnWriteList<IValueProviderFactory>(_valueProviderFactories),
                cacheEntry,
                _parameterBinder);
        }

        private PageActionInvokerCacheEntry CreateCacheEntry(
            ActionInvokerProviderContext context,
            FilterItem[] cachedFilters)
        {
            var actionDescriptor = (PageActionDescriptor)context.ActionContext.ActionDescriptor;
            var compiledActionDescriptor = _loader.Load(actionDescriptor);

            var pageFactory = _pageFactoryProvider.CreatePageFactory(compiledActionDescriptor);
            var pageDisposer = _pageFactoryProvider.CreatePageDisposer(compiledActionDescriptor);
            var propertyBinder = PagePropertyBinderFactory.CreateBinder(
                _parameterBinder,
                _modelMetadataProvider,
                compiledActionDescriptor);

            Func<PageContext, object> modelFactory = null;
            Action<PageContext, object> modelReleaser = null;
            if (compiledActionDescriptor.ModelTypeInfo != compiledActionDescriptor.PageTypeInfo)
            {
                modelFactory = _modelFactoryProvider.CreateModelFactory(compiledActionDescriptor);
                modelReleaser = _modelFactoryProvider.CreateModelDisposer(compiledActionDescriptor);
            }

            var viewStartFactories = GetViewStartFactories(compiledActionDescriptor);

            var executors = GetExecutors(compiledActionDescriptor);

            return new PageActionInvokerCacheEntry(
                compiledActionDescriptor,
                pageFactory,
                pageDisposer,
                modelFactory,
                modelReleaser,
                propertyBinder,
                executors,
                viewStartFactories,
                cachedFilters);
        }

        // Internal for testing.
        internal List<Func<IRazorPage>> GetViewStartFactories(CompiledPageActionDescriptor descriptor)
        {
            var viewStartFactories = new List<Func<IRazorPage>>();
            var viewStartItems = _razorProject.FindHierarchicalItems(
                _razorPagesOptions.RootDirectory,
                descriptor.RelativePath,
                ViewStartFileName);
            foreach (var item in viewStartItems)
            {
                var factoryResult = _razorPageFactoryProvider.CreateFactory(item.Path);
                if (factoryResult.Success)
                {
                    viewStartFactories.Insert(0, factoryResult.RazorPageFactory);
                }
            }

            return viewStartFactories;
        }

        private static object GetDefaultValue(ParameterInfo methodParameter)
        {
            object defaultValue = null;
            if (methodParameter.HasDefaultValue)
            {
                defaultValue = methodParameter.DefaultValue;
            }
            else if (methodParameter.ParameterType.GetTypeInfo().IsValueType)
            {
                defaultValue = Activator.CreateInstance(methodParameter.ParameterType);
            }

            return defaultValue;
        }

        private static Func<object, object[], Task<IActionResult>>[] GetExecutors(CompiledPageActionDescriptor actionDescriptor)
        {
            if (actionDescriptor.HandlerMethods == null || actionDescriptor.HandlerMethods.Count == 0)
            {
                return Array.Empty<Func<object, object[], Task<IActionResult>>>();
            }

            var results = new Func<object, object[], Task<IActionResult>>[actionDescriptor.HandlerMethods.Count];

            for (var i = 0; i < actionDescriptor.HandlerMethods.Count; i++)
            {
                results[i] = ExecutorFactory.CreateExecutor(actionDescriptor.HandlerMethods[i]);
            }

            return results;
        }

        internal class InnerCache
        {
            public InnerCache(int version)
            {
                Version = version;
            }

            public ConcurrentDictionary<ActionDescriptor, PageActionInvokerCacheEntry> Entries { get; } =
                new ConcurrentDictionary<ActionDescriptor, PageActionInvokerCacheEntry>();

            public int Version { get; }
        }
    }
}
