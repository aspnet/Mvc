// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Generator.Compiler;

namespace Microsoft.AspNet.Mvc.Razor.Directives
{
    /// <summary>
    /// The base type that handles merging of chunks into a CodeTree.
    /// </summary>
    public abstract class ChunkMergerBase
    {
        public ChunkMergerBase([NotNull] CodeTree codeTree)
        {
            CodeTree = codeTree;
        }

        /// <summary>
        /// The <see cref="CodeTree"/> to inject chunks into.
        /// </summary>
        protected CodeTree CodeTree { get; private set; }

        /// <summary>
        /// Visits a chunk from the CodeTree.
        /// </summary>
        /// <param name="chunk"></param>
        public abstract void VisitChunk(Chunk chunk);

        /// <summary>
        /// Merges an inherited chunk into the CodeTree.
        /// </summary>
        /// <param name="chunk"></param>
        public abstract void Merge(Chunk chunk);
    }
}