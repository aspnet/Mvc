// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    public class AnchorTagHelperTest
    {
        [Fact]
        public async Task ProcessAsync_GeneratesExpectedOutput()
        {
            // Arrange
            var expectedTagName = "not-a";
            var metadataProvider = new TestModelMetadataProvider();

            var tagHelperContext = new TagHelperContext(
                tagName: "not-a",
                allAttributes: new TagHelperAttributeList
                {
                    { "id", "myanchor" },
                    { "asp-route-name", "value" },
                    { "asp-action", "index" },
                    { "asp-controller", "home" },
                    { "asp-fragment", "hello=world" },
                    { "asp-host", "contoso.com" },
                    { "asp-protocol", "http" }
                },
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                expectedTagName,
                attributes: new TagHelperAttributeList
                {
                    { "id", "myanchor" },
                },
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something Else");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent("Something");

            var urlHelper = new Mock<IUrlHelper>();
            urlHelper
                .Setup(mock => mock.Action(It.IsAny<UrlActionContext>())).Returns("home/index");

            var htmlGenerator = new TestableHtmlGenerator(metadataProvider, urlHelper.Object);
            var viewContext = TestableHtmlGenerator.GetViewContext(
                model: null,
                htmlGenerator: htmlGenerator,
                metadataProvider: metadataProvider);
            var anchorTagHelper = new AnchorTagHelper(htmlGenerator)
            {
                Action = "index",
                Controller = "home",
                Fragment = "hello=world",
                Host = "contoso.com",
                Protocol = "http",
                RouteValues =
                {
                    {  "name", "value" },
                },
                ViewContext = viewContext,
            };

            // Act
            await anchorTagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            Assert.Equal(2, output.Attributes.Count);
            var attribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("id"));
            Assert.Equal("myanchor", attribute.Value);
            attribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("href"));
            Assert.Equal("home/index", attribute.Value);
            Assert.Equal("Something", output.Content.GetContent());
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Fact]
        public async Task ProcessAsync_CallsIntoRouteLinkWithExpectedParameters()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "a",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "a",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent(string.Empty);

            var generator = new Mock<IHtmlGenerator>(MockBehavior.Strict);
            generator
                .Setup(mock => mock.GenerateRouteLink(
                    It.IsAny<ViewContext>(),
                    string.Empty,
                    "Default",
                    "http",
                    "contoso.com",
                    "hello=world",
                    It.IsAny<IDictionary<string, object>>(),
                    null))
                .Returns(new TagBuilder("a"))
                .Verifiable();
            var anchorTagHelper = new AnchorTagHelper(generator.Object)
            {
                Fragment = "hello=world",
                Host = "contoso.com",
                Protocol = "http",
                Route = "Default",
            };

            // Act & Assert
            await anchorTagHelper.ProcessAsync(context, output);
            generator.Verify();
            Assert.Equal("a", output.TagName);
            Assert.Empty(output.Attributes);
            Assert.Empty(output.Content.GetContent());
        }

        [Fact]
        public async Task ProcessAsync_CallsIntoActionLinkWithExpectedParameters()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "a",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "a",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent(string.Empty);

            var generator = new Mock<IHtmlGenerator>();
            generator
                .Setup(mock => mock.GenerateActionLink(
                    It.IsAny<ViewContext>(),
                    string.Empty,
                    "Index",
                    "Home",
                    "http",
                    "contoso.com",
                    "hello=world",
                    It.IsAny<IDictionary<string, object>>(),
                    null))
                .Returns(new TagBuilder("a"))
                .Verifiable();
            var anchorTagHelper = new AnchorTagHelper(generator.Object)
            {
                Action = "Index",
                Controller = "Home",
                Fragment = "hello=world",
                Host = "contoso.com",
                Protocol = "http",
            };

            // Act & Assert
            await anchorTagHelper.ProcessAsync(context, output);
            generator.Verify();
            Assert.Equal("a", output.TagName);
            Assert.Empty(output.Attributes);
            Assert.Empty(output.Content.GetContent());
        }

        [Fact]
        public async Task ProcessAsync_AddsAreaToRouteValuesAndCallsIntoActionLinkWithExpectedParameters()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "a",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "a",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent(string.Empty);

            var generator = new Mock<IHtmlGenerator>();
            var expectedRouteValues = new Dictionary<string, object> { { "area", "Admin" } };

            generator
                .Setup(mock => mock.GenerateActionLink(
                    It.IsAny<ViewContext>(),
                    string.Empty,
                    "Index",
                    "Home",
                    "http",
                    "contoso.com",
                    "hello=world",
                    expectedRouteValues,
                    null))
                .Returns(new TagBuilder("a"))
                .Verifiable();
            var anchorTagHelper = new AnchorTagHelper(generator.Object)
            {
                Action = "Index",
                Controller = "Home",
                Area = "Admin",
                Fragment = "hello=world",
                Host = "contoso.com",
                Protocol = "http",
            };

            // Act
            await anchorTagHelper.ProcessAsync(context, output);

            // Assert
            generator.Verify();

            Assert.Equal("a", output.TagName);
            Assert.Empty(output.Attributes);
            Assert.Empty(output.Content.GetContent());
        }

        [Fact]
        public async Task ProcessAsync_AspAreaOverridesAspRouteArea()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "a",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "a",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent(string.Empty);

            var generator = new Mock<IHtmlGenerator>();
            var expectedRouteValues = new Dictionary<string, object> { { "area", "Admin" } };

            generator
                .Setup(mock => mock.GenerateActionLink(
                    It.IsAny<ViewContext>(),
                    string.Empty,
                    "Index",
                    "Home",
                    "http",
                    "contoso.com",
                    "hello=world",
                    expectedRouteValues,
                    null))
                .Returns(new TagBuilder("a"))
                .Verifiable();
            var anchorTagHelper = new AnchorTagHelper(generator.Object)
            {
                Action = "Index",
                Controller = "Home",
                Area = "Admin",
                Fragment = "hello=world",
                Host = "contoso.com",
                Protocol = "http",
                RouteValues = new Dictionary<string, string> { { "area", "Home" } }
            };

            // Act
            await anchorTagHelper.ProcessAsync(context, output);

            // Assert
            generator.Verify();

            Assert.Equal("a", output.TagName);
            Assert.Empty(output.Attributes);
            Assert.Empty(output.Content.GetContent());
        }

        [Fact]
        public async Task ProcessAsync_EmptyStringOnAspAreaIsPassedThroughToRouteValues()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "a",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "a",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent(string.Empty);

            var generator = new Mock<IHtmlGenerator>();
            var expectedRouteValues = new Dictionary<string, object> { { "area", string.Empty } };

            generator
                .Setup(mock => mock.GenerateActionLink(
                    It.IsAny<ViewContext>(),
                    string.Empty,
                    "Index",
                    "Home",
                    "http",
                    "contoso.com",
                    "hello=world",
                    expectedRouteValues,
                    null))
                .Returns(new TagBuilder("a"))
                .Verifiable();
            var anchorTagHelper = new AnchorTagHelper(generator.Object)
            {
                Action = "Index",
                Controller = "Home",
                Area = string.Empty,
                Fragment = "hello=world",
                Host = "contoso.com",
                Protocol = "http"
            };

            // Act
            await anchorTagHelper.ProcessAsync(context, output);

            // Assert
            generator.Verify();

            Assert.Equal("a", output.TagName);
            Assert.Empty(output.Attributes);
            Assert.Empty(output.Content.GetContent());
        }

        [Fact]
        public async Task ProcessAsync_AddsPageToRouteValuesAndCallsIntoActionLinkWithExpectedParameters()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "a",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "a",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent(string.Empty);

            var generator = new Mock<IHtmlGenerator>();
            object actualRouteValues = null;
            generator
                .Setup(mock => mock.GenerateActionLink(
                    It.IsAny<ViewContext>(),
                    string.Empty,
                    null,
                    null,
                    "http",
                    "contoso.com",
                    "hello=world",
                    It.IsAny<object>(),
                    null))
                .Returns(
                    (ViewContext _,
                    string linkText,
                    string actionName,
                    string controllerName,
                    string protocol,
                    string hostname,
                    string fragment,
                    object routeValues,
                    object htmlAttributes) =>
                    {
                        actualRouteValues = routeValues;
                        return new TagBuilder("a");
                    })
                .Verifiable();
            var anchorTagHelper = new AnchorTagHelper(generator.Object)
            {
                Page = "/User/Home/Index",
                Fragment = "hello=world",
                Host = "contoso.com",
                Protocol = "http",
            };

            // Act
            await anchorTagHelper.ProcessAsync(context, output);

            // Assert
            generator.Verify();

            Assert.NotNull(actualRouteValues);
            var routeValueDictionary = Assert.IsType<RouteValueDictionary>(actualRouteValues);
            Assert.Collection(routeValueDictionary,
                item =>
                {
                    Assert.Equal("page", item.Key);
                    Assert.Equal("/User/Home/Index", item.Value);
                });
        }

        [Fact]
        public async Task ProcessAsync_WithPageSet_AppendsToRouteValues()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "a",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "a",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent(string.Empty);

            var generator = new Mock<IHtmlGenerator>();
            object actualRouteValues = null;
            generator
                .Setup(mock => mock.GenerateActionLink(
                    It.IsAny<ViewContext>(),
                    string.Empty,
                    null,
                    null,
                    "http",
                    "contoso.com",
                    "hello=world",
                    It.IsAny<object>(),
                    null))
                .Returns(
                    (ViewContext _,
                    string linkText,
                    string actionName,
                    string controllerName,
                    string protocol,
                    string hostname,
                    string fragment,
                    object routeValues,
                    object htmlAttributes) =>
                    {
                        actualRouteValues = routeValues;
                        return new TagBuilder("a");
                    })
                .Verifiable();
            var anchorTagHelper = new AnchorTagHelper(generator.Object)
            {
                Page = "/User/Home/Index",
                Fragment = "hello=world",
                Host = "contoso.com",
                Protocol = "http",
                RouteValues = new Dictionary<string, string>
                {
                    { "hello", "world" },
                },
            };

            // Act
            await anchorTagHelper.ProcessAsync(context, output);

            // Assert
            generator.Verify();

            Assert.NotNull(actualRouteValues);
            var routeValueDictionary = Assert.IsType<RouteValueDictionary>(actualRouteValues);
            Assert.Collection(routeValueDictionary,
                item =>
                {
                    Assert.Equal("hello", item.Key);
                    Assert.Equal("world", item.Value);
                },
                item =>
                {
                    Assert.Equal("page", item.Key);
                    Assert.Equal("/User/Home/Index", item.Value);
                });
        }

        [Fact]
        public async Task ProcessAsync_AspPageOverridesAspRoutePage()
        {
            // Arrange
            var context = new TagHelperContext(
                tagName: "a",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "a",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent(string.Empty);

            var generator = new Mock<IHtmlGenerator>();
            var expectedRouteValues = new Dictionary<string, object> { { "page", "/User/Home/Admin" } };

            generator
                .Setup(mock => mock.GenerateActionLink(
                    It.IsAny<ViewContext>(),
                    string.Empty,
                    null,
                    null,
                    "http",
                    "contoso.com",
                    "hello=world",
                    expectedRouteValues,
                    null))
                .Returns(new TagBuilder("a"))
                .Verifiable();
            var anchorTagHelper = new AnchorTagHelper(generator.Object)
            {
                Page = "/User/Home/Admin",
                Fragment = "hello=world",
                Host = "contoso.com",
                Protocol = "http",
                RouteValues = new Dictionary<string, string> { { "page", "/CurrentPage" } }
            };

            // Act
            await anchorTagHelper.ProcessAsync(context, output);

            // Assert
            generator.Verify();

            Assert.Equal("a", output.TagName);
            Assert.Empty(output.Attributes);
            Assert.Empty(output.Content.GetContent());
        }

        [Theory]
        [InlineData("Action")]
        [InlineData("Controller")]
        [InlineData("Route")]
        [InlineData("Protocol")]
        [InlineData("Host")]
        [InlineData("Fragment")]
        [InlineData("asp-route-")]
        [InlineData("Page")]
        public async Task ProcessAsync_ThrowsIfHrefConflictsWithBoundAttributes(string propertyName)
        {
            // Arrange
            var metadataProvider = new EmptyModelMetadataProvider();
            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);

            var anchorTagHelper = new AnchorTagHelper(htmlGenerator);

            var output = new TagHelperOutput(
                "a",
                attributes: new TagHelperAttributeList
                {
                    { "href", "http://www.contoso.com" }
                },
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(null));
            if (propertyName == "asp-route-")
            {
                anchorTagHelper.RouteValues.Add("name", "value");
            }
            else
            {
                typeof(AnchorTagHelper).GetProperty(propertyName).SetValue(anchorTagHelper, "Home");
            }

            var expectedErrorMessage = "Cannot override the 'href' attribute for <a>. An <a> with a specified " +
                "'href' must not have attributes starting with 'asp-route-' or an " +
                "'asp-action', 'asp-controller', 'asp-area', 'asp-route', 'asp-protocol', 'asp-host', " +
                "'asp-fragment', or 'asp-page' attribute.";

            var context = new TagHelperContext(
                tagName: "test",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => anchorTagHelper.ProcessAsync(context, output));

            Assert.Equal(expectedErrorMessage, ex.Message);
        }

        [Theory]
        [InlineData("Action")]
        [InlineData("Controller")]
        public async Task ProcessAsync_ThrowsIfRouteAndActionOrControllerProvided(string propertyName)
        {
            // Arrange
            var metadataProvider = new EmptyModelMetadataProvider();
            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);

            var anchorTagHelper = new AnchorTagHelper(htmlGenerator)
            {
                Route = "Default",
            };

            typeof(AnchorTagHelper).GetProperty(propertyName).SetValue(anchorTagHelper, "Home");
            var output = new TagHelperOutput(
                "a",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(null));
            var expectedErrorMessage = string.Join(
                Environment.NewLine,
                "Cannot determine the 'href' attribute for <a>. The following attributes are mutually exclusive:",
                "asp-route",
                "asp-controller, asp-action",
                "asp-page");

            var context = new TagHelperContext(
                tagName: "test",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => anchorTagHelper.ProcessAsync(context, output));

            Assert.Equal(expectedErrorMessage, ex.Message);
        }

        [Fact]
        public async Task ProcessAsync_ThrowsIfRouteAndPageProvided()
        {
            // Arrange
            var metadataProvider = new EmptyModelMetadataProvider();
            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);

            var anchorTagHelper = new AnchorTagHelper(htmlGenerator)
            {
                Route = "Default",
                Page = "Page",
            };

            var output = new TagHelperOutput(
                "a",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(null));
            var expectedErrorMessage = string.Join(
                Environment.NewLine,
                "Cannot determine the 'href' attribute for <a>. The following attributes are mutually exclusive:",
                "asp-route",
                "asp-controller, asp-action",
                "asp-page");

            var context = new TagHelperContext(
                tagName: "test",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => anchorTagHelper.ProcessAsync(context, output));

            Assert.Equal(expectedErrorMessage, ex.Message);
        }

        [Fact]
        public async Task ProcessAsync_ThrowsIfActionAndPageProvided()
        {
            // Arrange
            var metadataProvider = new EmptyModelMetadataProvider();
            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);

            var anchorTagHelper = new AnchorTagHelper(htmlGenerator)
            {
                Action = "Action",
                Page = "Page",
            };

            var output = new TagHelperOutput(
                "a",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(null));
            var expectedErrorMessage = string.Join(
                Environment.NewLine,
                "Cannot determine the 'href' attribute for <a>. The following attributes are mutually exclusive:",
                "asp-route",
                "asp-controller, asp-action",
                "asp-page");

            var context = new TagHelperContext(
                tagName: "test",
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => anchorTagHelper.ProcessAsync(context, output));

            Assert.Equal(expectedErrorMessage, ex.Message);
        }
    }
}