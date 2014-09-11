// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.Razor;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Text;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class MvcRazorHostTest
    {
        [Fact]
        public void MvcRazorHost_GeneratesInstrumentedCodeByDefault()
        {
            // Arrange
            var host = new MvcRazorHost(new TestFileSystem());

            var input = "TestFiles/Input/Basic.cshtml";
            var output = "TestFiles/Output/Basic.cs";
            var expectedLineMappings = new[]
            {
                BuildLineMapping(13, 0, 13, 1342, 34, 13, 4),
                BuildLineMapping(43, 2, 5, 1705, 45, 5, 21)
            };

            // Act and Assert
            RunTest(host, input, output, expectedLineMappings);
        }

        [Fact]
        public void InjectVisitor_GeneratesCorrectLineMappings()
        {
            // Arrange
            var host = new MvcRazorHost(new TestFileSystem())
            {
                DesignTimeMode = true
            };
            host.NamespaceImports.Clear();
            var input = "TestFiles/Input/Inject.cshtml";
            var output = "TestFiles/Output/Inject.cs";
            var expectedLineMappings = new List<LineMapping>
            {
                BuildLineMapping(1, 0, 1, 59, 3, 0, 17),
                BuildLineMapping(28, 1, 8, 678, 26, 8, 20)
            };

            // Act and Assert
            RunTest(host, input, output, expectedLineMappings);
        }

        [Fact]
        public void InjectVisitorWithModel_GeneratesCorrectLineMappings()
        {
            // Arrange
            var host = new MvcRazorHost(new TestFileSystem())
            {
                DesignTimeMode = true
            };
            host.NamespaceImports.Clear();
            var input = "TestFiles/Input/InjectWithModel.cshtml";
            var output = "TestFiles/Output/InjectWithModel.cs";
            var expectedLineMappings = new[]
            {
                BuildLineMapping(7, 0, 7, 209, 6, 7, 7),
                BuildLineMapping(24, 1, 8, 703, 26, 8, 20),
                BuildLineMapping(54, 2, 8, 911, 34, 8, 23)
            };

            // Act and Assert
            RunTest(host, input, output, expectedLineMappings);
        }

        [Fact]
        public void ModelVisitor_GeneratesCorrectLineMappings()
        {
            // Arrange
            var host = new MvcRazorHost(new TestFileSystem())
            {
                DesignTimeMode = true
            };
            host.NamespaceImports.Clear();
            var input = "TestFiles/Input/Model.cshtml";
            var output = "TestFiles/Output/Model.cs";
            var expectedLineMappings = new[]
            {
                BuildLineMapping(7, 0, 7, 189, 6, 7, 30),
            };

            // Act
            RunTest(host, input, output, expectedLineMappings);
        }

        private static void RunTest(MvcRazorHost host,
                                    string inputFile,
                                    string outputFile,
                                    IEnumerable<LineMapping> expectedLineMappings)
        {
            var expectedCode = ReadResource(outputFile);

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
    }
}