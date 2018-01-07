﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Razor.Hosting;

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    /// <summary>
    /// An <see cref="IApplicationFeatureProvider{TFeature}"/> for <see cref="ViewsFeature"/>.
    /// </summary>
    public class ViewsFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        public static readonly string PrecompiledViewsAssemblySuffix = ".PrecompiledViews";

        public static readonly IReadOnlyList<string> ViewAssemblySuffixes = new string[]
        {
            PrecompiledViewsAssemblySuffix,
            ".Views",
        };

        /// <inheritdoc />
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            foreach (var assemblyPart in parts.OfType<AssemblyPart>())
            {
                var attributes = GetViewAttributes(assemblyPart);
                var items = LoadItems(assemblyPart);

                var merged = Merge(items, attributes);
                foreach (var entry in merged)
                {
                    feature.ViewDescriptors.Add(new CompiledViewDescriptor(entry.item, entry.attribute));
                }
            }
        }

        private ICollection<(RazorCompiledItem item, RazorViewAttribute attribute)> Merge(
            IReadOnlyList<RazorCompiledItem> items, 
            IEnumerable<RazorViewAttribute> attributes)
        {
            // This code is a intentionally defensive. We assume that it's possible to have duplicates
            // of attributes, and also items that have a single kind of metadata, but not the other.
            var dictionary = new Dictionary<string, (RazorCompiledItem item, RazorViewAttribute attribute)>();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!dictionary.TryGetValue(item.Identifier, out var entry))
                {
                    dictionary.Add(item.Identifier, (item, null));

                }
                else if (entry.item == null)
                {
                    dictionary[item.Identifier] = (item, entry.attribute);
                }
            }

            foreach (var attribute in attributes)
            {
                if (!dictionary.TryGetValue(attribute.Path, out var entry))
                {
                    dictionary.Add(attribute.Path, (null, attribute));
                }
                else if (entry.attribute == null)
                {
                    dictionary[attribute.Path] = (entry.item, attribute);
                }
            }

            return dictionary.Values;
        }

        protected virtual IReadOnlyList<RazorCompiledItem> LoadItems(AssemblyPart assemblyPart)
        {
            if (assemblyPart == null)
            {
                throw new ArgumentNullException(nameof(assemblyPart));
            }

            var viewAssembly = GetViewAssembly(assemblyPart);
            if (viewAssembly != null)
            {
                var loader = new RazorCompiledItemLoader();
                return loader.LoadItems(viewAssembly);
            }

            return Array.Empty<RazorCompiledItem>();
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

            var featureAssembly = GetViewAssembly(assemblyPart);
            if (featureAssembly != null)
            {
                return featureAssembly.GetCustomAttributes<RazorViewAttribute>();
            }

            return Enumerable.Empty<RazorViewAttribute>();
        }

        private Assembly GetViewAssembly(AssemblyPart assemblyPart)
        {
            if (assemblyPart.Assembly.IsDynamic || string.IsNullOrEmpty(assemblyPart.Assembly.Location))
            {
                return null;
            }

            for (var i = 0; i < ViewAssemblySuffixes.Count; i++)
            {
                var fileName = assemblyPart.Assembly.GetName().Name + ViewAssemblySuffixes[i] + ".dll";
                var filePath = Path.Combine(Path.GetDirectoryName(assemblyPart.Assembly.Location), fileName);

                if (File.Exists(filePath))
                {
                    try
                    {
                        return Assembly.LoadFile(filePath);
                    }
                    catch (FileLoadException)
                    {
                        // Don't throw if assembly cannot be loaded. This can happen if the file is not a managed assembly.
                    }
                }
            }

            return null;
        }
    }
}
