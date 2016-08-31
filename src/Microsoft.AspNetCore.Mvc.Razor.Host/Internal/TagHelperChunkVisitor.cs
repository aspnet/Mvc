// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.CodeGenerators.Visitors;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.Host.Internal
{
    public class TagHelperChunkVisitor : IChunkVisitor
    {
        private string _namespaceName { get; }
        private string _className { get; }

        public TagHelperChunkVisitor(CodeGeneratorContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _namespaceName = context.RootNamespace;
            _className = context.ClassName;
        }

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

        public void Accept(Chunk chunk)
        {
            if (chunk == null)
            {
                throw new ArgumentNullException(nameof(chunk));
            }

            var tagHelperChunk = chunk as TagHelperChunk;
            if (tagHelperChunk != null)
            {
                tagHelperChunk.Descriptors = Decorate(tagHelperChunk);
            }

            var parentChunk = chunk as ParentChunk;
            if (parentChunk != null)
            {
                Accept(parentChunk.Children);
            }
        }

        private IEnumerable<TagHelperDescriptor> Decorate(TagHelperChunk chunk)
        {
            var decoratedDescriptors = new List<TagHelperDescriptor>();

            foreach (var descriptor in chunk.Descriptors)
            {
                if (ViewComponentTagHelperDescriptorConventions.IsViewComponentDescriptor(descriptor))
                {
                    var copyDescriptor = new TagHelperDescriptor(descriptor);
                    copyDescriptor.TypeName = $"{_namespaceName}.{_className}.{descriptor.TypeName}";

                    decoratedDescriptors.Add(copyDescriptor);
                }
                else
                {
                    decoratedDescriptors.Add(descriptor);
                }
            }

            return decoratedDescriptors;
        }

        private IEnumerable<TagHelperDescriptor> GetViewComponentDescriptors(TagHelperChunk chunk) =>
            chunk.Descriptors.Where(
                descriptor => ViewComponentTagHelperDescriptorConventions.IsViewComponentDescriptor(descriptor));
    }
}