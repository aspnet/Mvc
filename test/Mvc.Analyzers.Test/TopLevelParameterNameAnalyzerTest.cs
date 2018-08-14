﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Analyzer.Testing;
using Microsoft.AspNetCore.Mvc.Analyzers.TopLevelParameterNameAnalyzerTestFiles;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public class TopLevelParameterNameAnalyzerTest
    {
        private MvcDiagnosticAnalyzerRunner Runner { get; } = new MvcDiagnosticAnalyzerRunner(new TopLevelParameterNameAnalyzer());

        [Fact]
        public Task DiagnosticsAreReturned_ForControllerActionsWithParametersThatMatchProperties()
            => RunTest(nameof(DiagnosticsAreReturned_ForControllerActionsWithParametersThatMatchPropertiesModel), "model");

        [Fact]
        public Task DiagnosticsAreReturned_ForModelBoundParameters()
            => RunTest(nameof(DiagnosticsAreReturned_ForModelBoundParametersModel), "value");

        [Fact]
        public Task DiagnosticsAreReturned_IfModelNameProviderIsUsedToModifyParameterName()
            => RunTest(nameof(DiagnosticsAreReturned_IfModelNameProviderIsUsedToModifyParameterNameModel), "parameter");

        [Fact]
        public Task NoDiagnosticsAreReturnedForApiControllers()
            => RunNoDiagnosticsAreReturned();

        [Fact]
        public Task NoDiagnosticsAreReturnedIfParameterIsRenamedUsingBindingAttribute()
            => RunNoDiagnosticsAreReturned();

        [Fact]
        public Task NoDiagnosticsAreReturnedForNonActions()
            => RunNoDiagnosticsAreReturned();

        [Fact]
        public async Task IsProblematicParameter_ReturnsTrue_IfParameterNameIsTheSameAsModelProperty()
        {
            var result = await IsProblematicParameterTest();
            Assert.True(result);
        }

        [Fact]
        public async Task IsProblematicParameter_ReturnsTrue_IfParameterNameWithBinderAttributeIsTheSameNameAsModelProperty()
        {
            var result = await IsProblematicParameterTest();
            Assert.True(result);
        }

        [Fact]
        public async Task IsProblematicParameter_ReturnsTrue_IfPropertyWithModelBindingAttributeHasSameNameAsParameter()
        {
            var result = await IsProblematicParameterTest();
            Assert.True(result);
        }

        [Fact]
        public async Task IsProblematicParameter_ReturnsFalse_IfModelBinderAttributeIsUsedToRenameParameter()
        {
            var result = await IsProblematicParameterTest();
            Assert.False(result);
        }

        [Fact]
        public async Task IsProblematicParameter_ReturnsFalse_IfModelBinderAttributeIsUsedToRenameProperty()
        {
            var result = await IsProblematicParameterTest();
            Assert.False(result);
        }

        [Fact]
        public async Task IsProblematicParameter_ReturnsFalse_ForFromBodyParameter()
        {
            var result = await IsProblematicParameterTest();
            Assert.False(result);
        }

        [Fact]
        public async Task IsProblematicParameter_IgnoresStaticProperties()
        {
            var result = await IsProblematicParameterTest();
            Assert.False(result);
        }

        [Fact]
        public async Task IsProblematicParameter_IgnoresFields()
        {
            var result = await IsProblematicParameterTest();
            Assert.False(result);
        }

        [Fact]
        public async Task IsProblematicParameter_IgnoresMethods()
        {
            var result = await IsProblematicParameterTest();
            Assert.False(result);
        }

        [Fact]
        public async Task IsProblematicParameter_IgnoresNonPublicProperties()
        {
            var result = await IsProblematicParameterTest();
            Assert.False(result);
        }

        private async Task<bool> IsProblematicParameterTest([CallerMemberName] string testMethod = "")
        {
            var testSource = MvcTestSource.Read(GetType().Name, testMethod);
            var project = DiagnosticProject.Create(GetType().Assembly, new[] { testSource.Source });

            var compilation = await project.GetCompilationAsync();

            var modelType = compilation.GetTypeByMetadataName($"Microsoft.AspNetCore.Mvc.Analyzers.TopLevelParameterNameAnalyzerTestFiles.{testMethod}");
            var method = (IMethodSymbol)modelType.GetMembers("ActionMethod").First();
            var parameter = method.Parameters[0];

            var symbolCache = new TopLevelParameterNameAnalyzer.SymbolCache(compilation);

            var result = TopLevelParameterNameAnalyzer.IsProblematicParameter(symbolCache, parameter);
            return result;
        }

        private async Task RunNoDiagnosticsAreReturned([CallerMemberName] string testMethod = "")
        {
            // Arrange
            var testSource = MvcTestSource.Read(GetType().Name, testMethod);
            var expectedLocation = testSource.DefaultMarkerLocation;

            // Act
            var result = await Runner.GetDiagnosticsAsync(testSource.Source);

            // Assert
            Assert.Empty(result);
        }

        private async Task RunTest(string typeName, string parameterName, [CallerMemberName] string testMethod = "")
        {
            // Arrange
            var descriptor = DiagnosticDescriptors.MVC1004_ParameterNameCollidesWithTopLevelProperty;
            var testSource = MvcTestSource.Read(GetType().Name, testMethod);
            var expectedLocation = testSource.DefaultMarkerLocation;

            // Act
            var result = await Runner.GetDiagnosticsAsync(testSource.Source);

            // Assert
            Assert.Collection(
                result,
                diagnostic =>
                {
                    Assert.Equal(descriptor.Id, diagnostic.Id);
                    Assert.Same(descriptor, diagnostic.Descriptor);
                    AnalyzerAssert.DiagnosticLocation(expectedLocation, diagnostic.Location);
                    Assert.Equal(string.Format(descriptor.MessageFormat.ToString(), typeName, parameterName), diagnostic.GetMessage());
                });
        }
    }
}
