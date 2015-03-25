﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.Mvc.Razor.Directives;
using Microsoft.AspNet.Mvc.Razor.Internal;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.AspNet.Razor.Text;
using Microsoft.Framework.Internal;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class MvcRazorHostTest
    {
        [Theory]
        [InlineData("//")]
        [InlineData("C:/")]
        [InlineData(@"\\")]
        [InlineData(@"C:\")]
        public void DecorateRazorParser_DesignTimeRazorPathNormalizer_NormalizesChunkInheritanceUtilityPaths(
            string rootPrefix)
        {
            // Arrange
            var rootedAppPath = $"{rootPrefix}SomeComputer/Location/Project/";
            var rootedFilePath = $"{rootPrefix}SomeComputer/Location/Project/src/file.cshtml";
            var host = new MvcRazorHost(
                codeTreeCache: null,
                pathNormalizer: new DesignTimeRazorPathNormalizer(rootedAppPath));
            var parser = new RazorParser(
                host.CodeLanguage.CreateCodeParser(),
                host.CreateMarkupParser(),
                tagHelperDescriptorResolver: null);
            var chunkInheritanceUtility = new PathValidatingChunkInheritanceUtility(host);
            host.ChunkInheritanceUtility = chunkInheritanceUtility;

            // Act
            host.DecorateRazorParser(parser, rootedFilePath);

            // Assert
            Assert.Equal("src/file.cshtml", chunkInheritanceUtility.InheritedCodeTreePagePath, StringComparer.Ordinal);
        }

        [Theory]
        [InlineData("//")]
        [InlineData("C:/")]
        [InlineData(@"\\")]
        [InlineData(@"C:\")]
        public void DecorateCodeBuilder_DesignTimeRazorPathNormalizer_NormalizesChunkInheritanceUtilityPaths(
            string rootPrefix)
        {
            // Arrange
            var rootedAppPath = $"{rootPrefix}SomeComputer/Location/Project/";
            var rootedFilePath = $"{rootPrefix}SomeComputer/Location/Project/src/file.cshtml";
            var host = new MvcRazorHost(
                codeTreeCache: null,
                pathNormalizer: new DesignTimeRazorPathNormalizer(rootedAppPath));
            var chunkInheritanceUtility = new PathValidatingChunkInheritanceUtility(host);
            var codeBuilderContext = new CodeBuilderContext(
                new CodeGeneratorContext(
                    host,
                    host.DefaultClassName,
                    host.DefaultNamespace,
                    rootedFilePath,
                    shouldGenerateLinePragmas: true),
                new ParserErrorSink());
            var codeBuilder = new CSharpCodeBuilder(codeBuilderContext);
            host.ChunkInheritanceUtility = chunkInheritanceUtility;

            // Act
            host.DecorateCodeBuilder(codeBuilder, codeBuilderContext);

            // Assert
            Assert.Equal("src/file.cshtml", chunkInheritanceUtility.InheritedCodeTreePagePath, StringComparer.Ordinal);
        }

        [Fact]
        public void MvcRazorHost_EnablesInstrumentationByDefault()
        {
            // Arrange
            var fileProvider = new TestFileProvider();
            var host = new MvcRazorHost(new DefaultCodeTreeCache(fileProvider));

            // Act
            var instrumented = host.EnableInstrumentation;

            // Assert
            Assert.True(instrumented);
        }

        [Fact]
        public void MvcRazorHost_GeneratesTagHelperModelExpressionCode_DesignTime()
        {
            // Arrange
            var fileProvider = new TestFileProvider();
            var host = new MvcRazorHost(new DefaultCodeTreeCache(fileProvider))
            {
                DesignTimeMode = true
            };
            var expectedLineMappings = new List<LineMapping>
            {
                BuildLineMapping(documentAbsoluteIndex: 7,
                                 documentLineIndex: 0,
                                 documentCharacterIndex: 7,
                                 generatedAbsoluteIndex: 444,
                                 generatedLineIndex: 12,
                                 generatedCharacterIndex: 7,
                                 contentLength: 8),
                BuildLineMapping(documentAbsoluteIndex: 33,
                                 documentLineIndex: 2,
                                 documentCharacterIndex: 14,
                                 generatedAbsoluteIndex: 823,
                                 generatedLineIndex: 25,
                                 generatedCharacterIndex: 14,
                                 contentLength: 85),
                BuildLineMapping(documentAbsoluteIndex: 139,
                                 documentLineIndex: 4,
                                 documentCharacterIndex: 17,
                                 generatedAbsoluteIndex: 2105,
                                 generatedLineIndex: 53,
                                 generatedCharacterIndex: 95,
                                 contentLength: 3),
                BuildLineMapping(
                    documentAbsoluteIndex: 166,
                    documentLineIndex: 5,
                    documentCharacterIndex: 18,
                    generatedAbsoluteIndex: 2418,
                    generatedLineIndex: 59,
                    generatedCharacterIndex: 87,
                    contentLength: 5),
            };

            // Act and Assert
            RunDesignTimeTest(host,
                              testName: "ModelExpressionTagHelper",
                              expectedLineMappings: expectedLineMappings);
        }

        [Theory]
        [InlineData("Basic")]
        [InlineData("Inject")]
        [InlineData("InjectWithModel")]
        [InlineData("InjectWithSemicolon")]
        [InlineData("Model")]
        [InlineData("ModelExpressionTagHelper")]
        public void MvcRazorHost_ParsesAndGeneratesCodeForBasicScenarios(string scenarioName)
        {
            // Arrange
            var fileProvider = new TestFileProvider();
            var host = new TestMvcRazorHost(new DefaultCodeTreeCache(fileProvider));

            // Act and Assert
            RunRuntimeTest(host, scenarioName);
        }

        [Fact]
        public void InjectVisitor_GeneratesCorrectLineMappings()
        {
            // Arrange
            var fileProvider = new TestFileProvider();
            var host = new MvcRazorHost(new DefaultCodeTreeCache(fileProvider))
            {
                DesignTimeMode = true
            };
            host.NamespaceImports.Clear();
            var expectedLineMappings = new List<LineMapping>
            {
                BuildLineMapping(1, 0, 1, 59, 3, 0, 17),
                BuildLineMapping(28, 1, 8, 688, 26, 8, 20)
            };

            // Act and Assert
            RunDesignTimeTest(host, "Inject", expectedLineMappings);
        }

        [Fact]
        public void InjectVisitorWithModel_GeneratesCorrectLineMappings()
        {
            // Arrange
            var fileProvider = new TestFileProvider();
            var host = new MvcRazorHost(new DefaultCodeTreeCache(fileProvider))
            {
                DesignTimeMode = true
            };
            host.NamespaceImports.Clear();
            var expectedLineMappings = new[]
            {
                BuildLineMapping(7, 0, 7, 214, 6, 7, 7),
                BuildLineMapping(24, 1, 8, 713, 26, 8, 20),
                BuildLineMapping(54, 2, 8, 921, 34, 8, 23)
            };

            // Act and Assert
            RunDesignTimeTest(host, "InjectWithModel", expectedLineMappings);
        }

        [Fact]
        public void InjectVisitorWithSemicolon_GeneratesCorrectLineMappings()
        {
            // Arrange
            var fileProvider = new TestFileProvider();
            var host = new MvcRazorHost(new DefaultCodeTreeCache(fileProvider))
            {
                DesignTimeMode = true
            };
            host.NamespaceImports.Clear();
            var expectedLineMappings = new[]
            {
                BuildLineMapping(7, 0, 7, 222, 6, 7, 7),
                BuildLineMapping(24, 1, 8, 729, 26, 8, 20),
                BuildLineMapping(58, 2, 8, 941, 34, 8, 23),
                BuildLineMapping(93, 3, 8, 1156, 42, 8, 21),
                BuildLineMapping(129, 4, 8, 1369, 50, 8, 24),
            };

            // Act and Assert
            RunDesignTimeTest(host, "InjectWithSemicolon", expectedLineMappings);
        }

        [Fact]
        public void ModelVisitor_GeneratesCorrectLineMappings()
        {
            // Arrange
            var fileProvider = new TestFileProvider();
            var host = new MvcRazorHost(new DefaultCodeTreeCache(fileProvider))
            {
                DesignTimeMode = true
            };
            host.NamespaceImports.Clear();
            var expectedLineMappings = new[]
            {
                BuildLineMapping(7, 0, 7, 194, 6, 7, 30),
            };

            // Act and Assert
            RunDesignTimeTest(host, "Model", expectedLineMappings);
        }

        private static void RunRuntimeTest(MvcRazorHost host,
                                           string testName)
        {
            var inputFile = "TestFiles/Input/" + testName + ".cshtml";
            var expectedCode = ReadResource("TestFiles/Output/Runtime/" + testName + ".cs");

            // Act
            GeneratorResults results;
            using (var stream = GetResourceStream(inputFile))
            {
                results = host.GenerateCode(inputFile, stream);
            }

            // Assert
            Assert.True(results.Success);
            Assert.Equal(expectedCode, results.GeneratedCode);
            Assert.Empty(results.ParserErrors);
        }

        private static void RunDesignTimeTest(MvcRazorHost host,
                                              string testName,
                                              IEnumerable<LineMapping> expectedLineMappings)
        {
            var inputFile = "TestFiles/Input/" + testName + ".cshtml";
            var expectedCode = ReadResource("TestFiles/Output/DesignTime/" + testName + ".cs");

            // Act
            GeneratorResults results;
            using (var stream = GetResourceStream(inputFile))
            {
                results = host.GenerateCode(inputFile, stream);
            }

            // Assert
            Assert.True(results.Success);
            Assert.Equal(expectedCode, results.GeneratedCode);
            Assert.Empty(results.ParserErrors);
            Assert.Equal(expectedLineMappings, results.DesignTimeLineMappings);
        }

        private static string ReadResource(string resourceName)
        {
            using (var stream = GetResourceStream(resourceName))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        private static Stream GetResourceStream(string resourceName)
        {
            var assembly = typeof(MvcRazorHostTest).Assembly;
            return assembly.GetManifestResourceStream(resourceName);
        }

        private static LineMapping BuildLineMapping(int documentAbsoluteIndex,
                                                    int documentLineIndex,
                                                    int documentCharacterIndex,
                                                    int generatedAbsoluteIndex,
                                                    int generatedLineIndex,
                                                    int generatedCharacterIndex,
                                                    int contentLength)
        {
            var documentLocation = new SourceLocation(documentAbsoluteIndex,
                                                      documentLineIndex,
                                                      documentCharacterIndex);
            var generatedLocation = new SourceLocation(generatedAbsoluteIndex,
                                                       generatedLineIndex,
                                                       generatedCharacterIndex);

            return new LineMapping(
                documentLocation: new MappingLocation(documentLocation, contentLength),
                generatedLocation: new MappingLocation(generatedLocation, contentLength));
        }

        private class PathValidatingChunkInheritanceUtility : ChunkInheritanceUtility
        {
            public PathValidatingChunkInheritanceUtility(MvcRazorHost razorHost)
                : base(razorHost, codeTreeCache: null, defaultInheritedChunks: new Chunk[0])
            {
            }

            public string InheritedCodeTreePagePath { get; private set; }

            public override IReadOnlyList<CodeTree> GetInheritedCodeTrees([NotNull] string pagePath)
            {
                InheritedCodeTreePagePath = pagePath;

                return new CodeTree[0];
            }
        }

        /// <summary>
        /// Used when testing Tag Helpers, it disables the unique ID generation feature.
        /// </summary>
        private class TestMvcRazorHost : MvcRazorHost
        {
            public TestMvcRazorHost(ICodeTreeCache codeTreeCache)
                : base(codeTreeCache)
            { }

            public override CodeBuilder DecorateCodeBuilder(CodeBuilder incomingBuilder, CodeBuilderContext context)
            {
                base.DecorateCodeBuilder(incomingBuilder, context);

                return new TestCSharpCodeBuilder(context,
                                                 DefaultModel,
                                                 ActivateAttribute,
                                                 new GeneratedTagHelperAttributeContext
                                                 {
                                                     ModelExpressionTypeName = ModelExpressionType,
                                                     CreateModelExpressionMethodName = CreateModelExpressionMethod
                                                 });
            }

            protected class TestCSharpCodeBuilder : MvcCSharpCodeBuilder
            {
                private readonly GeneratedTagHelperAttributeContext _tagHelperAttributeContext;

                public TestCSharpCodeBuilder(CodeBuilderContext context,
                                             string defaultModel,
                                             string activateAttribute,
                                             GeneratedTagHelperAttributeContext tagHelperAttributeContext)
                    : base(context, defaultModel, activateAttribute, tagHelperAttributeContext)
                {
                    _tagHelperAttributeContext = tagHelperAttributeContext;
                }

                protected override CSharpCodeVisitor CreateCSharpCodeVisitor(CSharpCodeWriter writer, CodeBuilderContext context)
                {
                    var visitor = base.CreateCSharpCodeVisitor(writer, context);
                    visitor.TagHelperRenderer = new NoUniqueIdsTagHelperCodeRenderer(visitor, writer, context)
                    {
                        AttributeValueCodeRenderer =
                            new MvcTagHelperAttributeValueCodeRenderer(_tagHelperAttributeContext)
                    };
                    return visitor;
                }

                private class NoUniqueIdsTagHelperCodeRenderer : CSharpTagHelperCodeRenderer
                {
                    public NoUniqueIdsTagHelperCodeRenderer(IChunkVisitor bodyVisitor,
                                                            CSharpCodeWriter writer,
                                                            CodeBuilderContext context)
                        : base(bodyVisitor, writer, context)
                    { }

                    protected override string GenerateUniqueId()
                    {
                        return "test";
                    }
                }
            }
        }
    }
}