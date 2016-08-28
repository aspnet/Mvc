using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.CodeGenerators.Visitors;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.Host
{
    public class TagHelperChunkVisitor : IChunkVisitor
    {
        public string NamespaceName { get; }
        public string ClassName { get; }

        public TagHelperChunkVisitor(CodeGeneratorContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            NamespaceName = context.RootNamespace;
            ClassName = context.ClassName;
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

            var parentChunk = chunk as ParentChunk;
            var tagHelperChunk = chunk as TagHelperChunk;

            if (parentChunk != null && !(parentChunk is TagHelperChunk))
            {
                var resultChunk = parentChunk;
                Accept(parentChunk.Children);
                return;
            }

            else if (tagHelperChunk != null && HasViewComponentDescriptors(tagHelperChunk))
            {
                Decorate(tagHelperChunk);
            }
        }

        public void Decorate(TagHelperChunk chunk)
        {
            if (chunk == null)
            {
                throw new ArgumentNullException(nameof(chunk));
            }

            var viewComponentDescriptors = GetViewComponentDescriptors(chunk);
            foreach (var descriptor in viewComponentDescriptors)
            {
                descriptor.TypeName = $"{NamespaceName}.{ClassName}.{descriptor.TypeName}";
            }
        }

        private bool HasViewComponentDescriptors(TagHelperChunk chunk)
        {
            var viewComponentDescriptors = GetViewComponentDescriptors(chunk);
            return (viewComponentDescriptors.Count() > 0);
        }

        private IEnumerable<TagHelperDescriptor> GetViewComponentDescriptors(TagHelperChunk chunk)
        {
            var viewComponentDescriptors = chunk.Descriptors.Where(
                descriptor => ViewComponentTagHelperDescriptorConventions.IsViewComponentDescriptor(descriptor));
            return viewComponentDescriptors;
        }
    }
}