// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Logging
{
    public class ViewLocationExpanderValues : LoggerStructureBase
    {
        public ViewLocationExpanderValues(IEnumerable<string> locationsToExpand, IEnumerable<string> expandedLocations)
        {
            ViewLocationsToExpand = locationsToExpand;
            ExpandedViewLocations = expandedLocations;
        }

        public IEnumerable<string> ViewLocationsToExpand { get; }

        public IEnumerable<string> ExpandedViewLocations { get; }

        public override string Format()
        {
            return LogFormatter.FormatStructure(this);
        }
    }
}