using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Analyzer.Testing;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public class TopLevelParameterAttributeAnalyzerTest
    {
        private MvcDiagnosticAnalyzerRunner Runner { get; } = new MvcDiagnosticAnalyzerRunner(new TopLevelParameterAttributeAnalyzer());

        [Fact]
        public Task DiagnosticsAreReturned_ForControllerActionsWithParametersWithRequiredAttributeForArray()
            => RunTest();

        [Fact]
        public Task DiagnosticsAreReturned_ForControllerActionsWithParametersWithRequiredAttributeForIEnumerable()
            => RunTest();

        [Fact]
        public async Task DiagnosticsAreReturned_ForControllerActionsWithParametersWithOutRequiredAttribute()
        {
            {
                // Arrange
                var testSource = MvcTestSource.Read(GetType().Name, nameof(DiagnosticsAreReturned_ForControllerActionsWithParametersWithOutRequiredAttribute));

                //Act
                var result = await Runner.GetDiagnosticsAsync(testSource.Source);

                Assert.Empty(result);
            }
        }

        [Fact]
        public Task NoDiagnosticsAreReturnedIfParameterIsRenamedUsingBindingAttribute()
            => RunNoDiagnosticsAreReturned();

        [Fact]
        public Task NoDiagnosticsAreReturnedForNonActions()
            => RunNoDiagnosticsAreReturned();

        private async Task RunNoDiagnosticsAreReturned([CallerMemberName] string testMethod = "")
        {
            // Arrange
            var testSource = MvcTestSource.Read(nameof(TopLevelParameterNameAnalyzerTest), testMethod);
            var expectedLocation = testSource.DefaultMarkerLocation;

            // Act
            var result = await Runner.GetDiagnosticsAsync(testSource.Source);

            // Assert
            Assert.Empty(result);
        }

        private async Task RunTest([CallerMemberName] string testMethod = "")
        {
            // Arrange
            var descriptor = DiagnosticDescriptors.MVC1005_ParameterAttributeAvoidUsingRequiredOncollections;
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
                    Assert.Equal(string.Format(descriptor.MessageFormat.ToString(), SymbolNames.RequiredAttribute), diagnostic.GetMessage());
                });
        }
    }
}
