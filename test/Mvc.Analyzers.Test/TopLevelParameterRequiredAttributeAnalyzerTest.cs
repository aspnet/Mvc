using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Analyzer.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public class TopLevelParameterRequiredAttributeAnalyzerTest
    {
        private MvcDiagnosticAnalyzerRunner Runner { get; } = new MvcDiagnosticAnalyzerRunner(new TopLevelParameterRequiredAttributeAnalyzer());

        [Fact]
        public Task DiagnosticsAreReturned_ForControllerActionsWithParametersWithRequiredAttributeForArray()
            => RunTest();

        [Fact]
        public Task DiagnosticsAreReturned_ForControllerActionsWithParametersWithRequiredAttributeForIEnumerable()
            => RunTest();

        [Fact]
        public Task DiagnosticsAreReturned_ForControllerActionsWithParametersWithOutRequiredAttribute()
            => RunNoDiagnosticsAreReturned(nameof(TopLevelParameterRequiredAttributeAnalyzerTest));


        [Fact]
        public Task NoDiagnosticsAreReturnedIfParameterIsRenamedUsingBindingAttribute()
            => RunNoDiagnosticsAreReturned(nameof(TopLevelParameterNameAnalyzerTest));

        [Fact]
        public Task NoDiagnosticsAreReturnedForNonActions()
            => RunNoDiagnosticsAreReturned(nameof(TopLevelParameterNameAnalyzerTest));

        [Fact]
        public Task RunNoDiagnosticsAreReturnedForNonCollectionsParameterWithRequired()
            => RunNoDiagnosticsAreReturned(nameof(TopLevelParameterRequiredAttributeAnalyzerTest));

        private async Task RunNoDiagnosticsAreReturned(string typename, [CallerMemberName] string testMethod = "")
        {
            // Arrange
            var testSource = MvcTestSource.Read(typename, testMethod);
            var expectedLocation = testSource.DefaultMarkerLocation;

            // Act
            var result = await Runner.GetDiagnosticsAsync(testSource.Source);

            // Assert
            Assert.Empty(result);
        }

        private async Task RunTest([CallerMemberName] string testMethod = "")
        {
            // Arrange
            var descriptor = DiagnosticDescriptors.MVC1005_AttributeAvoidUsingRequiredAndBindRequired;
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
                    Assert.Equal(string.Format(descriptor.MessageFormat.ToString(), "RequiredAttribute", "MinLength(1)"), diagnostic.GetMessage());
                });
        }
    }
}
