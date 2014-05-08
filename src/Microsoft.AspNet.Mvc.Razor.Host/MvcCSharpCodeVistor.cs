// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;

namespace Microsoft.AspNet.Mvc.Razor
{
    public abstract class MvcCSharpCodeVisitor : CodeVisitor<CSharpCodeWriter>
    {
        public MvcCSharpCodeVisitor(CSharpCodeWriter writer, CodeGeneratorContext context)
            : base(writer, context)
        {
        }

        public override void Accept(Chunk chunk)
        {
            var injectChunk = chunk as InjectChunk;
            if (injectChunk != null)
            {
                Visit(injectChunk);
            }
            base.Accept(chunk);
        }

        protected abstract void Visit(InjectChunk chunk);
    }
}