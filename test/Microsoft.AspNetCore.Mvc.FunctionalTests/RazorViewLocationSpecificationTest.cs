// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Net;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class RazorViewLocationSpecificationTest : IClassFixture<MvcTestFixture<RazorWebSite.Startup>>
    {
        private const string BaseUrl = "http://localhost/ViewNameSpecification_Home/";

        public RazorViewLocationSpecificationTest(MvcTestFixture<RazorWebSite.Startup> fixture)
        {
            Client = fixture.CreateDefaultClient();
        }

        public HttpClient Client { get; }

        [Theory]
        [InlineData("LayoutSpecifiedWithPartialPathInViewStart")]
        [InlineData("LayoutSpecifiedWithPartialPathInViewStart_ForViewSpecifiedWithRelativePath")]
        [InlineData("LayoutSpecifiedWithPartialPathInViewStart_ForViewSpecifiedWithPartialName")]
        [InlineData("LayoutSpecifiedWithPartialPathInViewStart_ForViewSpecifiedWithAppRelativePath")]
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
        [InlineData("LayoutSpecifiedWithPartialPathInPageWithRelativePath")]
        [InlineData("LayoutSpecifiedWithPartialPathInPageWithAppRelativePath")]
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
        [InlineData("LayoutSpecifiedWithRelativePath")]
        [InlineData("LayoutSpecifiedWithAppRelativePath")]
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
        [InlineData("ViewWithPartial_SpecifiedWithRelativePath")]
        [InlineData("ViewWithPartial_SpecifiedWithAppRelativePath")]
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

        [Fact]
        public async Task PartialLayout_ThrowsIfRequiredSectionMissing()
        {
            // Arrange
            var path = "http://localhost/PartialViewEngine/ViewPartialMissingSection";

            // Act
            var content = await (await Client.GetAsync(path)).Content.ReadAsStringAsync();

            // Assert
            Assert.Contains(
                "The layout page '/Views/Shared/_PartialLayout.cshtml' cannot find the section " +
                    "'section' in the content page '/Views/PartialViewEngine/PartialMissingSection.cshtml'.",
                WebUtility.HtmlDecode(content));
        }
    }
}