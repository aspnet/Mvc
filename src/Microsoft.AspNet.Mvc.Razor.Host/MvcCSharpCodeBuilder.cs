// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using Microsoft.AspNet.Mvc.Razor.Directives;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class MvcCSharpCodeBuilder : CSharpCodeBuilder
    {
        private readonly GeneratedTagHelperAttributeContext _tagHelperAttributeContext;
        private readonly string _defaultModel;
        private readonly string _injectAttribute;

        public MvcCSharpCodeBuilder([NotNull] CodeBuilderContext context,
                                    [NotNull] string defaultModel,
                                    [NotNull] string injectAttribute,
                                    [NotNull] GeneratedTagHelperAttributeContext tagHelperAttributeContext)
            : base(context)
        {
            _tagHelperAttributeContext = tagHelperAttributeContext;
            _defaultModel = defaultModel;
            _injectAttribute = injectAttribute;
        }

        private string Model { get; set; }

        protected override CSharpCodeVisitor CreateCSharpCodeVisitor([NotNull] CSharpCodeWriter writer,
                                                                     [NotNull] CodeBuilderContext context)
        {
            var csharpCodeVisitor = base.CreateCSharpCodeVisitor(writer, context);

            csharpCodeVisitor.TagHelperRenderer.AttributeValueCodeRenderer =
                new MvcTagHelperAttributeValueCodeRenderer(_tagHelperAttributeContext);

            return csharpCodeVisitor;
        }

        protected override CSharpCodeWritingScope BuildClassDeclaration(CSharpCodeWriter writer)
        {
            // Grab the last model chunk so it gets intellisense.
            var modelChunk = ChunkHelper.GetModelChunk(Context.CodeTreeBuilder.CodeTree);

            Model = modelChunk != null ? modelChunk.ModelType : _defaultModel;

            // If there were any model chunks then we need to modify the class declaration signature.
            if (modelChunk != null)
            {
                writer.Write(string.Format(CultureInfo.InvariantCulture, "public class {0} : ", Context.ClassName));

                var modelVisitor = new ModelChunkVisitor(writer, Context);
                // This generates the base class signature
                modelVisitor.Accept(modelChunk);

                writer.WriteLine();

                return new CSharpCodeWritingScope(writer);
            }
            else
            {
                return base.BuildClassDeclaration(writer);
            }
        }

        protected override void BuildConstructor([NotNull] CSharpCodeWriter writer)
        {
            base.BuildConstructor(writer);

            writer.WriteLineHiddenDirective();

            var injectVisitor = new InjectChunkVisitor(writer, Context, _injectAttribute);
            injectVisitor.Accept(Context.CodeTreeBuilder.CodeTree.Chunks);

            writer.WriteLine();
            writer.WriteLineHiddenDirective();
        }
    }
}