// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNet.Mvc.Razor.Compilation
{
    /// <summary>
    /// Result of <see cref="ICompilerCache"/>.
    /// </summary>
    public class CompilerCacheResult
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CompilerCacheResult"/> with the specified
        /// <see cref="CompilationResult"/>.
        /// </summary>
        /// <param name="compilationResult">The <see cref="Compilation.CompilationResult"/> </param>
        public CompilerCacheResult(CompilationResult compilationResult, IList<IChangeToken> expirationTokens)
        {
            if (compilationResult == null)
            {
                throw new ArgumentNullException(nameof(compilationResult));
            }

            if (expirationTokens == null)
            {
                throw new ArgumentNullException(nameof(expirationTokens));
            }

            CompilationResult = compilationResult;
            ExpirationTokens = expirationTokens;
        }

        public CompilerCacheResult(IList<IChangeToken> expirationTokens)
        {
            if (expirationTokens == null)
            {
                throw new ArgumentNullException(nameof(expirationTokens));
            }

            CompilationResult = null;
            ExpirationTokens = expirationTokens;
        }

        /// <summary>
        /// The <see cref="Compilation.CompilationResult"/>.
        /// </summary>
        /// <remarks>This property is null when file lookup failed.</remarks>
        public CompilationResult CompilationResult { get; }

        public IList<IChangeToken> ExpirationTokens { get; }

        public bool IsFoundResult => CompilationResult != null;
    }
}