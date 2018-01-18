// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Core.ApplicationParts;
using Microsoft.Extensions.DependencyModel;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    // Discovers assemblies that are part of the MVC application using the DependencyContext.
    public static class DefaultAssemblyPartDiscoveryProvider
    {
        private static readonly string PrecompiledViewsAssemblySuffix = ".PrecompiledViews";

        private static readonly IReadOnlyList<string> ViewAssemblySuffixes = new string[]
        {
            PrecompiledViewsAssemblySuffix,
            ".Views",
        };

        internal static HashSet<string> ReferenceAssemblies { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Microsoft.AspNetCore.All",
            "Microsoft.AspNetCore.Mvc",
            "Microsoft.AspNetCore.Mvc.Abstractions",
            "Microsoft.AspNetCore.Mvc.ApiExplorer",
            "Microsoft.AspNetCore.Mvc.Core",
            "Microsoft.AspNetCore.Mvc.Cors",
            "Microsoft.AspNetCore.Mvc.DataAnnotations",
            "Microsoft.AspNetCore.Mvc.Formatters.Json",
            "Microsoft.AspNetCore.Mvc.Formatters.Xml",
            "Microsoft.AspNetCore.Mvc.Localization",
            "Microsoft.AspNetCore.Mvc.Razor",
            "Microsoft.AspNetCore.Mvc.Razor.Extensions",
            "Microsoft.AspNetCore.Mvc.RazorPages",
            "Microsoft.AspNetCore.Mvc.TagHelpers",
            "Microsoft.AspNetCore.Mvc.ViewFeatures"
        };

        public static IEnumerable<ApplicationPart> DiscoverAssemblyParts(string entryPointAssemblyName)
        {
            var entryAssembly = Assembly.Load(new AssemblyName(entryPointAssemblyName));
            var context = DependencyContext.Load(entryAssembly);

            var candidateAssemblies = GetCandidateAssemblies(entryAssembly, context);
            var additionalReferences = candidateAssemblies
                .Select(ca =>
                    (assembly: ca,
                     metadata: ca.GetCustomAttributes<AssemblyMetadataAttribute>()
                        .Where(ama => ama.Key.Equals("Microsoft.AspNetCore.Mvc.AdditionalReference", StringComparison.Ordinal)).ToArray()));

            // Find out all the additional references defined by the assembly.
            // [assembly: AssemblyMetadataAttribute("Microsoft.AspNetCore.Mvc.AdditionalReference", "Library.PrecompiledViews.dll,true|false")]
            var additionalAssemblies = new List<AdditionalReference>();
            var entryAssemblyAdditionalReferences = new List<AdditionalReference>();
            foreach (var (assembly, metadata) in additionalReferences)
            {
                if (metadata.Length > 0)
                {
                    foreach (var metadataAttribute in metadata)
                    {
                        var fileName = Path.GetFileName(metadataAttribute.Value.Substring(0, metadataAttribute.Value.IndexOf(",")));
                        var filePath = Path.Combine(Path.GetDirectoryName(assembly.Location), fileName);
                        var additionalReference = new AdditionalReference
                        {
                            File = filePath,
                            IncludeByDefault = metadataAttribute.Value.Substring(metadataAttribute.Value.IndexOf(",") + 1).Equals("true")
                        };

                        additionalAssemblies.Add(additionalReference);
                        if (assembly.Equals(entryAssembly))
                        {
                            entryAssemblyAdditionalReferences.Add(additionalReference);
                        }
                    }
                }
                else
                {
                    // Fallback to loading the views like in previous versions if the additional reference metadata attribute is not present.
                    var viewsAssemblies = GetViewAssemblies(assembly);
                    foreach (var viewsAssembly in viewsAssemblies)
                    {
                        if (viewsAssembly != null)
                        {
                            var additionalReference = new AdditionalReference
                            {
                                File = null,
                                Assembly = viewsAssembly,
                                IncludeByDefault = true
                            };

                            additionalAssemblies.Add(additionalReference);
                            if (entryAssembly.Equals(assembly))
                            {
                                entryAssemblyAdditionalReferences.Add(additionalReference);
                            }
                        }
                    }
                }
            }

            // Load all the additional references.
            for (var i = 0; i < additionalAssemblies.Count; i++)
            {
                var reference = additionalAssemblies[i];
                if (reference.Assembly == null)
                {
                    var assembly = Assembly.LoadFile(reference.File);
                    reference.Assembly = assembly;
                }
            }

            // Discard any additional reference that was added by default through the dependency context.
            var result = candidateAssemblies
                .Where(ca => !additionalAssemblies.Any(ara => ara.Assembly.FullName.Equals(ca.FullName)))
                .Distinct()
                .OrderBy(assembly => entryAssembly.Equals(assembly) ? 0 : 1)
                .ThenBy(a=> a.GetName().Name)
                .ToList();

            var entryAssemblyReferences = entryAssemblyAdditionalReferences.Select(e => e.Assembly).ToList();

            // Concatenate all the candidate assemblies with all the additional references that should be loaded by default.
            var additionalParts = additionalAssemblies
                .Where(ara => ara.IncludeByDefault)
                .Select(ara => ara.Assembly)
                .OrderBy(assembly => entryAssemblyReferences.Contains(assembly) ? 0 : 1)
                .ThenBy(a=> a.GetName().Name)
                .Distinct();

            // Create the list of assembly parts.
            return result.Select(p => new AssemblyPart(p)).Concat(additionalParts.Select(ap => new AdditionalAssemblyPart(ap)));
        }

        private class AdditionalReference
        {
            public string File { get; set; }
            public Assembly Assembly { get; set; }
            public bool IncludeByDefault { get; set; }
        }

        private static IEnumerable<Assembly> GetViewAssemblies(Assembly assembly)
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
            {
                yield break;
            }

            for (var i = 0; i < ViewAssemblySuffixes.Count; i++)
            {
                var fileName = assembly.GetName().Name + ViewAssemblySuffixes[i] + ".dll";
                var filePath = Path.Combine(Path.GetDirectoryName(assembly.Location), fileName);

                if (File.Exists(filePath))
                {
                    Assembly viewsAssembly = null;
                    try
                    {
                        viewsAssembly = Assembly.LoadFile(filePath);
                    }
                    catch (FileLoadException)
                    {
                        // Don't throw if assembly cannot be loaded. This can happen if the file is not a managed assembly.
                    }
                    if (viewsAssembly != null)
                    {
                        yield return viewsAssembly;
                    }
                }
            }
        }

        internal static IEnumerable<Assembly> GetCandidateAssemblies(Assembly entryAssembly, DependencyContext dependencyContext)
        {
            if (dependencyContext == null)
            {
                // Use the entry assembly as the sole candidate.
                return new[] { entryAssembly };
            }

            return GetCandidateLibraries(dependencyContext)
                .SelectMany(library => library.GetDefaultAssemblyNames(dependencyContext))
                .Select(Assembly.Load);
        }

        // Returns a list of libraries that references the assemblies in <see cref="ReferenceAssemblies"/>.
        // By default it returns all assemblies that reference any of the primary MVC assemblies
        // while ignoring MVC assemblies.
        // Internal for unit testing
        internal static IEnumerable<RuntimeLibrary> GetCandidateLibraries(DependencyContext dependencyContext)
        {
            if (ReferenceAssemblies == null)
            {
                return Enumerable.Empty<RuntimeLibrary>();
            }

            var candidatesResolver = new CandidateResolver(dependencyContext.RuntimeLibraries, ReferenceAssemblies);
            return candidatesResolver.GetCandidates();
        }

        private class CandidateResolver
        {
            private readonly IDictionary<string, Dependency> _runtimeDependencies;

            public CandidateResolver(IReadOnlyList<RuntimeLibrary> runtimeDependencies, ISet<string> referenceAssemblies)
            {
                var dependenciesWithNoDuplicates = new Dictionary<string, Dependency>(StringComparer.OrdinalIgnoreCase);
                foreach (var dependency in runtimeDependencies)
                {
                    if (dependenciesWithNoDuplicates.ContainsKey(dependency.Name))
                    {
                        throw new InvalidOperationException(Resources.FormatCandidateResolver_DifferentCasedReference(dependency.Name));
                    }
                    dependenciesWithNoDuplicates.Add(dependency.Name, CreateDependency(dependency, referenceAssemblies));
                }

                _runtimeDependencies = dependenciesWithNoDuplicates;
            }

            private Dependency CreateDependency(RuntimeLibrary library, ISet<string> referenceAssemblies)
            {
                var classification = DependencyClassification.Unknown;
                if (referenceAssemblies.Contains(library.Name))
                {
                    classification = DependencyClassification.MvcReference;
                }

                return new Dependency(library, classification);
            }

            private DependencyClassification ComputeClassification(string dependency)
            {
                if (!_runtimeDependencies.ContainsKey(dependency))
                {
                    // Library does not have runtime dependency. Since we can't infer
                    // anything about it's references, we'll assume it does not have a reference to Mvc.
                    return DependencyClassification.DoesNotReferenceMvc;
                }

                var candidateEntry = _runtimeDependencies[dependency];
                if (candidateEntry.Classification != DependencyClassification.Unknown)
                {
                    return candidateEntry.Classification;
                }
                else
                {
                    var classification = DependencyClassification.DoesNotReferenceMvc;
                    foreach (var candidateDependency in candidateEntry.Library.Dependencies)
                    {
                        var dependencyClassification = ComputeClassification(candidateDependency.Name);
                        if (dependencyClassification == DependencyClassification.ReferencesMvc ||
                            dependencyClassification == DependencyClassification.MvcReference)
                        {
                            classification = DependencyClassification.ReferencesMvc;
                            break;
                        }
                    }

                    candidateEntry.Classification = classification;

                    return classification;
                }
            }

            public IEnumerable<RuntimeLibrary> GetCandidates()
            {
                foreach (var dependency in _runtimeDependencies)
                {
                    if (ComputeClassification(dependency.Key) == DependencyClassification.ReferencesMvc)
                    {
                        yield return dependency.Value.Library;
                    }
                }
            }

            private class Dependency
            {
                public Dependency(RuntimeLibrary library, DependencyClassification classification)
                {
                    Library = library;
                    Classification = classification;
                }

                public RuntimeLibrary Library { get; }

                public DependencyClassification Classification { get; set; }

                public override string ToString()
                {
                    return $"Library: {Library.Name}, Classification: {Classification}";
                }
            }

            private enum DependencyClassification
            {
                Unknown = 0,

                /// <summary>
                /// References (directly or transitively) one of the Mvc packages listed in
                /// <see cref="ReferenceAssemblies"/>.
                /// </summary>
                ReferencesMvc = 1,

                /// <summary>
                /// Does not reference (directly or transitively) one of the Mvc packages listed by
                /// <see cref="ReferenceAssemblies"/>.
                /// </summary>
                DoesNotReferenceMvc = 2,

                /// <summary>
                /// One of the references listed in <see cref="ReferenceAssemblies"/>.
                /// </summary>
                MvcReference = 3,
            }
        }
    }
}
