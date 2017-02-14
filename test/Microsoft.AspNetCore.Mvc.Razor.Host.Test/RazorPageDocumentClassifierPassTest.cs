﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.AspNetCore.Razor.Evolution.Intermediate;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Host
{
    public class RazorPageDocumentClassifierPassTest
    {
        [Fact]
        public void RazorPageDocumentClassifierPass_SetsDocumentKind()
        {
            // Arrange
            var codeDocument = CreateDocument("@page");
            var engine = CreateEngine();
            var irDocument = CreateIRDocument(engine, codeDocument);
            var pass = new RazorPageDocumentClassifierPass
            {
                Engine = engine
            };

            // Act
            pass.Execute(codeDocument, irDocument);

            // Assert
            Assert.Equal("mvc.1.0.razor-page", irDocument.DocumentKind);
        }

        [Fact]
        public void RazorPageDocumentClassifierPass_NoOpsIfDocumentKindIsAlreadySet()
        {
            // Arrange
            var codeDocument = CreateDocument("@page");
            var engine = CreateEngine();
            var irDocument = CreateIRDocument(engine, codeDocument);
            irDocument.DocumentKind = "some-value";
            var pass = new RazorPageDocumentClassifierPass
            {
                Engine = engine
            };

            // Act
            pass.Execute(codeDocument, irDocument);

            // Assert
            Assert.Equal("some-value", irDocument.DocumentKind);
        }

        [Fact]
        public void RazorPageDocumentClassifierPass_NoOpsIfPageDirectiveIsMalformed()
        {
            // Arrange
            var codeDocument = CreateDocument("@page+1");
            var engine = CreateEngine();
            var irDocument = CreateIRDocument(engine, codeDocument);
            irDocument.DocumentKind = "some-value";
            var pass = new RazorPageDocumentClassifierPass
            {
                Engine = engine
            };

            // Act
            pass.Execute(codeDocument, irDocument);

            // Assert
            Assert.Equal("some-value", irDocument.DocumentKind);
        }

        [Fact]
        public void RazorPageDocumentClassifierPass_SetsNamespace()
        {
            // Arrange
            var codeDocument = CreateDocument("@page");
            var engine = CreateEngine();
            var irDocument = CreateIRDocument(engine, codeDocument);
            var pass = new RazorPageDocumentClassifierPass
            {
                Engine = engine
            };

            // Act
            pass.Execute(codeDocument, irDocument);
            var visitor = new Visitor();
            visitor.Visit(irDocument);

            // Assert
            Assert.Equal("AspNetCore", visitor.Namespace.Content);
        }

        [Fact]
        public void RazorPageDocumentClassifierPass_SetsClass()
        {
            // Arrange
            var codeDocument = CreateDocument("@page");
            var engine = CreateEngine();
            var irDocument = CreateIRDocument(engine, codeDocument);
            var pass = new RazorPageDocumentClassifierPass
            {
                Engine = engine
            };
            codeDocument.SetRelativePath("Test.cshtml");

            // Act
            pass.Execute(codeDocument, irDocument);
            var visitor = new Visitor();
            visitor.Visit(irDocument);

            // Assert
            Assert.Equal("global::Microsoft.AspNetCore.Mvc.RazorPages.Page", visitor.Class.BaseType);
            Assert.Equal("public", visitor.Class.AccessModifier);
            Assert.Equal("Test_cshtml", visitor.Class.Name);
        }

        [Theory]
        [InlineData("/Views/Home/Index.cshtml", "_Views_Home_Index_cshtml")]
        [InlineData("/Areas/MyArea/Views/Home/About.cshtml", "_Areas_MyArea_Views_Home_About_cshtml")]
        public void RazorPageDocumentClassifierPass_UsesRelativePathToGenerateTypeName(string relativePath, string expected)
        {
            // Arrange
            var codeDocument = CreateDocument("@page");
            codeDocument.SetRelativePath(relativePath);
            var engine = CreateEngine();
            var irDocument = CreateIRDocument(engine, codeDocument);
            var pass = new RazorPageDocumentClassifierPass
            {
                Engine = engine
            };

            // Act
            pass.Execute(codeDocument, irDocument);
            var visitor = new Visitor();
            visitor.Visit(irDocument);

            // Assert
            Assert.Equal(expected, visitor.Class.Name);
        }

        [Fact]
        public void RazorPageDocumentClassifierPass_UsesAbsolutePath_IfRelativePathIsNotSet()
        {
            // Arrange
            var expected = "x___application_Views_Home_Index_cshtml";
            var path = @"x::\application\Views\Home\Index.cshtml";
            var codeDocument = CreateDocument("@page", path);
            var engine = CreateEngine();
            var irDocument = CreateIRDocument(engine, codeDocument);
            var pass = new RazorPageDocumentClassifierPass
            {
                Engine = engine
            };

            // Act
            pass.Execute(codeDocument, irDocument);
            var visitor = new Visitor();
            visitor.Visit(irDocument);

            // Assert
            Assert.Equal(expected, visitor.Class.Name);
        }

        [Fact]
        public void RazorPageDocumentClassifierPass_SanitizesClassName()
        {
            // Arrange
            var expected = "path_with_invalid_chars";
            var codeDocument = CreateDocument("@page");
            codeDocument.SetRelativePath("path.with+invalid-chars");
            var engine = CreateEngine();
            var irDocument = CreateIRDocument(engine, codeDocument);
            var pass = new RazorPageDocumentClassifierPass
            {
                Engine = engine
            };

            // Act
            pass.Execute(codeDocument, irDocument);
            var visitor = new Visitor();
            visitor.Visit(irDocument);

            // Assert
            Assert.Equal(expected, visitor.Class.Name);
        }

        [Fact]
        public void RazorPageDocumentClassifierPass_SetsUpExecuteAsyncMethod()
        {
            // Arrange
            var codeDocument = CreateDocument("@page");
            var engine = CreateEngine();
            var irDocument = CreateIRDocument(engine, codeDocument);
            var pass = new RazorPageDocumentClassifierPass
            {
                Engine = engine
            };

            // Act
            pass.Execute(codeDocument, irDocument);
            var visitor = new Visitor();
            visitor.Visit(irDocument);

            // Assert
            Assert.Equal("ExecuteAsync", visitor.Method.Name);
            Assert.Equal("public", visitor.Method.AccessModifier);
            Assert.Equal("global::System.Threading.Tasks.Task", visitor.Method.ReturnType);
            Assert.Equal(new[] { "async", "override" }, visitor.Method.Modifiers);
        }

        private static RazorCodeDocument CreateDocument(string content, string filePath = null)
        {
            filePath = filePath ?? Path.Combine(Directory.GetCurrentDirectory(), "Test.cshtml");

            var bytes = Encoding.UTF8.GetBytes(content);
            using (var stream = new MemoryStream(bytes))
            {
                var source = RazorSourceDocument.ReadFrom(stream, filePath);
                return RazorCodeDocument.Create(source);
            }
        }

        private static RazorEngine CreateEngine()
        {
            return RazorEngine.Create(b =>
            {
                PageDirective.Register(b);
            });
        }

        private static DocumentIRNode CreateIRDocument(RazorEngine engine, RazorCodeDocument codeDocument)
        {
            var phases = engine.Phases.TakeWhile(p => !(p is IRazorIRPhase));

            foreach (var phase in phases)
            {
                phase.Execute(codeDocument);
            }

            return codeDocument.GetIRDocument();
        }

        private class Visitor : RazorIRNodeWalker
        {
            public NamespaceDeclarationIRNode Namespace { get; private set; }

            public ClassDeclarationIRNode Class { get; private set; }

            public RazorMethodDeclarationIRNode Method { get; private set; }

            public override void VisitRazorMethodDeclaration(RazorMethodDeclarationIRNode node)
            {
                Method = node;
            }

            public override void VisitNamespace(NamespaceDeclarationIRNode node)
            {
                Namespace = node;
                base.VisitNamespace(node);
            }

            public override void VisitClass(ClassDeclarationIRNode node)
            {
                Class = node;
                base.VisitClass(node);
            }
        }
    }
}
