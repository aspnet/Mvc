// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.FileSystems;

namespace Microsoft.AspNet.Mvc.Razor
{
    /// <summary>
    /// Provides methods for compilation of a Razor page.
    /// </summary>
    public interface ICompilationService
    {
        /// <summary>
        /// Compiles content and returns the result of compilation.
        /// </summary>
        /// <param name="path">The path where the source file is located.</param>
        /// <param name="compilationContent">The contents to be compiled.</param>
        /// <param name="pageContent">The contents of the page.</param>
        /// <returns>
        /// A <see cref="CompilationResult"/> representing the result of compilation.
        /// </returns>
        CompilationResult Compile(IFileInfo fileInfo, string compilationContent);
    }
}
