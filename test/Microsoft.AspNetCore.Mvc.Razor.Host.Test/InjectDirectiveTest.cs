// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.AspNetCore.Razor.Evolution.Intermediate;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Host
{
    public class InjectDirectiveTest
    {
        [Fact(Skip = "aspnet/Mvc#5794")]
        public void InjectDirectivePass_Execute_DefinesProperty()
        {
            // Arrange
            var codeDocument = CreateDocument(@"
@inject PropertyType PropertyName
");

            var engine = CreateEngine();
            var pass = new InjectDirective.Pass()
            {
                Engine = engine,
            };

            var irDocument = CreateIRDocument(engine, codeDocument);

            // Act
            pass.Execute(codeDocument, irDocument);

            // Assert
            var @class = FindClassNode(irDocument);
            Assert.NotNull(@class);
            Assert.Equal(2, @class.Children.Count);

            var node = Assert.IsType<InjectDirectiveIRNode>(@class.Children[1]);
            Assert.Equal("PropertyType", node.TypeName);
            Assert.Equal("PropertyName", node.MemberName);
        }

        [Fact]
        public void InjectDirectivePass_Execute_DedupesPropertiesByName()
        {
            // Arrange
            var codeDocument = CreateDocument(@"
@inject PropertyType PropertyName
@inject PropertyType2 PropertyName
");

            var engine = CreateEngine();
            var pass = new InjectDirective.Pass()
            {
                Engine = engine,
            };

            var irDocument = CreateIRDocument(engine, codeDocument);

            // Act
            pass.Execute(codeDocument, irDocument);

            // Assert
            var @class = FindClassNode(irDocument);
            Assert.NotNull(@class);
            Assert.Equal(2, @class.Children.Count);

            var node = Assert.IsType<InjectDirectiveIRNode>(@class.Children[1]);
            Assert.Equal("PropertyType2", node.TypeName);
            Assert.Equal("PropertyName", node.MemberName);
        }

        [Fact(Skip = "aspnet/Mvc#5794")]
        public void InjectDirectivePass_Execute_ExpandsTModel_WithDynamic()
        {
            // Arrange
            var codeDocument = CreateDocument(@"
@inject PropertyType<TModel> PropertyName
");

            var engine = CreateEngine();
            var pass = new InjectDirective.Pass()
            {
                Engine = engine,
            };

            var irDocument = CreateIRDocument(engine, codeDocument);

            // Act
            pass.Execute(codeDocument, irDocument);

            // Assert
            var @class = FindClassNode(irDocument);
            Assert.NotNull(@class);
            Assert.Equal(2, @class.Children.Count);

            var node = Assert.IsType<InjectDirectiveIRNode>(@class.Children[1]);
            Assert.Equal("PropertyType<dynamic>", node.TypeName);
            Assert.Equal("PropertyName", node.MemberName);
        }

        [Fact(Skip = "aspnet/Mvc#5794")]
        public void InjectDirectivePass_Execute_ExpandsTModel_WithModelTypeFirst()
        {
            // Arrange
            var codeDocument = CreateDocument(@"
@model ModelType
@inject PropertyType<TModel> PropertyName
");

            var engine = CreateEngine();
            var pass = new InjectDirective.Pass()
            {
                Engine = engine,
            };

            var irDocument = CreateIRDocument(engine, codeDocument);

            // Act
            pass.Execute(codeDocument, irDocument);

            // Assert
            var @class = FindClassNode(irDocument);
            Assert.NotNull(@class);
            Assert.Equal(2, @class.Children.Count);

            var node = Assert.IsType<InjectDirectiveIRNode>(@class.Children[1]);
            Assert.Equal("PropertyType<ModelType>", node.TypeName);
            Assert.Equal("PropertyName", node.MemberName);
        }

        [Fact(Skip = "aspnet/Mvc#5794")]
        public void InjectDirectivePass_Execute_ExpandsTModel_WithModelType()
        {
            // Arrange
            var codeDocument = CreateDocument(@"
@inject PropertyType<TModel> PropertyName
@model ModelType
");

            var engine = CreateEngine();
            var pass = new InjectDirective.Pass()
            {
                Engine = engine,
            };

            var irDocument = CreateIRDocument(engine, codeDocument);

            // Act
            pass.Execute(codeDocument, irDocument);

            // Assert
            var @class = FindClassNode(irDocument);
            Assert.NotNull(@class);
            Assert.Equal(2, @class.Children.Count);

            var node = Assert.IsType<InjectDirectiveIRNode>(@class.Children[1]);
            Assert.Equal("PropertyType<ModelType>", node.TypeName);
            Assert.Equal("PropertyName", node.MemberName);
        }

        private RazorCodeDocument CreateDocument(string content)
        {
            var source = RazorSourceDocument.Create(content, "test.cshtml");
            return RazorCodeDocument.Create(source);
        }

        private ClassDeclarationIRNode FindClassNode(RazorIRNode node)
        {
            var visitor = new ClassNodeVisitor();
            visitor.Visit(node);
            return visitor.Node;
        }

        private RazorEngine CreateEngine()
        {
            return RazorEngine.Create(b =>
            {
                // Notice we're not registering the InjectDirective.Pass here so we can run it on demand.
                b.AddDirective(InjectDirective.Directive);
                b.AddDirective(ModelDirective.Directive);
            });
        }

        private DocumentIRNode CreateIRDocument(RazorEngine engine, RazorCodeDocument codeDocument)
        {
            for (var i = 0; i < engine.Phases.Count; i++)
            {
                var phase = engine.Phases[i];
                phase.Execute(codeDocument);

                if (phase is IRazorDocumentClassifierPhase)
                {
                    break;
                }
            }

            return codeDocument.GetIRDocument();
        }

        private string GetCSharpContent(RazorIRNode node)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i] as RazorIRToken;
                if (child.Kind == RazorIRToken.TokenKind.CSharp)
                {
                    builder.Append(child.Content);
                }
            }

            return builder.ToString();
        }

        private class ClassNodeVisitor : RazorIRNodeWalker
        {
            public ClassDeclarationIRNode Node { get; set; }

            public override void VisitClass(ClassDeclarationIRNode node)
            {
                Node = node;
            }
        }
    }
}