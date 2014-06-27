// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class InjectChunkVisitor : MvcCSharpCodeVisitor
    {
        private readonly List<InjectChunk> _injectChunks = new List<InjectChunk>();
        private readonly string _activateAttributeName;

        public InjectChunkVisitor([NotNull] CSharpCodeWriter writer,
                                  [NotNull] CodeGeneratorContext context,
                                  string activateAttributeName)
            : base(writer, context)
        {
            _activateAttributeName = activateAttributeName;
        }

        public List<InjectChunk> InjectChunks
        {
            get { return _injectChunks; }
        }

        protected override void Visit([NotNull] InjectChunk chunk)
        {
            if (!string.IsNullOrEmpty(_activateAttributeName))
            {
                Writer.Write("[")
                      .Write(_activateAttributeName)
                      .WriteLine("]");
            }

            if (Context.Host.DesignTimeMode)
            {
                Writer.WriteLine("public");
                var code = string.Format(CultureInfo.InvariantCulture,
                                     "{0} {1}",
                                     chunk.TypeName,
                                     chunk.MemberName);
                var csharpVisitor = new CSharpCodeVisitor(Writer, Context);
                csharpVisitor.CreateExpressionCodeMapping(code, chunk);
                Writer.WriteLine("{ get; private set; }");
            }
            else
            {
                Writer.Write("public ")
                      .Write(chunk.TypeName)
                      .Write(" ")
                      .Write(chunk.MemberName)
                      .WriteLine(" { get; private set; }");
            }
            _injectChunks.Add(chunk);
        }
    }
}