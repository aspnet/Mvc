﻿using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;

namespace Microsoft.AspNet.Mvc.Razor
{
    public abstract class MvcCSharpChunkVisitor : CodeVisitor<CSharpCodeWriter>
    {
        public MvcCSharpChunkVisitor([NotNull] CSharpCodeWriter writer,
                                     [NotNull] CodeGeneratorContext context)
            : base(writer, context)
        { }

        public override void Accept(Chunk chunk)
        {
            if (chunk is InjectChunk)
            {
                Visit((InjectChunk)chunk);
            }
            else if (chunk is ModelChunk)
            {
                Visit((ModelChunk)chunk);
            }
            else
            {
                base.Accept(chunk);
            }
        }

        protected abstract void Visit(InjectChunk chunk);
        protected abstract void Visit(ModelChunk chunk);
    }
}