﻿using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.Text;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class InjectChunkVisitorTest
    {
        [Fact]
        public void Visit_IgnoresNonInjectChunks()
        {
            // Arrange
            var writer = new CSharpCodeWriter();
            var context = CreateContext();

            var visitor = new InjectChunkVisitor(writer, context);

            // Act
            visitor.Accept(new Chunk[] { new LiteralChunk(),
                                         new CodeAttributeChunk() });
            var code = writer.GenerateCode();

            // Assert
            Assert.Empty(code);
        }

        [Fact]
        public void Visit_GeneratesProperties_ForInjectChunks()
        {
            // Arrange
            var expected =
@"public MyType1 MyPropertyName1 { get; private set; }
public MyType2 @MyPropertyName2 { get; private set; }
";
            var writer = new CSharpCodeWriter();
            var context = CreateContext();

            var visitor = new InjectChunkVisitor(writer, context);
            var factory = SpanFactory.CreateCsHtml();
            var node = (Span)factory.Code("Some code")
                                    .As(new InjectParameterGenerator("MyType", "MyPropertyName"));

            // Act
            visitor.Accept(new Chunk[]
            {
                new LiteralChunk(),
                new InjectChunk("MyType1", "MyPropertyName1") { Association = node },
                new InjectChunk("MyType2", "@MyPropertyName2") { Association = node }
            });
            var code = writer.GenerateCode();

            // Assert
            Assert.Equal(expected, code);
        }

        [Fact]
        public void Visit_WithDesignTimeHost_GeneratesPropertiesAndLinePragmas_ForInjectChunks()
        {
            // Arrange
            var expected = @"public
#line 1 """"
MyType1 MyPropertyName1

#line default
#line hidden
{ get; private set; }
public
#line 1 """"
MyType2 @MyPropertyName2

#line default
#line hidden
{ get; private set; }
";
            var writer = new CSharpCodeWriter();
            var context = CreateContext();
            context.Host.DesignTimeMode = true;

            var visitor = new InjectChunkVisitor(writer, context);
            var factory = SpanFactory.CreateCsHtml();
            var node = (Span)factory.Code("Some code")
                                    .As(new InjectParameterGenerator("MyType", "MyPropertyName"));

            // Act
            visitor.Accept(new Chunk[]
            {
                new LiteralChunk(),
                new InjectChunk("MyType1", "MyPropertyName1") { Association = node },
                new InjectChunk("MyType2", "@MyPropertyName2") { Association = node }
            });
            var code = writer.GenerateCode();

            // Assert
            Assert.Equal(expected, code);
        }

        [Fact]
        public void InjectVisitor_GeneratesCorrectLineMappings()
        {
            // Arrange
            var host = new MvcRazorHost("RazorView")
            {
                DesignTimeMode = true
            };
            host.NamespaceImports.Clear();
            var engine = new RazorTemplateEngine(host);
            var source = ReadResource("Inject.cshtml");
            var expectedCode = ReadResource("Inject.cs");
            var expectedLineMappings = new List<LineMapping>
            {
                BuildLineMapping(1, 0, 1, 32, 3, 0, 17),
                BuildLineMapping(28, 1, 8, 442, 21, 8, 20)
            };

            // Act
            GeneratorResults results = null;
            using (var buffer = new StringTextBuffer(source))
            {
                results = engine.GenerateCode(buffer);
            }

            // Assert
            Assert.True(results.Success);
            Assert.Equal(expectedCode, results.GeneratedCode);
            Assert.Empty(results.ParserErrors);
            Assert.Equal(expectedLineMappings, results.DesignTimeLineMappings);
        }

        private string ReadResource(string resourceName)
        {
            var assembly = typeof(InjectChunkVisitorTest).Assembly;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        private static CodeGeneratorContext CreateContext()
        {
            return CodeGeneratorContext.Create(new MvcRazorHost("RazorView"),
                                              "MyClass",
                                              "MyNamespace",
                                              string.Empty,
                                              shouldGenerateLinePragmas: true);
        }

        private static LineMapping BuildLineMapping(int documentAbsoluteIndex,
                                                    int documentLineIndex,
                                                    int documentCharacterIndex,
                                                    int generatedAbsoluteIndex,
                                                    int generatedLineIndex,
                                                    int generatedCharacterIndex,
                                                    int contentLength)
        {
            return new LineMapping(
                        documentLocation: new MappingLocation(new SourceLocation(documentAbsoluteIndex, documentLineIndex, documentCharacterIndex), contentLength),
                        generatedLocation: new MappingLocation(new SourceLocation(generatedAbsoluteIndex, generatedLineIndex, generatedCharacterIndex), contentLength)
                    );
        }
    }
}