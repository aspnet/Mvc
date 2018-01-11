using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class FallbackRazorPagesConventionsTest
    {
        [Theory]
        [InlineData("/Pages/Shared/Index", "/Pages/Index")]
        [InlineData("/Pages/Shared/Shared", "/Pages/Shared")]
        [InlineData("/Areas/Accounts/Pages/Users/Shared/Login", "/Areas/Accounts/Pages/Users/Login")]
        public void TryGetSupersedingPath_ReturnsPaths(string input, string expected)
        {
            // Act
            var result = FallbackRazorPagesConventions.TryGetSupersedingPath(input, out var supersedingPath);

            // Assert
            Assert.True(result);
            Assert.Equal(expected, supersedingPath);
        }

        [Theory]
        [InlineData("/Pages/Index")]
        [InlineData("/Pages/Shared")]
        [InlineData("/Areas/Accounts/Pages/Users/Profile")]
        public void TryGetSupersedingPath_ReturnsFalse_IfFileIsNotLocatedUnderFallbackDirectory(string input)
        {
            // Act
            var result = FallbackRazorPagesConventions.TryGetSupersedingPath(input, out var supersedingPath);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsSuperseded_ReturnsFalse_IfBothRoutesAreFallback()
        {
            // Arrange
            var routeModel1 = new PageRouteModel("/Pages/Shared/About.cshtml", "/Pages/About")
            {
                IsFallbackRoute = true,
            };
            var routeModel2 = new PageRouteModel("/Pages/Shared/About.cshtml", "/Pages/About")
            {
                IsFallbackRoute = true,
            };

            // Act
            var result = FallbackRazorPagesConventions.IsSuperseded(routeModel1, routeModel2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsSuperseded_ReturnsFalse_IfRoutesHaveDifferentViewEnginePaths()
        {
            // Arrange
            var routeModel1 = new PageRouteModel("/Pages/About.cshtml", "/Pages/About");
            var routeModel2 = new PageRouteModel("/Pages/Shared/Edit.cshtml", "/Pages/Edit")
            {
                IsFallbackRoute = true,
            };

            // Act
            var result = FallbackRazorPagesConventions.IsSuperseded(routeModel1, routeModel2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsSuperseded_ReturnsFalse_IfRoutesHaveDifferentRouteValues()
        {
            // Arrange
            var routeModel1 = new PageRouteModel("/Pages/About.cshtml", "/Pages/About");
            var routeModel2 = new PageRouteModel("/Areas/MyArea/Pages/Shared/About.cshtml", "/Pages/About")
            {
                IsFallbackRoute = true,
                RouteValues = { ["area"] = "MyArea" },
            };

            // Act
            var result = FallbackRazorPagesConventions.IsSuperseded(routeModel1, routeModel2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsSuperseded_ReturnsTrue_IfRoutesHaveSameViewEnginePathAndSameRouteValues()
        {
            // Arrange
            var routeModel1 = new PageRouteModel("/Pages/Shared/About.cshtml", "/Pages/About")
            {
                IsFallbackRoute = true,
            };
            var routeModel2 = new PageRouteModel("/Pages/Shared/About.cshtml", "/Pages/About");

            // Act
            var result = FallbackRazorPagesConventions.IsSuperseded(routeModel1, routeModel2);

            // Assert
            Assert.True(result);
        }
    }
}
