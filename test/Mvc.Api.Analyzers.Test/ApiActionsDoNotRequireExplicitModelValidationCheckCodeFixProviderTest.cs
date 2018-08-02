﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Analyzer.Testing;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Api.Analyzers
{
    public class ApiActionsDoNotRequireExplicitModelValidationCheckCodeFixProviderTest
    {
        private MvcDiagnosticAnalyzerRunner AnalyzerRunner { get; } = new MvcDiagnosticAnalyzerRunner(new ApiActionsDoNotRequireExplicitModelValidationCheckAnalyzer());

        private CodeFixRunner CodeFixRunner => CodeFixRunner.Default;

        [Fact]
        public Task CodeFixRemovesModelStateIsInvalidBlockWithIfNotCheck()
            => RunTest();

        [Fact]
        public Task CodeFixRemovesModelStateIsInvalidBlockWithEqualityCheck()
            => RunTest();

        [Fact]
        public Task CodeFixRemovesIfBlockWithoutBraces()
            => RunTest();

        private async Task RunTest([CallerMemberName] string testMethod = "")
        {
            // Arrange
            var project = GetProject(testMethod);
            var controllerDocument = project.DocumentIds[0];
            var expectedOutput = Read(testMethod + ".Output");

            // Act
            var diagnostics = await AnalyzerRunner.GetDiagnosticsAsync(project);
            Assert.NotEmpty(diagnostics);
            var actualOutput = await CodeFixRunner.ApplyCodeFixAsync(
                new ApiActionsDoNotRequireExplicitModelValidationCheckCodeFixProvider(),
                project.GetDocument(controllerDocument),
                diagnostics[0]);

            Assert.Equal(expectedOutput, actualOutput, ignoreLineEndingDifferences: true);
        }

        private Project GetProject(string testMethod)
        {
            var testSource = Read(testMethod + ".Input");
            return DiagnosticProject.Create(GetType().Assembly, new[] { testSource });
        }

        private string Read(string fileName)
        {
            return MvcTestSource.Read(GetType().Name, fileName)
                .Source
                .Replace("_INPUT_", "_TEST_")
                .Replace("_OUTPUT_", "_TEST_");
        }
    }
}
