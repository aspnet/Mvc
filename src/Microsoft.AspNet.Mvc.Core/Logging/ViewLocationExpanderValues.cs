// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
// for doc comments
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    /// <summary>
    /// Represents the before/after state of view expansion. Logged when an <see cref="IViewEngine"/> did not find 
    /// a cached view and has to determine potential locations for a view.
    /// </summary>
    public class ViewLocationExpanderValues : LoggerStructureBase
    {
        public ViewLocationExpanderValues(IEnumerable<string> locationsToExpand, IEnumerable<string> expandedLocations)
        {
            ViewLocationsToExpand = locationsToExpand;
            ExpandedViewLocations = expandedLocations;
        }

        /// <summary>
        /// An enumerable of the view locations to be expanded.
        /// </summary>
        public IEnumerable<string> ViewLocationsToExpand { get; }

        /// <summary>
        /// An enumerable of the expanded view locations.
        /// </summary>
        public IEnumerable<string> ExpandedViewLocations { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}