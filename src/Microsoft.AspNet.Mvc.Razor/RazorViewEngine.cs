// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Represents a view engine that is used to render a page that uses the Razor syntax.
    /// </summary>
    public class RazorViewEngine : IViewEngine
    {
        private const string ViewExtension = ".cshtml";

        private static readonly IImmutableList<string> _viewLocationFormats = ImmutableList.Create(
            "/views/{controller}/{view}" + ViewExtension,
            "/views/shared/{view}" + ViewExtension
        );

        private static readonly IImmutableList<string> _areaViewLocationFormats = ImmutableList.Create(
            "/areas/{area}/views/{controller}/{view}" + ViewExtension,
            "/areas/{area}/views/shared/{view}" + ViewExtension,
            "/views/shared/{view}" + ViewExtension
        );

        private readonly IRazorPageFactory _pageFactory;
        private readonly IReadOnlyList<IViewLocationExpander> _viewLocationExpanders;
        private readonly IViewLocationCache _viewLocationCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="RazorViewEngine" /> class.
        /// </summary>
        /// <param name="pageFactory">The page factory used for creating <see cref="IRazorPage"/> instances.</param>
        public RazorViewEngine(IRazorPageFactory pageFactory,
                               IViewLocationExpanderProvider viewLocationExpanderProvider,
                               IViewLocationCache viewLocationCache)
        {
            _pageFactory = pageFactory;
            _viewLocationExpanders = viewLocationExpanderProvider.ViewLocationExpanders;
            _viewLocationCache = viewLocationCache;
        }

        public IEnumerable<string> ViewLocationFormats
        {
            get { return _viewLocationFormats; }
        }

        /// <inheritdoc />
        public Task<ViewEngineResult> FindViewAsync([NotNull] ActionContext context,
                                                    [NotNull] string viewName)
        {
            var viewEngineResult = CreateViewEngineResult(context, viewName, partial: false);
            return viewEngineResult;
        }

        /// <inheritdoc />
        public Task<ViewEngineResult> FindPartialViewAsync([NotNull] ActionContext context,
                                                           [NotNull] string partialViewName)
        {
            return CreateViewEngineResult(context, partialViewName, partial: true);
        }

        private async Task<ViewEngineResult> CreateViewEngineResult(ActionContext context,
                                                                    string viewName,
                                                                    bool partial)
        {
            var nameRepresentsPath = IsSpecificPath(viewName);

            if (nameRepresentsPath)
            {
                if (viewName.EndsWith(ViewExtension, StringComparison.OrdinalIgnoreCase))
                {
                    var page = _pageFactory.CreateInstance(viewName);
                    if (page != null)
                    {
                        return CreateFoundResult(context, page, viewName, partial);
                    }
                }
                return ViewEngineResult.NotFound(viewName, new[] { viewName });
            }
            else
            {
                return await LocateViewFromViewLocations(context, viewName, partial);
            }
        }

        private async Task<ViewEngineResult> LocateViewFromViewLocations(ActionContext context,
                                                                         string viewName,
                                                                         bool partial)
        {
            var routeValues = context.RouteData.Values;
            var controllerName = routeValues.GetValueOrDefault<string>("controller");
            var areaName = routeValues.GetValueOrDefault<string>("area");

            var values = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                { "area", areaName },
                { "controller", controllerName },
                { "view", viewName },
            };

            // 1. Populate values from viewLocationExpanders.
            foreach (var expander in _viewLocationExpanders)
            {
                await expander.PopulateValuesAsync(context, values);
            }

            // 2. Use the poulated dictionary to look up a view path in the cache.
            var result = _viewLocationCache.Get(values);
            if (!string.IsNullOrEmpty(result.Value) &&
                (var page = _pageFactory.CreateInstance(result.Value)) != null)
            {
                // 2a. We found a IRazorPage at the specified location.
                return CreateFoundResult(context, page, viewName, partial);
            }

            // 2b. We did not find a cached location or did not find a IRazorPage at the cached location.
            // Only use the area view location formats if we have an area token.
            var viewPaths = !string.IsNullOrEmpty(areaName) ? _areaViewLocationFormats : _viewLocationFormats;
            foreach (var expander in _viewLocationExpanders)
            {
                viewPaths = expander.ExpandViewLocations(viewPaths, values);
            }

            // 3. Expand the view locations
            foreach (var path in viewPaths)
            {
                var page = _pageFactory.CreateInstance(path);
                if (page != null)
                {
                    _viewLocationCache.Set(result.Key, path);
                    return CreateFoundResult(context, page, path, partial);
                }
            }

            return ViewEngineResult.NotFound(viewName, viewPaths);
        }

        private ViewEngineResult CreateFoundResult(ActionContext actionContext,
                                                   IRazorPage page,
                                                   string viewName,
                                                   bool partial)
        {
            // A single request could result in creating multiple IRazorView instances (for partials, view components)
            // and might store state. We'll use the service container to create new instances as we require.

            var services = actionContext.HttpContext.RequestServices;
            var view = services.GetService<IRazorView>();
            view.Contextualize(page, partial);
            return ViewEngineResult.Found(viewName, view);
        }

        private static bool IsSpecificPath(string name)
        {
            return name[0] == '~' || name[0] == '/';
        }
    }
}
