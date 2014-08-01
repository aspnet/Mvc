// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator.Compiler;

namespace Microsoft.AspNet.Mvc.Razor
{
    public interface IMvcRazorHost
    {
        GeneratorResults GenerateCode(string rootRelativePath, Stream inputStream);

        /// <summary>
        /// Parses the contents represented by the stream and provides a list of 
        /// <see cref="Chunk"/> for the parsed content.
        /// </summary>
        /// <param name="rootRelativePath">The path of the file relative to the root of application.</param>
        /// <param name="inputStream">The stream to parse.</param>
        /// <returns>A list of Chunks parsed from the tree.</returns>
        IList<Chunk> GetCodeTree(string rootRelativePath, Stream inputStream);
    }
}
