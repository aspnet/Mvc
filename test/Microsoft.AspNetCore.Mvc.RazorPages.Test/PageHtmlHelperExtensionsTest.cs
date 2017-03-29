// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.RazorPages
{
    public class PageHtmlHelperExtensionsTest
    {
        [Fact]
        public void PageLink_InvokesRouteLinkWithPageValue()
        {
            // Arrange
            var htmlHelper = new Mock<IHtmlHelper>(MockBehavior.Strict);
            object actualRouteValues = null;
            htmlHelper.Setup(h => h.RouteLink("Hello world", null, null, null, null, It.IsAny<object>(), It.IsAny<object>()))
                .Returns((
                    string linkText,
                    string routeName,
                    string protocol,
                    string hostName,
                    string fragment,
                    object routeValues,
                    object htmlAttributes) =>
                {
                    actualRouteValues = routeValues;
                    return null;
                })
                .Verifiable();

            // Act
            PageHtmlHelperExtensions.PageLink(htmlHelper.Object, "Hello world", "MyPage");

            // Assert
            htmlHelper.Verify();
            Assert.NotNull(actualRouteValues);
            var routeValueDictionary = Assert.IsType<RouteValueDictionary>(actualRouteValues);
            Assert.Collection(routeValueDictionary,
                item =>
                {
                    Assert.Equal("page", item.Key);
                    Assert.Equal("MyPage", item.Value);
                });
        }

        [Fact]
        public void PageLink_InvokesRouteLinkWithHtmlAttributes()
        {
            // Arrange
            var htmlHelper = new Mock<IHtmlHelper>();
            object actualRouteValues = null;
            object htmlAttributes = new { @class = "value" };
            htmlHelper.Setup(h => h.RouteLink("Hello world", null, null, null, null, It.IsAny<object>(), htmlAttributes))
                .Returns((
                    string linkText,
                    string routeName,
                    string protocol,
                    string hostName,
                    string fragment,
                    object routeValues,
                    object attributes) =>
                {
                    actualRouteValues = routeValues;
                    return null;
                })
                .Verifiable();

            // Act
            PageHtmlHelperExtensions.PageLink(htmlHelper.Object, "Hello world", "MyPage");

            // Assert
            htmlHelper.Verify();
            Assert.NotNull(actualRouteValues);
            var routeValueDictionary = Assert.IsType<RouteValueDictionary>(actualRouteValues);
            Assert.Collection(routeValueDictionary,
                item =>
                {
                    Assert.Equal("page", item.Key);
                    Assert.Equal("MyPage", item.Value);
                });
        }

        [Fact]
        public void PageLink_OverridesPageValueSpecifiedInRouteValues()
        {
            // Arrange
            var htmlHelper = new Mock<IHtmlHelper>();
            object actualRouteValues = null;
            htmlHelper.Setup(h => h.RouteLink("Hello world", null, null, null, null, It.IsAny<object>(), It.IsAny<object>()))
                .Returns((
                    string linkText,
                    string routeName,
                    string protocol,
                    string hostName,
                    string fragment,
                    object routeValues,
                    object htmlAttributes) =>
                {
                    actualRouteValues = routeValues;
                    return null;
                });

            // Act
            PageHtmlHelperExtensions.PageLink(htmlHelper.Object, "Hello world", "MyPage", new { page = "notmypage", id = 10 });

            // Assert
            Assert.NotNull(actualRouteValues);
            var routeValueDictionary = Assert.IsType<RouteValueDictionary>(actualRouteValues);
            Assert.Collection(routeValueDictionary,
                item =>
                {
                    Assert.Equal("id", item.Key);
                    Assert.Equal(10, item.Value);
                },
                item =>
                {
                    Assert.Equal("page", item.Key);
                    Assert.Equal("MyPage", item.Value);
                });
        }
    }
}
