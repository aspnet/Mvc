// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Mvc.Razor.Compilation
{
    /// <summary>
    /// Result of <see cref="ICompilerCache"/>.
    /// </summary>
    public class CompilerCacheResult
    {
        /// <summary>
        /// Result of <see cref="ICompilerCache"/> when the specified file does not exist in the
        /// file system.
        /// </summary>
        public static CompilerCacheResult FileNotFound { get; } = new CompilerCacheResult();

        /// <summary>
        /// Initializes a new instance of <see cref="CompilerCacheResult"/> with the specified
        /// <see cref="CompilationResult"/>.
        /// </summary>
        /// <param name="compilationResult">The <see cref="Compilation.CompilationResult"/> </param>
        public CompilerCacheResult(CompilationResult compilationResult)
        {
            if (compilationResult == null)
            {
                throw new ArgumentNullException(nameof(compilationResult));
            }

            CompilationResult = compilationResult;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CompilerCacheResult"/> for a failed file lookup.
        /// </summary>
        protected CompilerCacheResult()
        {
        }

        /// <summary>
        /// The <see cref="Compilation.CompilationResult"/>.
        /// </summary>
        /// <remarks>This property is null when file lookup failed.</remarks>
        public CompilationResult CompilationResult { get; }
    }
}