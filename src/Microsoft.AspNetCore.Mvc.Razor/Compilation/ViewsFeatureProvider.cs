// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using static Microsoft.AspNetCore.Mvc.Razor.Compilation.CompiledViewDescriptorProvider;

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    /// <summary>
    /// An <see cref="IApplicationFeatureProvider{TFeature}"/> for <see cref="ViewsFeature"/>.
    /// </summary>
    public class ViewsFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        private const string CompiledViewsLoadBehavior = "Microsoft.AspNetCore.Mvc.Razor.CompiledViewsLoadBehavior";
        public static readonly string PrecompiledViewsAssemblySuffix = ".PrecompiledViews";
        private readonly DefaultCompiledViewDescriptorProvider _compiledViewDescriptorProvider;

        public ViewsFeatureProvider()
            : this(new DefaultCompiledViewDescriptorProvider())
        {
        }

        // Internal for unit testing
        internal ViewsFeatureProvider(DefaultCompiledViewDescriptorProvider compiledViewDescriptorProvider)
        {
            _compiledViewDescriptorProvider = compiledViewDescriptorProvider;
        }

        /// <inheritdoc />
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            var knownIdentifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var descriptors = new List<CompiledViewDescriptor>();
            foreach (var assemblyPart in parts.OfType<AssemblyPart>())
            {
                if (!HasDefaultCompiledViewsLoadBehavior(assemblyPart))
                {
                    continue;
                }

                var attributes = GetViewAttributes(assemblyPart);
                var compiledViewDescriptors = _compiledViewDescriptorProvider.GetCompiledViewDescriptors(
                    assemblyPart.Assembly,
                    attributes);
                foreach (var descriptor in compiledViewDescriptors)
                {
                    // We iterate through ApplicationPart instances that appear in precendence order.
                    // If a view path appears in multiple parts, we'll use the order to break ties.
                    if (knownIdentifiers.Add(descriptor.RelativePath))
                    {
                        feature.ViewDescriptors.Add(descriptor);
                    }
                }
            }
        }

        private bool HasDefaultCompiledViewsLoadBehavior(AssemblyPart assemblyPart)
        {
            var assemblyMetadata = assemblyPart.Assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
            var loadBehavior = assemblyMetadata.FirstOrDefault(m => string.Equals(CompiledViewsLoadBehavior, m.Key, StringComparison.Ordinal));

            return loadBehavior == null ||
                string.Equals("default", loadBehavior.Value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the sequence of <see cref="RazorViewAttribute"/> instances associated with the specified <paramref name="assemblyPart"/>.
        /// </summary>
        /// <param name="assemblyPart">The <see cref="AssemblyPart"/>.</param>
        /// <returns>The sequence of <see cref="RazorViewAttribute"/> instances.</returns>
        protected virtual IEnumerable<RazorViewAttribute> GetViewAttributes(AssemblyPart assemblyPart)
        {
            if (assemblyPart == null)
            {
                throw new ArgumentNullException(nameof(assemblyPart));
            }

            return _compiledViewDescriptorProvider.GetViewAttributes(assemblyPart.Assembly);
        }
    }
}
