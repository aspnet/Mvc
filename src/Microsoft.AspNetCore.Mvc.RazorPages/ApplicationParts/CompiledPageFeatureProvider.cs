﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    /// <summary>
    /// An <see cref="IApplicationFeatureProvider{TFeature}"/> for <see cref="ViewsFeature"/>.
    /// </summary>
    public class CompiledPageFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        /// <summary>
        /// Gets the namespace for the <see cref="ViewInfoContainer"/> type in the view assembly.
        /// </summary>
        public static readonly string CompiledPageManifestNamespace = "AspNetCore";

        /// <summary>
        /// Gets the type name for the view collection type in the view assembly.
        /// </summary>
        public static readonly string CompiledPageManifestTypeName = "__CompiledRazorPagesManifest";

        private static readonly string FullyQualifiedManifestTypeName =
            CompiledPageManifestNamespace + "." + CompiledPageManifestTypeName;

        /// <inheritdoc />
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            foreach (var item in GetCompiledPageDescriptors(parts))
            {
                feature.ViewDescriptors.Add(item);
            }
        }

        /// <summary>
        /// Gets the sequence of <see cref="CompiledViewDescriptor"/> from <paramref name="parts"/>.
        /// </summary>
        /// <param name="parts">The <see cref="ApplicationPart"/>s</param>
        /// <returns>The sequence of <see cref="CompiledViewDescriptor"/>.</returns>
        public static IEnumerable<CompiledViewDescriptor> GetCompiledPageDescriptors(IEnumerable<ApplicationPart> parts)
        {
            foreach (var assemblyPart in parts.OfType<AssemblyPart>())
            {
                var pageAttributes = GetAttributes(assemblyPart);
                foreach (var attribute in pageAttributes)
                {
                    var relativePath = ViewPath.NormalizePath(attribute.Path);
                    var pageDescriptor = new CompiledViewDescriptor
                    {
                        ExpirationTokens = Array.Empty<IChangeToken>(),
                        RelativePath = relativePath,
                        ViewAttribute = attribute,
                        IsPrecompiled = true,
                    };

                    yield return pageDescriptor;
                }
            }
        }

        private static IEnumerable<RazorPageAttribute> GetAttributes(AssemblyPart assemblyPart)
        {
            if (assemblyPart == null)
            {
                throw new ArgumentNullException(nameof(assemblyPart));
            }

            var featureAssembly = CompiledViewManfiest.GetFeatureAssembly(assemblyPart);
            if (featureAssembly != null)
            {
                return featureAssembly.GetCustomAttributes<RazorPageAttribute>();;
            }

            return Enumerable.Empty<RazorPageAttribute>();
        }
    }
}
