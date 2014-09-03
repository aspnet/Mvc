// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <inheritdoc />
    public class CompositeViewEngine : ICompositeViewEngine
    {
        public CompositeViewEngine(IViewEngineProvider viewEngineProvider)
        {
            ViewEngines = viewEngineProvider.ViewEngines;
        }

        /// <summary>
        /// Gets the list of ViewEngines the CompositeViewEngine delegates to.
        /// </summary>
        public IReadOnlyList<IViewEngine> ViewEngines { get; private set; }

        /// <inheritdoc />
        public Task<ViewEngineResult> FindPartialViewAsync([NotNull] ActionContext context,
                                                           [NotNull] string partialViewName)
        {
            return FindView(context, partialViewName, partial: true);
        }

        /// <inheritdoc />
        public Task<ViewEngineResult> FindViewAsync([NotNull] ActionContext context,
                                                    [NotNull] string viewName)
        {
            return FindView(context, viewName, partial: false);
        }

        private async Task<ViewEngineResult> FindView(ActionContext context,
                                                      string viewName,
                                                      bool partial)
        {
            var searchedLocations = Enumerable.Empty<string>();
            foreach (var engine in ViewEngines)
            {
                var result = partial ? await engine.FindPartialViewAsync(context, viewName) :
                                       await engine.FindViewAsync(context, viewName);

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