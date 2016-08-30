// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    public class FormActionTagHelperTest
    {
        [Fact]
        public async Task ProcessAsync_GeneratesExpectedOutput()
        {
            // Arrange
            var expectedTagName = "not-button-or-submit";
            var metadataProvider = new TestModelMetadataProvider();

            var tagHelperContext = new TagHelperContext(
                allAttributes: new TagHelperAttributeList
                {
                    { "id", "my-id" },
                    { "asp-route-name", "value" },
                    { "asp-action", "index" },
                    { "asp-controller", "home" },
                },
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                expectedTagName,
                attributes: new TagHelperAttributeList
                {
                    { "id", "my-id" },
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
                .Setup(mock => mock.Action(It.IsAny<UrlActionContext>())).Returns("home/index").Verifiable();

            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            urlHelperFactory
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelper.Object);

            var htmlGenerator = new TestableHtmlGenerator(metadataProvider, urlHelper.Object);
            var viewContext = TestableHtmlGenerator.GetViewContext(
                model: null,
                htmlGenerator: htmlGenerator,
                metadataProvider: metadataProvider);
            var tagHelper = new FormActionTagHelper(urlHelperFactory.Object)
            {
                Action = "index",
                Controller = "home",
                RouteValues =
                {
                    {  "name", "value" },
                },
                ViewContext = viewContext,
            };

            // Act
            await tagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            urlHelper.Verify();
            Assert.Equal(2, output.Attributes.Count);
            var attribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("id"));
            Assert.Equal("my-id", attribute.Value);
            attribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("formaction"));
            Assert.Equal("home/index", attribute.Value);
            Assert.Equal("Something", output.Content.GetContent());
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Fact]
        public async Task ProcessAsync_CallsIntoRouteLinkWithExpectedParameters()
        {
            // Arrange
            var context = new TagHelperContext(
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "button",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent(string.Empty);

            var urlHelper = new Mock<IUrlHelper>();
            urlHelper
                .Setup(mock => mock.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("home/index").Verifiable();

            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            urlHelperFactory
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelper.Object);

            var metadataProvider = new TestModelMetadataProvider();
            var htmlGenerator = new TestableHtmlGenerator(metadataProvider, urlHelper.Object);
            var viewContext = TestableHtmlGenerator.GetViewContext(
                model: null,
                htmlGenerator: htmlGenerator,
                metadataProvider: metadataProvider);
            var tagHelper = new FormActionTagHelper(urlHelperFactory.Object)
            {
                Route = "Default",
                ViewContext = viewContext,
                RouteValues =
                {
                    { "name", "value" },
                },
            };

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            urlHelper.Verify();
            Assert.Equal("button", output.TagName);
            Assert.Equal(1, output.Attributes.Count);
            var attribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("formaction"));
            Assert.Equal("home/index", attribute.Value);
            Assert.True(output.Content.GetContent().Length == 0);
        }

        [Fact]
        public async Task ProcessAsync_AddsAreaToRouteValuesAndCallsIntoActionLinkWithExpectedParameters()
        {
            // Arrange
            var context = new TagHelperContext(
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "submit",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent(string.Empty);

            var expectedRouteValues = new RouteValueDictionary(new Dictionary<string, string> { { "area", "Admin" } });
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper
                .Setup(mock => mock.Action(It.IsAny<UrlActionContext>()))
                .Returns("admin/dashboard/index")
                .Callback<UrlActionContext>(param => Assert.Equal(param.Values, expectedRouteValues))
                .Verifiable();

            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            urlHelperFactory
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelper.Object);

            var tagHelper = new FormActionTagHelper(urlHelperFactory.Object)
            {
                Action = "Index",
                Controller = "Dashboard",
                Area = "Admin",
            };

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            urlHelper.Verify();
            Assert.Equal("submit", output.TagName);
            Assert.Equal(1, output.Attributes.Count);
            var attribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("formaction"));
            Assert.Equal("admin/dashboard/index", attribute.Value);
            Assert.True(output.Content.GetContent().Length == 0);
        }

        [Fact]
        public async Task ProcessAsync_AspAreaOverridesAspRouteArea()
        {
            // Arrange
            var context = new TagHelperContext(
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "button",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent(string.Empty);

            var expectedRouteValues = new RouteValueDictionary(new Dictionary<string, string> { { "area", "Admin" } });
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper
                .Setup(mock => mock.Action(It.IsAny<UrlActionContext>()))
                .Returns("admin/dashboard/index")
                .Callback<UrlActionContext>(param => Assert.Equal(param.Values, expectedRouteValues))
                .Verifiable();

            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            urlHelperFactory
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelper.Object);

            var tagHelper = new FormActionTagHelper(urlHelperFactory.Object)
            {
                Action = "Index",
                Controller = "Dashboard",
                Area = "Admin",
                RouteValues = new Dictionary<string, string> { { "area", "Home" } }
            };

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            urlHelper.Verify();
            Assert.Equal("button", output.TagName);
            Assert.Equal(1, output.Attributes.Count);
            var attribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("formaction"));
            Assert.Equal("admin/dashboard/index", attribute.Value);
            Assert.True(output.Content.GetContent().Length == 0);
        }

        [Fact]
        public async Task ProcessAsync_EmptyStringOnAspAreaIsPassedThroughToRouteValues()
        {
            // Arrange
            var context = new TagHelperContext(
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");
            var output = new TagHelperOutput(
                "submit",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            output.Content.SetContent(string.Empty);

            var expectedRouteValues = new RouteValueDictionary(new Dictionary<string, string> { { "area", string.Empty } });
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper
                .Setup(mock => mock.Action(It.IsAny<UrlActionContext>()))
                .Returns("admin/dashboard/index")
                .Callback<UrlActionContext>(param => Assert.Equal(param.Values, expectedRouteValues))
                .Verifiable();

            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            urlHelperFactory
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelper.Object);

            var tagHelper = new FormActionTagHelper(urlHelperFactory.Object)
            {
                Action = "Index",
                Controller = "Dashboard",
                Area = string.Empty,
            };

            // Act
            await tagHelper.ProcessAsync(context, output);

            // Assert
            urlHelper.Verify();
            Assert.Equal("submit", output.TagName);
            Assert.Equal(1, output.Attributes.Count);
            var attribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("formaction"));
            Assert.Equal("admin/dashboard/index", attribute.Value);
            Assert.True(output.Content.GetContent().Length == 0);
        }

        [Theory]
        [InlineData("button", "Action")]
        [InlineData("button", "Controller")]
        [InlineData("button", "Route")]
        [InlineData("button", "asp-route-")]
        [InlineData("submit", "Action")]
        [InlineData("submit", "Controller")]
        [InlineData("submit", "Route")]
        [InlineData("submit", "asp-route-")]
        public async Task ProcessAsync_ThrowsIfFormActionConflictsWithBoundAttributes(string tagName, string propertyName)
        {
            // Arrange
            var urlHelperFactory = new Mock<IUrlHelperFactory>().Object;

            var tagHelper = new FormActionTagHelper(urlHelperFactory);

            var output = new TagHelperOutput(
                tagName,
                attributes: new TagHelperAttributeList
                {
                    { "formaction", "my-action" }
                },
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(null));
            if (propertyName == "asp-route-")
            {
                tagHelper.RouteValues.Add("name", "value");
            }
            else
            {
                typeof(FormActionTagHelper).GetProperty(propertyName).SetValue(tagHelper, "Home");
            }

            var expectedErrorMessage = $"Cannot override the 'formaction' attribute for <{tagName}>. <{tagName}> " +
                "elements with a specified 'formaction' must not have attributes starting with 'asp-route-' or an " +
                "'asp-action', 'asp-controller', 'asp-area', or 'asp-route' attribute.";

            var context = new TagHelperContext(
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => tagHelper.ProcessAsync(context, output));

            Assert.Equal(expectedErrorMessage, ex.Message);
        }

        [Theory]
        [InlineData("button", "Action")]
        [InlineData("button", "Controller")]
        [InlineData("submit", "Action")]
        [InlineData("submit", "Controller")]
        public async Task ProcessAsync_ThrowsIfRouteAndActionOrControllerProvided(string tagName, string propertyName)
        {
            // Arrange
            var urlHelperFactory = new Mock<IUrlHelperFactory>().Object;

            var tagHelper = new FormActionTagHelper(urlHelperFactory)
            {
                Route = "Default",
            };

            typeof(FormActionTagHelper).GetProperty(propertyName).SetValue(tagHelper, "Home");
            var output = new TagHelperOutput(
                tagName,
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(null));
            var expectedErrorMessage = $"Cannot determine a 'formaction' attribute for <{tagName}>. <{tagName}> " +
                "elements with a specified 'asp-route' must not have an 'asp-action' or 'asp-controller' attribute.";

            var context = new TagHelperContext(
                allAttributes: new TagHelperAttributeList(
                    Enumerable.Empty<TagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => tagHelper.ProcessAsync(context, output));

            Assert.Equal(expectedErrorMessage, ex.Message);
        }
    }
}