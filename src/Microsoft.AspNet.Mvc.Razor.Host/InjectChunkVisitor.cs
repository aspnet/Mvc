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

        public InjectChunkVisitor(CSharpCodeWriter writer, CodeGeneratorContext context)
            : base(writer, context)
        {
        }

        public List<InjectChunk> InjectChunks
        {
            get { return _injectChunks; }
        }

        protected override void Visit(InjectChunk chunk)
        {
            Writer.WriteLine(string.Format(CultureInfo.InvariantCulture,
                                           "public {0} {1} {{ get; private set; }}",
                                            chunk.TypeName,
                                            chunk.MemberName));
            _injectChunks.Add(chunk);
        }
    }
}