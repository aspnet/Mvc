// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Result of <see cref="IViewLocationCache"/> lookups.
    /// </summary>
    public class ViewLocationCacheResult
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ViewLocationCacheResult"/>
        /// for a view that was successfully found at the specified location.
        /// </summary>
        /// <param name="foundLocation">The view location.</param>
        /// <param name="searchedLocations">Locations that were searched
        /// in addition to <paramref name="foundLocation"/>.</param>
        public ViewLocationCacheResult(
            ViewLocationCacheItem view,
            IReadOnlyList<ViewLocationCacheItem> viewStarts)
        {
            if (viewStarts == null)
            {
                throw new ArgumentNullException(nameof(viewStarts));
            }

            ViewEntry = view;
            ViewStartEntries = viewStarts;
            IsFoundResult = true;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ViewLocationCacheResult"/> for a
        /// failed view lookup.
        /// </summary>
        /// <param name="searchedLocations">Locations that were searched.</param>
        public ViewLocationCacheResult(IEnumerable<string> searchedLocations)
        {
            if (searchedLocations == null)
            {
                throw new ArgumentNullException(nameof(searchedLocations));
            }

            SearchedLocations = searchedLocations;
        }

        public ViewLocationCacheItem ViewEntry { get; }

        public IReadOnlyList<ViewLocationCacheItem> ViewStartEntries { get; }

        /// <summary>
        /// The sequence of locations that were searched.
        /// </summary>
        /// <remarks>
        /// When <see cref="IsFoundResult"/> is <c>true</c> this includes all paths that were search prior to finding
        /// a view at <see cref="ViewLocation"/>. When <see cref="IsFoundResult"/> is <c>false</c>, this includes
        /// all search paths.
        /// </remarks>
        public IEnumerable<string> SearchedLocations { get; }

        /// <summary>
        /// Gets a value that indicates whether the view was successfully found.
        /// </summary>
        public bool IsFoundResult { get; }
    }
}
