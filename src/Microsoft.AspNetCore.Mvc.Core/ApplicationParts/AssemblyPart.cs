// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Core.ApplicationParts;
using Microsoft.Extensions.DependencyModel;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    /// <summary>
    /// An <see cref="ApplicationPart"/> backed by an <see cref="Assembly"/>.
    /// </summary>
    public class AssemblyPart :
        ApplicationPart,
        IApplicationPartTypeProvider,
        ICompilationReferencesProvider,
        IPrecompiledViewsProvider
    {
        public static readonly string PrecompiledResourcePrefix = "__PrecompiledView__.";
        private const string DllExtension = ".dll";
        private const string PdbExtension = ".pdb";

        /// <summary>
        /// Initalizes a new <see cref="AssemblyPart"/> instance.
        /// </summary>
        /// <param name="assembly"></param>
        public AssemblyPart(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            Assembly = assembly;
        }

        /// <summary>
        /// Gets the <see cref="Assembly"/> of the <see cref="ApplicationPart"/>.
        /// </summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// Gets the name of the <see cref="ApplicationPart"/>.
        /// </summary>
        public override string Name => Assembly.GetName().Name;

        /// <inheritdoc />
        public IEnumerable<TypeInfo> Types => Assembly.DefinedTypes;

        public IEnumerable<PrecompiledViewInfo> PrecompiledViews
        {
            get
            {
                var precompiledViews = new List<PrecompiledViewInfo>();
                foreach (var resourceName in Assembly.GetManifestResourceNames())
                {
                    if (resourceName.StartsWith(PrecompiledResourcePrefix, StringComparison.Ordinal) &&
                        resourceName.EndsWith(DllExtension, StringComparison.Ordinal))
                    {
                        var viewPath = resourceName.Substring(
                            PrecompiledResourcePrefix.Length,
                            resourceName.Length - PrecompiledResourcePrefix.Length - DllExtension.Length);

                        var pdbStreamName = Path.ChangeExtension(viewPath, PdbExtension);
                        precompiledViews.Add(new PrecompiledViewInfo(
                            viewPath,
                            () => Assembly.GetManifestResourceStream(resourceName),
                            () => Assembly.GetManifestResourceStream(pdbStreamName)));
                    }
                }

                return precompiledViews;
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetReferencePaths()
        {
            var dependencyContext = DependencyContext.Load(Assembly);
            if (dependencyContext != null)
            {
                return dependencyContext.CompileLibraries.SelectMany(library => library.ResolveReferencePaths());
            }

            // If an application has been compiled without preserveCompilationContext, return the path to the assembly
            // as a reference. For runtime compilation, this will allow the compilation to succeed as long as it least
            // one application part has been compiled with preserveCompilationContext and contains a super set of types
            // required for the compilation to succeed.
            return new[] { Assembly.Location };
        }
    }
}
