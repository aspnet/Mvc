// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Generator.Compiler;

namespace Microsoft.AspNet.Mvc.Razor.Directives
{
    /// <summary>
    /// A <see cref="ChunkMergerBase"/> that merges chunks of type <typeparamref name="TChunk"/>.
    /// </summary>
    /// <typeparam name="TChunk">The <see cref="Chunk"/> type this instance supports.</typeparam>
    public abstract class ChunkMergerBase<TChunk> : ChunkMergerBase
        where TChunk : Chunk
    {
        public ChunkMergerBase(CodeTree codeTree)
            : base(codeTree)
        {
        }

        /// <inheritdoc />
        public override void VisitChunk(Chunk chunk)
        {
            VisitChunk((TChunk)chunk);
        }

        /// <summary>
        /// Invoked when a chunk of type <typeparamref name="TChunk"/> from the <see cref="CodeTree"/> is visited.
        /// </summary>
        /// <param name="chunk">The chunk to be visited.</param>
        protected abstract void VisitChunk(TChunk chunk);

        /// <inheritdoc />
        public override void Merge(Chunk chunkToMerge)
        {
            Merge((TChunk)chunkToMerge);
        }

        /// <summary>
        /// Invoked when a chunk of type <typeparamref name="TChunk"/> is to be merged into the 
        /// <see cref="CodeTree"/> instance.
        /// </summary>
        /// <param name="chunkToMerge">The <see cref="Chunk"/> to be merged.</param>
        protected abstract void Merge(TChunk chunkToMerge);
    }
}