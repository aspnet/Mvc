using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Analyzer.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public class ModelPropertyRequiredAttribueBindRequiredAttributeAnalyzerTest
    {
        private MvcDiagnosticAnalyzerRunner Runner { get; } = new MvcDiagnosticAnalyzerRunner(new ModelPropertyRequiredAttribueBindRequiredAttributeAnalyzer());

        [Fact]
        public Task DiagnosticsAreReturned_ForNonNullableTypeProperty() 
            => RunTest(new[] { "RequiredAttribute", "BindRequiredAttribute" });

        [Fact]
        public Task DiagnosticsAreReturned_ForComplexTypeProperty()
            => RunTest(new[] {"BindRequiredAttribute", "RequiredAttribute" });

        private async Task RunTest(string[] attributes,[CallerMemberName] string testMethod = "")
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
                    Assert.Equal(string.Format(descriptor.MessageFormat.ToString(), attributes[0], attributes[1]), diagnostic.GetMessage());
                });
        }

    }
}
