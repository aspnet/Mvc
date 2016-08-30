// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.CodeGenerators.Visitors;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.Host
{
    /// <summary>
    /// A <see cref="IChunkVisitor"/> that decorates tag helper chunk content. 
    /// </summary>
    public class TagHelperChunkVisitor : IChunkVisitor
    {
        private string _namespaceName { get; }
        private string _className { get; }

        /// <summary>
        /// Creates a <see cref="TagHelperChunkVisitor"/>.
        /// </summary>
        /// <param name="context">The <see cref="CodeGeneratorContext"/> where
        /// the <see cref="Chunk"/>s live.</param>
        public TagHelperChunkVisitor(CodeGeneratorContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            
            _namespaceName = context.RootNamespace;
            _className = context.ClassName;
        }

        /// <summary>
        /// Accepts the selected chunks.
        /// </summary>
        /// <param name="chunks">A <see cref="IList{Chunk}"/> to accept.</param>
        public void Accept(IList<Chunk> chunks)
        {
            if (chunks == null)
            {
                throw new ArgumentNullException(nameof(chunks));
            }

            foreach (Chunk chunk in chunks)
            {
                Accept(chunk);
            }
        }

        /// <summary>
        /// Decorates the selected <see cref="Chunk"/>, if the chunk is a <see cref="TagHelperChunk"/>.
        /// If the chunk is a <see cref="ParentChunk"/>, accepts the chunk's children. 
        /// </summary>
        /// <param name="chunk">A <see cref="Chunk"/> to accept.</param>
        public void Accept(Chunk chunk)
        {
            if (chunk == null)
            {
                throw new ArgumentNullException(nameof(chunk));
            }

            var parentChunk = chunk as ParentChunk;
            var tagHelperChunk = chunk as TagHelperChunk;

            if (parentChunk != null && !(parentChunk is TagHelperChunk))
            {
                Accept(parentChunk.Children);
                return;
            }

            else if (tagHelperChunk != null && GetViewComponentDescriptors(tagHelperChunk).Count() > 0)
            {
                Decorate(tagHelperChunk);
            }
        }

        private void Decorate(TagHelperChunk chunk)
        {
            if (chunk == null)
            {
                throw new ArgumentNullException(nameof(chunk));
            }

            var viewComponentDescriptors = GetViewComponentDescriptors(chunk);
            foreach (var descriptor in viewComponentDescriptors)
            {
                descriptor.TypeName = $"{_namespaceName}.{_className}.{descriptor.TypeName}";
            }
        }

        private IEnumerable<TagHelperDescriptor> GetViewComponentDescriptors(TagHelperChunk chunk) =>
            chunk.Descriptors.Where(
                descriptor => ViewComponentTagHelperDescriptorConventions.IsViewComponentDescriptor(descriptor));
    }
}