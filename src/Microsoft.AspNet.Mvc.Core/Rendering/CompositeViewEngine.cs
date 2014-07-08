﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
            return FindView(context, partialViewName, partial: true);
        }

        /// <inheritdoc />
        public ViewEngineResult FindView([NotNull] IDictionary<string, object> context,
                                         [NotNull] string viewName)
        {
            return FindView(context, viewName, partial: false);
        }

        private ViewEngineResult FindView(IDictionary<string, object> context,
                                          string viewName,
                                          bool partial)
        {
            var searchedLocations = Enumerable.Empty<string>();
            foreach (var engine in ViewEngines)
            {
                var result = partial ? engine.FindPartialView(context, viewName) :
                                       engine.FindView(context, viewName);

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