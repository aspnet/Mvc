// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.AspNet.Razor.Chunks;
using Microsoft.AspNet.Razor.CodeGenerators;
using Microsoft.AspNet.Razor.CodeGenerators.Visitors;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class MvcCSharpDesignTimeCodeVisitor : CSharpDesignTimeCodeVisitor
    {
        private const string ModelVariable = "__modelHelper";
        private ModelChunk _modelChunk;

        public MvcCSharpDesignTimeCodeVisitor(
            CSharpCodeVisitor csharpCodeVisitor,
            CSharpCodeWriter writer,
            CodeGeneratorContext context)
            : base(csharpCodeVisitor, writer, context)
        {
        }

        protected override void AcceptTreeCore(ChunkTree tree)
        {
            base.AcceptTreeCore(tree);

            if (_modelChunk != null)
            {
                WriteModelChunkLineMapping();
            }
        }

        public override void Accept(Chunk chunk)
        {
            if (chunk is ModelChunk)
            {
                Visit((ModelChunk)chunk);
            }

            base.Accept(chunk);
        }

        private void Visit(ModelChunk chunk)
        {
            _modelChunk = chunk;
        }

        private void WriteModelChunkLineMapping()
        {
            Debug.Assert(Context.Host.DesignTimeMode);

            using (var lineMappingWriter =
                Writer.BuildLineMapping(_modelChunk.Start, _modelChunk.ModelType.Length, Context.SourceFile))
            {
                Writer.Indent(_modelChunk.Start.CharacterIndex);

                // MyModel __modelHelper = default(MyModel);
                lineMappingWriter.MarkLineMappingStart();
                Writer.Write(_modelChunk.ModelType);
                lineMappingWriter.MarkLineMappingEnd();

                Writer.Write(" ")
                    .Write(ModelVariable)
                    .Write($" = default({_modelChunk.ModelType})");
            }
        }
    }
}
