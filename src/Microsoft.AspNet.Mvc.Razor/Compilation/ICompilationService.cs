// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Provides methods for compilation.
    /// </summary>
    public interface ICompilationService
    {
        /// <summary>
        /// Compiles content and returns the result of compilation.
        /// </summary>
        /// <param name="path">The path where the source file is located.</param>
        /// <param name="content">The contents to be compiled.</param>
        /// <returns>
        /// A CompilationResult representing the result of compilation.
        /// </returns>
        CompilationResult Compile(string path, string content);
    }
}
