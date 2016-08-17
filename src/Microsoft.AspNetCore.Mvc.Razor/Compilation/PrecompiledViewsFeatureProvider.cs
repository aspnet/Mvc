// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Core.ApplicationParts;

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    public class PrecompiledViewsFeatureProvider : IApplicationFeatureProvider<PrecompiledViewsFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, PrecompiledViewsFeature feature)
        {
            foreach (var provider in parts.OfType<IPrecompiledViewsProvider>())
            {
                var precompiledViews = provider.PrecompiledViews;
                if (precompiledViews != null)
                {
                    foreach (var viewInfo in precompiledViews)
                    {
                        feature.PrecompiledViews[viewInfo.Path] = viewInfo.Type;
                    }
                }
            }
        }
    }
}
