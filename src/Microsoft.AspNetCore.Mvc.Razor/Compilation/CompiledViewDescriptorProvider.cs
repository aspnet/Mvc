// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Razor.Hosting;

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    public abstract class CompiledViewDescriptorProvider
    {
        public static CompiledViewDescriptorProvider Default { get; } = new DefaultCompiledViewDescriptorProvider();

        public abstract IEnumerable<CompiledViewDescriptor> GetCompiledViewDescriptors(Assembly assembly);

        internal class DefaultCompiledViewDescriptorProvider : CompiledViewDescriptorProvider
        {
            private static readonly IReadOnlyList<string> ViewAssemblySuffixes = new string[]
            {
                ViewsFeatureProvider.PrecompiledViewsAssemblySuffix,
                ".Views",
            };

            public override IEnumerable<CompiledViewDescriptor> GetCompiledViewDescriptors(Assembly assembly)
            {
                if (assembly == null)
                {
                    throw new ArgumentNullException(nameof(assembly));
                }

                var attributes = GetViewAttributes(assembly);
                return GetCompiledViewDescriptors(assembly, attributes);
            }

            public IEnumerable<CompiledViewDescriptor> GetCompiledViewDescriptors(
                Assembly assembly,
                IEnumerable<RazorViewAttribute> attributes)
            {
                var items = LoadItems(assembly);

                var merged = Merge(items, attributes);
                foreach (var item in merged)
                {
                    yield return new CompiledViewDescriptor(item.item, item.attribute);
                }
            }

            public virtual IReadOnlyList<RazorCompiledItem> LoadItems(Assembly assembly)
            {
                if (assembly == null)
                {
                    throw new ArgumentNullException(nameof(assembly));
                }

                var viewAssembly = GetViewAssembly(assembly);
                if (viewAssembly != null)
                {
                    var loader = new RazorCompiledItemLoader();
                    return loader.LoadItems(viewAssembly);
                }

                return Array.Empty<RazorCompiledItem>();
            }

            public virtual IEnumerable<RazorViewAttribute> GetViewAttributes(Assembly assembly)
            {
                if (assembly == null)
                {
                    throw new ArgumentNullException(nameof(assembly));
                }

                var featureAssembly = GetViewAssembly(assembly);
                if (featureAssembly != null)
                {
                    return featureAssembly.GetCustomAttributes<RazorViewAttribute>();
                }

                return Enumerable.Empty<RazorViewAttribute>();
            }

            private ICollection<(RazorCompiledItem item, RazorViewAttribute attribute)> Merge(
               IReadOnlyList<RazorCompiledItem> items,
               IEnumerable<RazorViewAttribute> attributes)
            {
                // This code is a intentionally defensive. We assume that it's possible to have duplicates
                // of attributes, and also items that have a single kind of metadata, but not the other.
                var dictionary = new Dictionary<string, (RazorCompiledItem item, RazorViewAttribute attribute)>(StringComparer.OrdinalIgnoreCase);
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

            public virtual Assembly GetViewAssembly(Assembly assembly)
            {
                if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
                {
                    return null;
                }

                for (var i = 0; i < ViewAssemblySuffixes.Count; i++)
                {
                    var fileName = assembly.GetName().Name + ViewAssemblySuffixes[i] + ".dll";
                    var filePath = Path.Combine(Path.GetDirectoryName(assembly.Location), fileName);

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
}
