// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Extensions.WebEncoders.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Razor.TagHelpers
{
    public class UrlResolutionTagHelperTest
    {
        public static TheoryData ResolvableUrlData
        {
            get
            {
                // url, expectedHref
                return new TheoryData<object, object>
                {
                   { "~/home/index.html", "/approot/home/index.html" },
                   { "  ~/home/index.html", "/approot/home/index.html" },
                   { new HtmlString("~/home/index.html"), new HtmlString("HtmlEncode[[/approot/]]home/index.html") },
                   {
                       new HtmlString("  ~/home/index.html"),
                       new HtmlString("HtmlEncode[[/approot/]]home/index.html")
                   },
                   {
                       "~/home/index.html ~/secondValue/index.html",
                       "/approot/home/index.html ~/secondValue/index.html"
                   },
                   {
                       new HtmlString("~/home/index.html ~/secondValue/index.html"),
                       new HtmlString("HtmlEncode[[/approot/]]home/index.html ~/secondValue/index.html")
                   },
                };
            }
        }

        [Theory]
        [MemberData(nameof(ResolvableUrlData))]
        public void Process_ResolvesTildeSlashValues(object url, object expectedHref)
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                tagName: "a",
                attributes: new TagHelperAttributeList
                {
                    { "href", url }
                },
                getChildContentAsync: _ => Task.FromResult<TagHelperContent>(null));
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(urlHelper => urlHelper.Content(It.IsAny<string>()))
                .Returns(new Func<string, string>(value => "/approot" + value.Substring(1)));
            var tagHelper = new UrlResolutionTagHelper(urlHelperMock.Object, new HtmlTestEncoder());

            var context = new TagHelperContext(
                allAttributes: new ReadOnlyTagHelperAttributeList<IReadOnlyTagHelperAttribute>(
                    Enumerable.Empty<IReadOnlyTagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            // Act
            tagHelper.Process(context, tagHelperOutput);

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal("href", attribute.Name, StringComparer.Ordinal);
            Assert.IsType(expectedHref.GetType(), url);
            Assert.Equal(expectedHref.ToString(), attribute.Value.ToString());
            Assert.False(attribute.Minimized);
        }

        public static TheoryData UnresolvableUrlData
        {
            get
            {
                // url
                return new TheoryData<object>
                {
                   { "/home/index.html" },
                   { "~ /home/index.html" },
                   { "/home/index.html ~/second/wontresolve.html" },
                   { "  ~\\home\\index.html" },
                   { "~\\/home/index.html" },
                   { new HtmlString("/home/index.html") },
                   { new HtmlString("~ /home/index.html") },
                   { new HtmlString("/home/index.html ~/second/wontresolve.html") },
                   { new HtmlString("~\\home\\index.html") },
                   { new HtmlString("~\\/home/index.html") },
                };
            }
        }

        [Theory]
        [MemberData(nameof(UnresolvableUrlData))]
        public void Process_DoesNotResolveNonTildeSlashValues(object url)
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                tagName: "a",
                attributes: new TagHelperAttributeList
                {
                    { "href", url }
                },
                getChildContentAsync: _ => Task.FromResult<TagHelperContent>(null));
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(urlHelper => urlHelper.Content(It.IsAny<string>()))
                .Returns("approot/home/index.html");
            var tagHelper = new UrlResolutionTagHelper(urlHelperMock.Object, htmlEncoder: null);

            var context = new TagHelperContext(
                allAttributes: new ReadOnlyTagHelperAttributeList<IReadOnlyTagHelperAttribute>(
                    Enumerable.Empty<IReadOnlyTagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            // Act
            tagHelper.Process(context, tagHelperOutput);

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal("href", attribute.Name, StringComparer.Ordinal);
            Assert.Equal(url, attribute.Value);
            Assert.False(attribute.Minimized);
        }

        [Fact]
        public void Process_IgnoresNonHtmlStringOrStringValues()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                tagName: "a",
                attributes: new TagHelperAttributeList
                {
                    { "href", true }
                },
                getChildContentAsync: _ => Task.FromResult<TagHelperContent>(null));
            var tagHelper = new UrlResolutionTagHelper(urlHelper: null, htmlEncoder: null);

            var context = new TagHelperContext(
                allAttributes: new ReadOnlyTagHelperAttributeList<IReadOnlyTagHelperAttribute>(
                    Enumerable.Empty<IReadOnlyTagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            // Act
            tagHelper.Process(context, tagHelperOutput);

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal("href", attribute.Name, StringComparer.Ordinal);
            Assert.Equal(true, attribute.Value);
            Assert.False(attribute.Minimized);
        }

        [Fact]
        public void Process_ThrowsWhenEncodingNeededAndIUrlHelperActsUnexpectedly()
        {
            // Arrange
            var relativeUrl = "~/home/index.html";
            var expectedExceptionMessage = Resources.FormatCouldNotResolveApplicationRelativeUrl_TagHelper(
                relativeUrl,
                nameof(IUrlHelper),
                nameof(IUrlHelper.Content),
                "removeTagHelper",
                typeof(UrlResolutionTagHelper).FullName,
                typeof(UrlResolutionTagHelper).GetTypeInfo().Assembly.GetName().Name);
            var tagHelperOutput = new TagHelperOutput(
                tagName: "a",
                attributes: new TagHelperAttributeList
                {
                    { "href", new HtmlString(relativeUrl) }
                },
                getChildContentAsync: _ => Task.FromResult<TagHelperContent>(null));
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(urlHelper => urlHelper.Content(It.IsAny<string>()))
                .Returns("UnexpectedResult");
            var tagHelper = new UrlResolutionTagHelper(urlHelperMock.Object, htmlEncoder: null);

            var context = new TagHelperContext(
                allAttributes: new ReadOnlyTagHelperAttributeList<IReadOnlyTagHelperAttribute>(
                    Enumerable.Empty<IReadOnlyTagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => tagHelper.Process(context, tagHelperOutput));
            Assert.Equal(expectedExceptionMessage, exception.Message, StringComparer.Ordinal);
        }

        public static TheoryData ResolvableSrcSetAttributeData
        {
            get
            {
                // url, expectedHref
                return new TheoryData<object, object>
                {
                   { "~/content/image.png 1x", "/approot/content/image.png 1x" },
                   { "  ~/content/image.png 1x, ~/content/image@2.png 2x", "/approot/content/image.png 1x, /approot/content/image@2.png 2x" },
                   { "~/content/image.png ~/secondValue/image.png", "/approot/content/image.png ~/secondValue/image.png" },
                   { "~/content/image.png 200w, ~/content/image@2.png 400w", "/approot/content/image.png 200w, /approot/content/image@2.png 400w" },
                   {
                        new HtmlString("~/content/image.png 1x"),
                        new HtmlString("HtmlEncode[[/approot/]]content/image.png 1x")
                   },
                   {
                        new HtmlString("~/content/image.png 1x, ~/content/image@2.png 2x"),
                        new HtmlString("HtmlEncode[[/approot/]]content/image.png 1x, HtmlEncode[[/approot/]]content/image@2.png 2x")
                   },
                   {
                       new HtmlString("  ~/content/image.png 1x,    ~/content/image.png 2x"),
                       new HtmlString("HtmlEncode[[/approot/]]content/image.png 1x, HtmlEncode[[/approot/]]content/image.png 2x")
                   },
                   {
                       new HtmlString("~/content/image.png ~/secondValue/image.png"),
                       new HtmlString("HtmlEncode[[/approot/]]content/image.png ~/secondValue/image.png")
                   }

                };
            }
        }

        [Theory]
        [MemberData(nameof(ResolvableSrcSetAttributeData))]
        public void Process_ResolvesImgSrcSetWithMultipleUrls(object url, object expectedHref)
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                tagName: "img",
                attributes: new TagHelperAttributeList
                {
                    { "srcset", url }
                },
                getChildContentAsync: _ => Task.FromResult<TagHelperContent>(null));
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(urlHelper => urlHelper.Content(It.IsAny<string>()))
                .Returns(new Func<string, string>(value => "/approot" + value.Substring(1)));
            var tagHelper = new UrlResolutionTagHelper(urlHelperMock.Object, new HtmlTestEncoder());

            var context = new TagHelperContext(
                allAttributes: new ReadOnlyTagHelperAttributeList<IReadOnlyTagHelperAttribute>(
                    Enumerable.Empty<IReadOnlyTagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            // Act
            tagHelper.Process(context, tagHelperOutput);

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal("srcset", attribute.Name, StringComparer.Ordinal);
            Assert.IsType(expectedHref.GetType(), url);
            Assert.Equal(expectedHref.ToString(), attribute.Value.ToString());
            Assert.False(attribute.Minimized);

        }

        public static TheoryData ResolvableSrcAttributeData
        {
            get
            {
                // url, expectedHref
                return new TheoryData<object, object>
                {
                   { "~/content/image.png 1x", "/approot/content/image.png 1x" },
                   { "~/content/image.png 1x, ~/content/image@2.png 2x", "/approot/content/image.png 1x, ~/content/image@2.png 2x" },
                   { "  ~/content/image.png 1x, ~/content/image@2.png 2x", "/approot/content/image.png 1x, ~/content/image@2.png 2x" },
                   { "~/content/image.png ~/secondValue/image.png", "/approot/content/image.png ~/secondValue/image.png" },
                   { "~/content/image.png 200w, ~/content/image@2.png 400w", "/approot/content/image.png 200w, ~/content/image@2.png 400w" },
                   {
                        new HtmlString("~/content/image.png 1x, ~/content/image@2.png 2x"),
                        new HtmlString("HtmlEncode[[/approot/]]content/image.png 1x, ~/content/image@2.png 2x") },
                   {
                       new HtmlString("  ~/content/image.png 1x,    ~/content/image.png 2x"),
                       new HtmlString("HtmlEncode[[/approot/]]content/image.png 1x,    ~/content/image.png 2x")
                   },
                   {
                       new HtmlString("~/content/image.png ~/secondValue/image.png"),
                       new HtmlString("HtmlEncode[[/approot/]]content/image.png ~/secondValue/image.png")
                   }

                };
            }
        }

        [Theory]
        [MemberData(nameof(ResolvableSrcAttributeData))]
        public void Process_AttributesWhichDontSupportMultipleUrlFormatShouldIgnoreMultipleTildSlash(object url, object expectedHref)
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                tagName: "img",
                attributes: new TagHelperAttributeList
                {
                    { "src", url }
                },
                getChildContentAsync: _ => Task.FromResult<TagHelperContent>(null));
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock
                .Setup(urlHelper => urlHelper.Content(It.IsAny<string>()))
                .Returns(new Func<string, string>(value => "/approot" + value.Substring(1)));
            var tagHelper = new UrlResolutionTagHelper(urlHelperMock.Object, new HtmlTestEncoder());

            var context = new TagHelperContext(
                allAttributes: new ReadOnlyTagHelperAttributeList<IReadOnlyTagHelperAttribute>(
                    Enumerable.Empty<IReadOnlyTagHelperAttribute>()),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

            // Act
            tagHelper.Process(context, tagHelperOutput);

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal("src", attribute.Name, StringComparer.Ordinal);
            Assert.IsType(expectedHref.GetType(), url);
            Assert.Equal(expectedHref.ToString(), attribute.Value.ToString());
            Assert.False(attribute.Minimized);

        }
    }
}
