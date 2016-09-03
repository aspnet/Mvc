// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Chunks;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.CodeGenerators.Visitors;
using Microsoft.AspNetCore.Razor.Compilation.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.Razor.Host.Internal
{
    public class TagHelperChunkDecorator : CodeVisitor<CSharpCodeWriter>
    {
        private readonly string _className;
        private readonly string _namespaceName;

        public TagHelperChunkDecorator(CodeGeneratorContext context)
            : base(new CSharpCodeWriter(), context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _namespaceName = context.RootNamespace;
            _className = context.ClassName;
        }

        public override void Accept(Chunk chunk)
        {
            if (chunk == null)
            {
                throw new ArgumentNullException(nameof(chunk));
            }

            var tagHelperChunk = chunk as TagHelperChunk;
            if (tagHelperChunk != null)
            {
                tagHelperChunk.Descriptors = Decorate(tagHelperChunk.Descriptors);
            }

            base.Accept(chunk);
        }

        protected override void Visit(ParentChunk parentChunk)
        {
            foreach (var chunk in parentChunk.Children)
            {
                Accept(chunk);
            }
        }

        private IEnumerable<TagHelperDescriptor> Decorate(IEnumerable<TagHelperDescriptor> descriptors)
        {
            var decoratedDescriptors = new List<TagHelperDescriptor>();

            foreach (var descriptor in descriptors)
            {
                if (ViewComponentTagHelperDescriptorConventions.IsViewComponentDescriptor(descriptor))
                {
                    var decoratedDescriptor = new TagHelperDescriptor(descriptor);
                    decoratedDescriptor.TypeName = $"{_namespaceName}.{_className}.{descriptor.TypeName}";

                    decoratedDescriptors.Add(decoratedDescriptor);
                }
                else
                {
                    decoratedDescriptors.Add(descriptor);
                }
            }

            return decoratedDescriptors;
        }
    }
}