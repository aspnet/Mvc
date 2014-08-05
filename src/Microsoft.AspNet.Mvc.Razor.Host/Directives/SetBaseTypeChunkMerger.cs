// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Generator.Compiler;

namespace Microsoft.AspNet.Mvc.Razor.Directives
{
    /// <inheritdoc />
    public class SetBaseTypeChunkMerger : ChunkMergerBase<SetBaseTypeChunk>
    {
        private bool _isBaseTypeSet;

        public SetBaseTypeChunkMerger(CodeTree codeTree)
            : base(codeTree)
        {
        }

        /// <inheritdoc />
        protected override void VisitChunk(SetBaseTypeChunk chunk)
        {
            _isBaseTypeSet = true;
        }

        /// <inheritdoc />
        protected override void Merge(SetBaseTypeChunk chunkToMerge)
        {
            if (!_isBaseTypeSet)
            {
                // The base type can set exactly once and the closest one we encounter wins.
                _isBaseTypeSet = true;
                CodeTree.Chunks.Add(chunkToMerge);
            }
        }
    }
}