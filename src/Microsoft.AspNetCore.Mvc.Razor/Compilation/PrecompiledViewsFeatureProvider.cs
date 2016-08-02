// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Core.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    public class PrecompiledViewsFeatureProvider : IApplicationFeatureProvider<PrecompiledViewsFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, PrecompiledViewsFeature feature)
        {
            foreach (var provider in parts.OfType<IPrecompiledViewsProvider>())
            {
                var precompiledViews = provider.PrecompiledViews;
                foreach (var viewInfo in precompiledViews)
                {
                    AddView(feature, viewInfo);
                }
            }
        }

        private void AddView(PrecompiledViewsFeature feature, PrecompiledViewInfo viewInfo)
        {
            using (var assemblyStream = viewInfo.AssemblyStreamFactory())
            {
                using (var pdbStream = viewInfo.PdbStreamFactory?.Invoke())
                {
                    var type = RazorAssemblyLoader.GetExportedType(assemblyStream, pdbStream);
                    feature.PrecompiledViews[viewInfo.Path] = type;
                }
            }
        }
    }
}
