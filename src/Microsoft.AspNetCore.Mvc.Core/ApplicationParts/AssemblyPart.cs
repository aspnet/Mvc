// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    /// <summary>
    /// An <see cref="ApplicationPart"/> backed by an <see cref="Assembly"/>.
    /// </summary>
    public class AssemblyPart : ApplicationPart, IApplicationPartTypeProvider, ICompilationReferencesProvider
    {
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

        /// <inheritdoc />
        public IEnumerable<string> GetReferencePaths()
        {
            var dependencyContext = DependencyContext.Load(Assembly);
            if (dependencyContext != null)
            {
                var referencePaths = new List<string>();
                var compileLibraries = dependencyContext.CompileLibraries;

                try
                {
                    for (var i = 0; i < compileLibraries.Count; i++)
                    {
                        var compileLibrary = compileLibraries[i];
                        referencePaths.AddRange(compileLibrary.ResolveReferencePaths());
                    }

                    return referencePaths;
                }
                catch
                {
                    // ResolveReferencePaths might fail if an assembly has been compiled with preserveCompilationContext
                    // but is loaded at runtime (e.g. Assembly.Load) without the application referencing the assembly.
                    // Ignore the DependencyContext in this case and specifically reference the assembly.
                }
            }

            // If an application has been compiled without preserveCompilationContext, return the path to the assembly
            // as a reference. For runtime compilation, this will allow the compilation to succeed as long as it least
            // one application part has been compiled with preserveCompilationContext and contains a super set of types
            // required for the compilation to succeed.
            return new[] { Assembly.Location };
        }
    }
}
