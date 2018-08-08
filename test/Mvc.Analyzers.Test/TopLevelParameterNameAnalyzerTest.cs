// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Analyzer.Testing;
using Microsoft.AspNetCore.Mvc.Analyzers.TopLevelParameterNameAnalyzerTestFiles;
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
        public Task NoDiagnosticsAreReturnedForApiControllers()
            => RunNoDiagnosticsAreReturned();

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
