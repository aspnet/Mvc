// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Mvc.FunctionalTests
{
    public class RazorViewLocationSpecificationTest : IClassFixture<MvcTestFixture<RazorWebSite.Startup>>
    {
        private const string BaseUrl = "http://localhost/ViewNameSpecification_Home/";

        public RazorViewLocationSpecificationTest(MvcTestFixture<RazorWebSite.Startup> fixture)
        {
            Client = fixture.Client;
        }

        public HttpClient Client { get; }

        [Theory]
        [InlineData("LayoutSpecifiedWithPartialPathInViewStart")]
        [InlineData("LayoutSpecifiedWithPartialPathInViewStart_ForViewSpecifiedWithAppRelativePath")]
        [InlineData("LayoutSpecifiedWithPartialPathInViewStart_ForViewSpecifiedWithPartialName")]
        [InlineData("LayoutSpecifiedWithPartialPathInViewStart_ForViewSpecifiedWithAppRelativePathWithExtension")]
        public async Task PartialLayoutPaths_SpecifiedInViewStarts_GetResolvedByViewEngine(string action)
        {
            // Arrange
            var expected =
@"<layout>
_ViewStart that specifies partial Layout
</layout>";

            // Act
            var body = await Client.GetStringAsync(BaseUrl + action);

            // Assert
            Assert.Equal(expected, body.Trim(), ignoreLineEndingDifferences: true);
        }

        [Theory]
        [InlineData("LayoutSpecifiedWithPartialPathInPage")]
        [InlineData("LayoutSpecifiedWithPartialPathInPageWithPartialPath")]
        [InlineData("LayoutSpecifiedWithPartialPathInPageWithAppRelativePath")]
        [InlineData("LayoutSpecifiedWithPartialPathInPageWithAppRelativePathWithExtension")]
        public async Task PartialLayoutPaths_SpecifiedInPage_GetResolvedByViewEngine(string actionName)
        {
            // Arrange
            var expected =
@"<non-shared>Layout specified in page
</non-shared>";

            // Act
            var body = await Client.GetStringAsync(BaseUrl + actionName);

            // Assert
            Assert.Equal(expected, body.Trim(), ignoreLineEndingDifferences: true);
        }

        [Theory]
        [InlineData("LayoutSpecifiedWithNonPartialPath")]
        [InlineData("LayoutSpecifiedWithNonPartialPathWithExtension")]
        public async Task NonPartialLayoutPaths_GetResolvedByViewEngine(string actionName)
        {
            // Arrange
            var expected =
@"<non-shared>Page With Non Partial Layout
</non-shared>";

            // Act
            var body = await Client.GetStringAsync(BaseUrl + actionName);

            // Assert
            Assert.Equal(expected, body.Trim(), ignoreLineEndingDifferences: true);
        }

        [Theory]
        [InlineData("ViewWithPartial_SpecifiedWithPartialName")]
        [InlineData("ViewWithPartial_SpecifiedWithAbsoluteName")]
        [InlineData("ViewWithPartial_SpecifiedWithAbsoluteNameAndExtension")]
        public async Task PartialsCanBeSpecifiedWithPartialPath(string actionName)
        {
            // Arrange
            var expected =
@"<layout>
Non Shared Partial

</layout>";

            // Act
            var body = await Client.GetStringAsync(BaseUrl + actionName);

            // Assert
            Assert.Equal(expected, body.Trim(), ignoreLineEndingDifferences: true);
        }
    }
}