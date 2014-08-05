// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Razor.Generator.Compiler;

namespace Microsoft.AspNet.Mvc.Razor.Directives
{
    /// <inheritdoc />
    public class InjectChunkMerger : ChunkMergerBase<InjectChunk>
    {
        private const string ModelToken = "<TModel>";
        private readonly HashSet<string> _addedMemberNames = new HashSet<string>(StringComparer.Ordinal);
        private readonly string _modelName;

        public InjectChunkMerger(CodeTree codeTree)
            : base(codeTree)
        {
            _modelName = '<' + GetModelName(codeTree) + '>';
        }

        /// <inheritdoc />
        protected override void VisitChunk(InjectChunk chunk)
        {
            _addedMemberNames.Add(chunk.MemberName);
        }

        /// <inheritdoc />
        protected override void Merge(InjectChunk chunkToMerge)
        {
            if (!_addedMemberNames.Contains(chunkToMerge.MemberName))
            {
                _addedMemberNames.Add(chunkToMerge.MemberName);
                CodeTree.Chunks.Add(TransformChunk(chunkToMerge));
            }
        }

        private Chunk TransformChunk(InjectChunk chunkToMerge)
        {
            return new InjectChunk(chunkToMerge.TypeName.Replace(ModelToken, _modelName),
                                   chunkToMerge.MemberName);
        }

        private static string GetModelName(CodeTree codeTree)
        {
            var modelName = MvcRazorHost.DefaultModel;
            var modelChunk = codeTree.Chunks
                                     .OfType<ModelChunk>()
                                     .LastOrDefault();
            if (modelChunk != null)
            {
                modelName = modelChunk.ModelType;
            }
            return modelName;
        }
    }
}