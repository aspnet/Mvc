// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Razor.Generator.Compiler;

namespace Microsoft.AspNet.Mvc.Razor.Directives
{
    /// <inheritdoc />
    public class UsingChunkMerger : ChunkMergerBase<UsingChunk>
    {
        private readonly HashSet<string> _currentUsings = new HashSet<string>(StringComparer.Ordinal);

        public UsingChunkMerger([NotNull] CodeTree codeTree)
            : base(codeTree)
        {
        }

        /// <inheritdoc />
        protected override void VisitChunk(UsingChunk chunk)
        {
            _currentUsings.Add(chunk.Namespace);
        }

        /// <inheritdoc />
        protected override void Merge(UsingChunk chunkToMerge)
        {
            if (!_currentUsings.Contains(chunkToMerge.Namespace))
            {
                CodeTree.Chunks.Add(chunkToMerge);
            }
        }
    }
}