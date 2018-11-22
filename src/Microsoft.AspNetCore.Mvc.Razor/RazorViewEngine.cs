// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.Razor
{
    /// <summary>
    /// Default implementation of <see cref="IRazorViewEngine"/>.
    /// </summary>
    /// <remarks>
    /// For <c>ViewResults</c> returned from controllers, views should be located in
    /// <see cref="RazorViewEngineOptions.ViewLocationFormats"/>
    /// by default. For the controllers in an area, views should exist in
    /// <see cref="RazorViewEngineOptions.AreaViewLocationFormats"/>.
    /// </remarks>
    public class RazorViewEngine : IRazorViewEngine
    {
        public static readonly string ViewExtension = ".cshtml";

        private const string AreaKey = "area";
        private const string ControllerKey = "controller";
        private const string PageKey = "page";

        private static readonly TimeSpan _cacheExpirationDuration = TimeSpan.FromMinutes(20);

        private readonly IRazorPageFactoryProvider _pageFactory;
        private readonly IRazorPageActivator _pageActivator;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly ILogger _logger;
        private readonly RazorViewEngineOptions _options;
        private readonly DiagnosticListener _diagnosticListener;

        /// <summary>
        /// Initializes a new instance of the <see cref="RazorViewEngine" />.
        /// </summary>
        public RazorViewEngine(
            IRazorPageFactoryProvider pageFactory,
            IRazorPageActivator pageActivator,
            HtmlEncoder htmlEncoder,
            IOptions<RazorViewEngineOptions> optionsAccessor,
            ILoggerFactory loggerFactory,
            DiagnosticListener diagnosticListener)
        {
            _options = optionsAccessor.Value;

            if (_options.ViewLocationFormats.Count == 0)
            {
                throw new ArgumentException(
                    Resources.FormatViewLocationFormatsIsRequired(nameof(RazorViewEngineOptions.ViewLocationFormats)),
                    nameof(optionsAccessor));
            }

            if (_options.AreaViewLocationFormats.Count == 0)
            {
                throw new ArgumentException(
                    Resources.FormatViewLocationFormatsIsRequired(nameof(RazorViewEngineOptions.AreaViewLocationFormats)),
                    nameof(optionsAccessor));
            }

            _pageFactory = pageFactory;
            _pageActivator = pageActivator;
            _htmlEncoder = htmlEncoder;
            _logger = loggerFactory.CreateLogger<RazorViewEngine>();
            _diagnosticListener = diagnosticListener;
            ViewLookupCache = new MemoryCache(new MemoryCacheOptions());
        }

        /// <summary>
        /// A cache for results of view lookups.
        /// </summary>
        protected IMemoryCache ViewLookupCache { get; }

        /// <summary>
        /// Gets the case-normalized route value for the specified route <paramref name="key"/>.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <param name="key">The route key to lookup.</param>
        /// <returns>The value corresponding to the key.</returns>
        /// <remarks>
        /// The casing of a route value in <see cref="ActionContext.RouteData"/> is determined by the client.
        /// This making constructing paths for view locations in a case sensitive file system unreliable. Using the
        /// <see cref="Abstractions.ActionDescriptor.RouteValues"/> to get route values
        /// produces consistently cased results.
        /// </remarks>
        public static string GetNormalizedRouteValue(ActionContext context, string key)
            => NormalizedRouteValue.GetNormalizedRouteValue(context, key);

        /// <inheritdoc />
        public RazorPageResult FindPage(ActionContext context, string pageName)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrEmpty(pageName))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(pageName));
            }

            if (IsApplicationRelativePath(pageName) || IsRelativePath(pageName))
            {
                // A path; not a name this method can handle.
                return new RazorPageResult(pageName, Enumerable.Empty<string>());
            }

            var cacheResult = LocatePageFromViewLocations(context, pageName, isMainPage: false);
            if (cacheResult.Success)
            {
                var razorPage = cacheResult.ViewEntry.PageFactory();
                return new RazorPageResult(pageName, razorPage);
            }
            else
            {
                return new RazorPageResult(pageName, cacheResult.SearchedLocations);
            }
        }

        /// <inheritdoc />
        public RazorPageResult GetPage(string executingFilePath, string pagePath)
        {
            if (string.IsNullOrEmpty(pagePath))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(pagePath));
            }

            if (!(IsApplicationRelativePath(pagePath) || IsRelativePath(pagePath)))
            {
                // Not a path this method can handle.
                return new RazorPageResult(pagePath, Enumerable.Empty<string>());
            }

            var cacheResult = LocatePageFromPath(executingFilePath, pagePath, isMainPage: false);
            if (cacheResult.Success)
            {
                var razorPage = cacheResult.ViewEntry.PageFactory();
                return new RazorPageResult(pagePath, razorPage);
            }
            else
            {
                return new RazorPageResult(pagePath, cacheResult.SearchedLocations);
            }
        }

        /// <inheritdoc />
        public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(viewName));
            }

            if (IsApplicationRelativePath(viewName) || IsRelativePath(viewName))
            {
                // A path; not a name this method can handle.
                return ViewEngineResult.NotFound(viewName, Enumerable.Empty<string>());
            }

            var cacheResult = LocatePageFromViewLocations(context, viewName, isMainPage);
            return CreateViewEngineResult(cacheResult, viewName);
        }

        /// <inheritdoc />
        public ViewEngineResult GetView(string executingFilePath, string viewPath, bool isMainPage)
        {
            if (string.IsNullOrEmpty(viewPath))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(viewPath));
            }

            if (!(IsApplicationRelativePath(viewPath) || IsRelativePath(viewPath)))
            {
                // Not a path this method can handle.
                return ViewEngineResult.NotFound(viewPath, Enumerable.Empty<string>());
            }

            var cacheResult = LocatePageFromPath(executingFilePath, viewPath, isMainPage);
            return CreateViewEngineResult(cacheResult, viewPath);
        }

        private ViewLocationCacheResult LocatePageFromPath(string executingFilePath, string pagePath, bool isMainPage)
        {
            var applicationRelativePath = GetAbsolutePath(executingFilePath, pagePath);
            var cacheKey = new ViewLocationCacheKey(applicationRelativePath, isMainPage);
            if (!ViewLookupCache.TryGetValue(cacheKey, out ViewLocationCacheResult cacheResult))
            {
                var expirationTokens = new HashSet<IChangeToken>();
                cacheResult = CreateCacheResult(expirationTokens, applicationRelativePath, isMainPage);

                var cacheEntryOptions = new MemoryCacheEntryOptions();
                cacheEntryOptions.SetSlidingExpiration(_cacheExpirationDuration);
                foreach (var expirationToken in expirationTokens)
                {
                    cacheEntryOptions.AddExpirationToken(expirationToken);
                }

                // No views were found at the specified location. Create a not found result.
                if (cacheResult == null)
                {
                    cacheResult = new ViewLocationCacheResult(new[] { applicationRelativePath });
                }

                cacheResult = ViewLookupCache.Set(
                    cacheKey,
                    cacheResult,
                    cacheEntryOptions);
            }

            return cacheResult;
        }

        private ViewLocationCacheResult LocatePageFromViewLocations(
            ActionContext actionContext,
            string pageName,
            bool isMainPage)
        {
            var controllerName = GetNormalizedRouteValue(actionContext, ControllerKey);
            var areaName = GetNormalizedRouteValue(actionContext, AreaKey);
            string razorPageName = null;
            if (actionContext.ActionDescriptor.RouteValues.ContainsKey(PageKey))
            {
                // Only calculate the Razor Page name if "page" is registered in RouteValues.
                razorPageName = GetNormalizedRouteValue(actionContext, PageKey);
            }

            var expanderContext = new ViewLocationExpanderContext(
                actionContext,
                pageName,
                controllerName,
                areaName,
                razorPageName,
                isMainPage);
            Dictionary<string, string> expanderValues = null;

            if (_options.ViewLocationExpanders.Count > 0)
            {
                expanderValues = new Dictionary<string, string>(StringComparer.Ordinal);
                expanderContext.Values = expanderValues;

                // Perf: Avoid allocations
                for (var i = 0; i < _options.ViewLocationExpanders.Count; i++)
                {
                    _options.ViewLocationExpanders[i].PopulateValues(expanderContext);
                }
            }

            var cacheKey = new ViewLocationCacheKey(
                expanderContext.ViewName,
                expanderContext.ControllerName,
                expanderContext.AreaName,
                expanderContext.PageName,
                expanderContext.IsMainPage,
                expanderValues);

            if (!ViewLookupCache.TryGetValue(cacheKey, out ViewLocationCacheResult cacheResult))
            {
                _logger.ViewLookupCacheMiss(cacheKey.ViewName, cacheKey.ControllerName);
                cacheResult = OnCacheMiss(expanderContext, cacheKey);
            }
            else
            {
                _logger.ViewLookupCacheHit(cacheKey.ViewName, cacheKey.ControllerName);
            }

            return cacheResult;
        }

        /// <inheritdoc />
        public string GetAbsolutePath(string executingFilePath, string pagePath)
        {
            if (string.IsNullOrEmpty(pagePath))
            {
                // Path is not valid; no change required.
                return pagePath;
            }

            if (IsApplicationRelativePath(pagePath))
            {
                // An absolute path already; no change required.
                return pagePath;
            }

            if (!IsRelativePath(pagePath))
            {
                // A page name; no change required.
                return pagePath;
            }

            if (string.IsNullOrEmpty(executingFilePath))
            {
                // Given a relative path i.e. not yet application-relative (starting with "~/" or "/"), interpret
                // path relative to currently-executing view, if any.
                // Not yet executing a view. Start in app root.
                var absolutePath = "/" + pagePath;
                return ViewEnginePath.ResolvePath(absolutePath);
            }

            return ViewEnginePath.CombinePath(executingFilePath, pagePath);
        }

        // internal for tests
        internal IEnumerable<string> GetViewLocationFormats(ViewLocationExpanderContext context)
        {
            if (!string.IsNullOrEmpty(context.AreaName) &&
                !string.IsNullOrEmpty(context.ControllerName))
            {
                return _options.AreaViewLocationFormats;
            }
            else if (!string.IsNullOrEmpty(context.ControllerName))
            {
                return _options.ViewLocationFormats;
            }
            else if (!string.IsNullOrEmpty(context.AreaName) &&
                !string.IsNullOrEmpty(context.PageName))
            {
                return _options.AreaPageViewLocationFormats;
            }
            else if (!string.IsNullOrEmpty(context.PageName))
            {
                return _options.PageViewLocationFormats;
            }
            else
            {
                // If we don't match one of these conditions, we'll just treat it like regular controller/action
                // and use those search paths. This is what we did in 1.0.0 without giving much thought to it.
                return _options.ViewLocationFormats;
            }
        }

        private ViewLocationCacheResult OnCacheMiss(
            ViewLocationExpanderContext expanderContext,
            ViewLocationCacheKey cacheKey)
        {
            var viewLocations = GetViewLocationFormats(expanderContext);

            for (var i = 0; i < _options.ViewLocationExpanders.Count; i++)
            {
                viewLocations = _options.ViewLocationExpanders[i].ExpandViewLocations(expanderContext, viewLocations);
            }

            ViewLocationCacheResult cacheResult = null;
            var searchedLocations = new List<string>();
            var expirationTokens = new HashSet<IChangeToken>();
            foreach (var location in viewLocations)
            {
                var path = string.Format(
                    CultureInfo.InvariantCulture,
                    location,
                    expanderContext.ViewName,
                    expanderContext.ControllerName,
                    expanderContext.AreaName);

                path = ViewEnginePath.ResolvePath(path);

                cacheResult = CreateCacheResult(expirationTokens, path, expanderContext.IsMainPage);
                if (cacheResult != null)
                {
                    break;
                }

                searchedLocations.Add(path);
            }

            // No views were found at the specified location. Create a not found result.
            if (cacheResult == null)
            {
                cacheResult = new ViewLocationCacheResult(searchedLocations);
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions();
            cacheEntryOptions.SetSlidingExpiration(_cacheExpirationDuration);
            foreach (var expirationToken in expirationTokens)
            {
                cacheEntryOptions.AddExpirationToken(expirationToken);
            }

            return ViewLookupCache.Set(cacheKey, cacheResult, cacheEntryOptions);
        }

        // Internal for unit testing
        internal ViewLocationCacheResult CreateCacheResult(
            HashSet<IChangeToken> expirationTokens,
            string relativePath,
            bool isMainPage)
        {
            var factoryResult = _pageFactory.CreateFactory(relativePath);
            var viewDescriptor = factoryResult.ViewDescriptor;
            if (viewDescriptor?.ExpirationTokens != null)
            {
                for (var i = 0; i < viewDescriptor.ExpirationTokens.Count; i++)
                {
                    expirationTokens.Add(viewDescriptor.ExpirationTokens[i]);
                }
            }

            if (factoryResult.Success)
            {
                // Only need to lookup _ViewStarts for the main page.
                var viewStartPages = isMainPage ?
                    GetViewStartPages(viewDescriptor.RelativePath, expirationTokens) :
                    Array.Empty<ViewLocationCacheItem>();

                return new ViewLocationCacheResult(
                    new ViewLocationCacheItem(factoryResult.RazorPageFactory, relativePath),
                    viewStartPages);
            }

            return null;
        }

        private IReadOnlyList<ViewLocationCacheItem> GetViewStartPages(
            string path,
            HashSet<IChangeToken> expirationTokens)
        {
            var viewStartPages = new List<ViewLocationCacheItem>();

            foreach (var filePath in RazorFileHierarchy.GetViewStartPaths(path))
            {
                var result = _pageFactory.CreateFactory(filePath);
                var viewDescriptor = result.ViewDescriptor;
                if (viewDescriptor?.ExpirationTokens != null)
                {
                    for (var i = 0; i < viewDescriptor.ExpirationTokens.Count; i++)
                    {
                        expirationTokens.Add(viewDescriptor.ExpirationTokens[i]);
                    }
                }

                if (result.Success)
                {
                    // Populate the viewStartPages list so that _ViewStarts appear in the order the need to be
                    // executed (closest last, furthest first). This is the reverse order in which
                    // ViewHierarchyUtility.GetViewStartLocations returns _ViewStarts.
                    viewStartPages.Insert(0, new ViewLocationCacheItem(result.RazorPageFactory, filePath));
                }
            }

            return viewStartPages;
        }

        private ViewEngineResult CreateViewEngineResult(ViewLocationCacheResult result, string viewName)
        {
            if (!result.Success)
            {
                return ViewEngineResult.NotFound(viewName, result.SearchedLocations);
            }

            var page = result.ViewEntry.PageFactory();

            var viewStarts = new IRazorPage[result.ViewStartEntries.Count];
            for (var i = 0; i < viewStarts.Length; i++)
            {
                var viewStartItem = result.ViewStartEntries[i];
                viewStarts[i] = viewStartItem.PageFactory();
            }

            var view = new RazorView(this, _pageActivator, viewStarts, page, _htmlEncoder, _diagnosticListener);
            return ViewEngineResult.Found(viewName, view);
        }

        private static bool IsApplicationRelativePath(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            return name[0] == '~' || name[0] == '/';
        }

        private static bool IsRelativePath(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            // Though ./ViewName looks like a relative path, framework searches for that view using view locations.
            return name.EndsWith(ViewExtension, StringComparison.OrdinalIgnoreCase);
        }
    }
}
