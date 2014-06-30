// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <inheritdoc />
    public class CompositeViewEngine : ICompositeViewEngine
    {

        private readonly IViewEnginesProvider _viewEnginesProvider;
        private IReadOnlyList<IViewEngine> _viewEngines;

        public CompositeViewEngine(IViewEnginesProvider viewEnginesProvider)
        {
            _viewEnginesProvider = viewEnginesProvider;
        }

        /// <summary>
        /// Gets the list of ViewEngines the CompositeViewEngine delegates to.
        /// </summary>
        public IReadOnlyList<IViewEngine> ViewEngines
        {
            get
            {
                if (_viewEngines == null)
                {
                    _viewEngines = _viewEnginesProvider.ViewEngines;
                }

                return _viewEngines;
            }
        }

        /// <inheritdoc />
        public ViewEngineResult FindPartialView([NotNull] IDictionary<string, object> context,
                                                [NotNull] string partialViewName)
        {
            return FindView(engine => engine.FindPartialView(context, partialViewName),
                            partialViewName);
        }

        /// <inheritdoc />
        public ViewEngineResult FindView([NotNull] IDictionary<string, object> context,
                                         [NotNull] string viewName)
        {
            return FindView(engine => engine.FindView(context, viewName),
                            viewName);
        }

        private ViewEngineResult FindView(Func<IViewEngine, ViewEngineResult> lookup,
                                          string viewName)
        {
            var searchedLocations = Enumerable.Empty<string>();
            foreach (var engine in ViewEngines)
            {
                var result = lookup(engine);

                if (result.Success)
                {
                    return result;
                }
                searchedLocations = searchedLocations.Concat(result.SearchedLocations);
            }

            return ViewEngineResult.NotFound(viewName, searchedLocations);
        }
    }
}