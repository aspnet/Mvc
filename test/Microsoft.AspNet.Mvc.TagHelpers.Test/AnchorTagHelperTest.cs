// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.AspNet.Razor.TagHelpers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers
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
                getChildContentAsync: useCachedResult =>
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
            var viewContext = TestableHtmlGenerator.GetViewContext(model: null,
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
                allAttributes: new ReadOnlyTagHelperAttributeList<IReadOnlyTagHelperAttribute>(
                    Enumerable.Empty<IReadOnlyTagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "a",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: useCachedResult =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent(string.Empty);

            var generator = new Mock<IHtmlGenerator>(MockBehavior.Strict);
            generator
                .Setup(mock => mock.GenerateRouteLink(
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
            Assert.True(output.Content.IsEmpty);
        }

        [Fact]
        public async Task ProcessAsync_CallsIntoActionLinkWithExpectedParameters()
        {
            // Arrange
            var context = new TagHelperContext(
                allAttributes: new ReadOnlyTagHelperAttributeList<IReadOnlyTagHelperAttribute>(
                    Enumerable.Empty<IReadOnlyTagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "a",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: useCachedResult =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent(string.Empty);

            var generator = new Mock<IHtmlGenerator>();
            generator
                .Setup(mock => mock.GenerateActionLink(
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
            Assert.True(output.Content.IsEmpty);
        }

        [Theory]
        [InlineData("Action")]
        [InlineData("Controller")]
        [InlineData("Route")]
        [InlineData("Protocol")]
        [InlineData("Host")]
        [InlineData("Fragment")]
        [InlineData("asp-route-")]
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
                getChildContentAsync: _ => Task.FromResult<TagHelperContent>(null));
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
                                       "'asp-action', 'asp-controller', 'asp-route', 'asp-protocol', 'asp-host', or " +
                                       "'asp-fragment' attribute.";

            var context = new TagHelperContext(
                allAttributes: new ReadOnlyTagHelperAttributeList<IReadOnlyTagHelperAttribute>(
                    Enumerable.Empty<IReadOnlyTagHelperAttribute>()),
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
                getChildContentAsync: _ => Task.FromResult<TagHelperContent>(null));
            var expectedErrorMessage = "Cannot determine an 'href' attribute for <a>. An <a> with a specified " +
                "'asp-route' must not have an 'asp-action' or 'asp-controller' attribute.";

            var context = new TagHelperContext(
                allAttributes: new ReadOnlyTagHelperAttributeList<IReadOnlyTagHelperAttribute>(
                    Enumerable.Empty<IReadOnlyTagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => anchorTagHelper.ProcessAsync(context, output));

            Assert.Equal(expectedErrorMessage, ex.Message);
        }
    }
}