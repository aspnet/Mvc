// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Microsoft.AspNetCore.Mvc.Razor.Internal
{
    public class PrecompiledViewsFeatureProvider : IApplicationFeatureProvider<PrecompiledViewsFeature>
    {
        public static readonly string PrecompiledResourcePrefix = "__RazorPrecompiledView__.";
        private const string DllExtension = ".dll";
        private const string PdbExtension = ".pdb";

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, PrecompiledViewsFeature feature)
        {
            foreach (var assemblyPart in parts.OfType<AssemblyPart>())
            {
                AddPrecompiledViews(feature, assemblyPart.Assembly);
            }
        }

        private void AddPrecompiledViews(PrecompiledViewsFeature feature, Assembly assembly)
        {
            var resourceNames = new HashSet<string>(assembly.GetManifestResourceNames(), StringComparer.Ordinal);
            foreach (var resourceName in resourceNames)
            {
                if (resourceName.StartsWith(PrecompiledResourcePrefix, StringComparison.Ordinal) &&
                    resourceName.EndsWith(".dll", StringComparison.Ordinal))
                {
                    var type = ReadResource(assembly, resourceName);
                    var viewPath = resourceName.Substring(
                        PrecompiledResourcePrefix.Length,
                        resourceName.Length - PrecompiledResourcePrefix.Length - DllExtension.Length);
                    feature.PrecompiledViews[viewPath] = type;
                }
            }
        }

        private Type ReadResource(Assembly assembly, string resourceName)
        {
            var pdbResourceName = Path.ChangeExtension(resourceName, PdbExtension);

            using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var pdbStream = assembly.GetManifestResourceStream(pdbResourceName))
                {
                    return CompiledAssemblyUtility.GetExportedType(resourceStream, pdbStream);
                }
            }
        }
    }
}
