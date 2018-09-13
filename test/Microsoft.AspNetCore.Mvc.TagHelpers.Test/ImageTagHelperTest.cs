// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.WebEncoders.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    public class ImageTagHelperTest
    {
        [Theory]
        [InlineData(null, "test.jpg", "test.jpg")]
        [InlineData("abcd.jpg", "test.jpg", "test.jpg")]
        [InlineData(null, "~/test.jpg", "virtualRoot/test.jpg")]
        [InlineData("abcd.jpg", "~/test.jpg", "virtualRoot/test.jpg")]
        public void Process_SrcDefaultsToTagHelperOutputSrcAttributeAddedByOtherTagHelper(
            string src,
            string srcOutput,
            string expectedSrcPrefix)
        {
            // Arrange
            var allAttributes = new TagHelperAttributeList(
                new TagHelperAttributeList
                {
                    { "alt", new HtmlString("Testing") },
                    { "asp-append-version", true },
                });
            var context = MakeTagHelperContext(allAttributes);
            var outputAttributes = new TagHelperAttributeList
                {
                    { "alt", new HtmlString("Testing") },
                    { "src", srcOutput },
                };
            var output = new TagHelperOutput(
                "img",
                outputAttributes,
                getChildContentAsync: (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(
                    new DefaultTagHelperContent()));
            var urlHelper = new Mock<IUrlHelper>();

            // Ensure expanded path does not look like an absolute path on Linux, avoiding
            // https://github.com/aspnet/External/issues/21
            urlHelper
                .Setup(urlhelper => urlhelper.Content(It.IsAny<string>()))
                .Returns(new Func<string, string>(url => url.Replace("~/", "virtualRoot/")));
            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            urlHelperFactory
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelper.Object);

            var helper = GetHelper(urlHelperFactory: urlHelperFactory.Object);
            helper.AppendVersion = true;
            helper.Src = src;

            // Act
            helper.Process(context, output);

            // Assert
            Assert.Equal(
                expectedSrcPrefix + "?v=f4OxZX_x_FO5LcGBSKHWXfwtSx-j1ncoSt3SABJtkGk",
                (string)output.Attributes["src"].Value,
                StringComparer.Ordinal);
        }

        [Fact]
        public void PreservesOrderOfSourceAttributesWhenRun()
        {
            // Arrange
            var context = MakeTagHelperContext(
                attributes: new TagHelperAttributeList
                {
                    { "alt", new HtmlString("alt text") },
                    { "data-extra", new HtmlString("something") },
                    { "title", new HtmlString("Image title") },
                    { "src", "testimage.png" },
                    { "asp-append-version", "true" }
                });
            var output = MakeImageTagHelperOutput(
                attributes: new TagHelperAttributeList
                {
                    { "alt", new HtmlString("alt text") },
                    { "data-extra", new HtmlString("something") },
                    { "title", new HtmlString("Image title") },
                });

            var expectedOutput = MakeImageTagHelperOutput(
                attributes: new TagHelperAttributeList
                {
                    { "alt", new HtmlString("alt text") },
                    { "data-extra", new HtmlString("something") },
                    { "title", new HtmlString("Image title") },
                    { "src", "testimage.png?v=f4OxZX_x_FO5LcGBSKHWXfwtSx-j1ncoSt3SABJtkGk" }
                });

            var helper = GetHelper();
            helper.Src = "testimage.png";
            helper.AppendVersion = true;

            // Act
            helper.Process(context, output);

            // Assert
            Assert.Equal(expectedOutput.TagName, output.TagName);
            Assert.Equal(4, output.Attributes.Count);

            for (var i = 0; i < expectedOutput.Attributes.Count; i++)
            {
                var expectedAttribute = expectedOutput.Attributes[i];
                var actualAttribute = output.Attributes[i];
                Assert.Equal(expectedAttribute.Name, actualAttribute.Name);
                Assert.Equal(expectedAttribute.Value.ToString(), actualAttribute.Value.ToString());
            }
        }

        [Fact]
        public void RendersImageTag_AddsFileVersion()
        {
            // Arrange
            var context = MakeTagHelperContext(
                attributes: new TagHelperAttributeList
                {
                    { "alt", new HtmlString("Alt image text") },
                    { "src", "/images/test-image.png" },
                    { "asp-append-version", "true" }
                });
            var output = MakeImageTagHelperOutput(attributes: new TagHelperAttributeList
            {
                { "alt", new HtmlString("Alt image text") },
            });
            var hostingEnvironment = MakeHostingEnvironment();
            var viewContext = MakeViewContext();

            var helper = GetHelper();
            helper.Src = "/images/test-image.png";
            helper.AppendVersion = true;

            // Act
            helper.Process(context, output);

            // Assert
            Assert.True(output.Content.GetContent().Length == 0);
            Assert.Equal("img", output.TagName);
            Assert.Equal(2, output.Attributes.Count);
            var srcAttribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("src"));
            Assert.Equal("/images/test-image.png?v=f4OxZX_x_FO5LcGBSKHWXfwtSx-j1ncoSt3SABJtkGk", srcAttribute.Value);
        }

        [Fact]
        public void RendersImageTag_DoesNotAddFileVersion()
        {
            // Arrange
            var context = MakeTagHelperContext(
                attributes: new TagHelperAttributeList
                {
                    { "alt", new HtmlString("Alt image text") },
                    { "src", "/images/test-image.png" },
                    { "asp-append-version", "false" }
                });
            var output = MakeImageTagHelperOutput(attributes: new TagHelperAttributeList
            {
                { "alt", new HtmlString("Alt image text") },
            });
            var hostingEnvironment = MakeHostingEnvironment();
            var viewContext = MakeViewContext();

            var helper = GetHelper();
            helper.Src = "/images/test-image.png";

            // Act
            helper.Process(context, output);

            // Assert
            Assert.True(output.Content.GetContent().Length == 0);
            Assert.Equal("img", output.TagName);
            Assert.Equal(2, output.Attributes.Count);
            var srcAttribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("src"));
            Assert.Equal("/images/test-image.png", srcAttribute.Value);
        }

        [Fact]
        public void RendersImageTag_AddsFileVersion_WithRequestPathBase()
        {
            // Arrange
            var context = MakeTagHelperContext(
                attributes: new TagHelperAttributeList
                {
                    { "alt", new HtmlString("alt text") },
                    { "src", "/bar/images/image.jpg" },
                    { "asp-append-version", "true" },
                });
            var output = MakeImageTagHelperOutput(attributes: new TagHelperAttributeList
            {
                { "alt", new HtmlString("alt text") },
            });
            var hostingEnvironment = MakeHostingEnvironment();
            var viewContext = MakeViewContext("/bar");

            var helper = GetHelper();
            helper.Src = "/bar/images/image.jpg";
            helper.AppendVersion = true;

            // Act
            helper.Process(context, output);
            // Assert
            Assert.True(output.Content.GetContent().Length == 0);
            Assert.Equal("img", output.TagName);
            Assert.Equal(2, output.Attributes.Count);
            var srcAttribute = Assert.Single(output.Attributes, attr => attr.Name.Equals("src"));
            Assert.Equal("/bar/images/image.jpg?v=f4OxZX_x_FO5LcGBSKHWXfwtSx-j1ncoSt3SABJtkGk", srcAttribute.Value);
        }

        private static ViewContext MakeViewContext(string requestPathBase = null)
        {
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            if (requestPathBase != null)
            {
                actionContext.HttpContext.Request.PathBase = new Http.PathString(requestPathBase);
            }

            var metadataProvider = new EmptyModelMetadataProvider();
            var viewData = new ViewDataDictionary(metadataProvider, new ModelStateDictionary());
            var viewContext = new ViewContext(
                actionContext,
                Mock.Of<IView>(),
                viewData,
                Mock.Of<ITempDataDictionary>(),
                TextWriter.Null,
                new HtmlHelperOptions());

            return viewContext;
        }

        private static ImageTagHelper GetHelper(
            IHostingEnvironment hostingEnvironment = null,
            IUrlHelperFactory urlHelperFactory = null,
            ViewContext viewContext = null)
        {
            hostingEnvironment = hostingEnvironment ?? MakeHostingEnvironment();
            urlHelperFactory = urlHelperFactory ?? MakeUrlHelperFactory();
            viewContext = viewContext ?? MakeViewContext();

            var cacheProvider = new TagHelperMemoryCacheProvider();
            var fileVersionProvider = new DefaultFileVersionProvider(hostingEnvironment, cacheProvider);

            return new ImageTagHelper(
                hostingEnvironment,
                new TagHelperMemoryCacheProvider(),
                fileVersionProvider,
                new HtmlTestEncoder(),
                urlHelperFactory)
            {
                ViewContext = viewContext,
            };
        }

        private static TagHelperContext MakeTagHelperContext(
            TagHelperAttributeList attributes)
        {
            return new TagHelperContext(
                tagName: "image",
                allAttributes: attributes,
                items: new Dictionary<object, object>(),
                uniqueId: Guid.NewGuid().ToString("N"));
        }

        private static TagHelperOutput MakeImageTagHelperOutput(TagHelperAttributeList attributes)
        {
            attributes = attributes ?? new TagHelperAttributeList();

            return new TagHelperOutput(
                "img",
                attributes,
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent(default(string));
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
        }

        private static IHostingEnvironment MakeHostingEnvironment()
        {
            var emptyDirectoryContents = new Mock<IDirectoryContents>();
            emptyDirectoryContents.Setup(dc => dc.GetEnumerator())
                .Returns(Enumerable.Empty<IFileInfo>().GetEnumerator());
            var mockFile = new Mock<IFileInfo>();
            mockFile.SetupGet(f => f.Exists).Returns(true);
            mockFile
                .Setup(m => m.CreateReadStream())
                .Returns(() => new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")));
            var mockFileProvider = new Mock<IFileProvider>();
            mockFileProvider.Setup(fp => fp.GetDirectoryContents(It.IsAny<string>()))
                .Returns(emptyDirectoryContents.Object);
            mockFileProvider.Setup(fp => fp.GetFileInfo(It.IsAny<string>()))
                .Returns(mockFile.Object);
            mockFileProvider.Setup(fp => fp.Watch(It.IsAny<string>()))
                .Returns(new TestFileChangeToken());
            var hostingEnvironment = new Mock<IHostingEnvironment>();
            hostingEnvironment.Setup(h => h.WebRootFileProvider).Returns(mockFileProvider.Object);

            return hostingEnvironment.Object;
        }

        private static IUrlHelperFactory MakeUrlHelperFactory()
        {
            var urlHelper = new Mock<IUrlHelper>();

            urlHelper
                .Setup(helper => helper.Content(It.IsAny<string>()))
                .Returns(new Func<string, string>(url => url));

            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            urlHelperFactory
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelper.Object);

            return urlHelperFactory.Object;
        }
    }
}
