// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    /// <summary>
    /// A <see cref="ICompilationService"/> that uses Roslyn.
    /// </summary>
    public interface IRoslynCompilationService : ICompilationService
    {
        /// <summary>
        /// Produces an <see cref="EmitResult"/> from the specified <paramref name="compilationContent"/>.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <param name="compilationContent">The content to be compiled.</param>
        /// <param name="assemblyStream">The <see cref="Stream"/> to write assemblies to.</param>
        /// <param name="pdbStream">The <see cref="Stream"/> to write pdbs to.</param>
        /// <returns>The <see cref="EmitResult"/>.</returns>
        EmitResult EmitAssembly(
            string assemblyName,
            string compilationContent,
            Stream assemblyStream,
            Stream pdbStream);
    }
}
