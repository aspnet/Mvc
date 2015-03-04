﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.WebEncoders;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    public class ValidationMessageTagHelperTest
    {
        [Fact]
        public async Task ProcessAsync_GeneratesExpectedOutput()
        {
            // Arrange
            var expectedTagName = "not-span";
            var metadataProvider = new DataAnnotationsModelMetadataProvider();
            var modelExpression = CreateModelExpression("Name");
            var validationMessageTagHelper = new ValidationMessageTagHelper
            {
                For = modelExpression
            };

            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var expectedPostContent = "original post-content";

            var tagHelperContext = new TagHelperContext(
                allAttributes: new Dictionary<string, object>
                {
                    { "id", "myvalidationmessage" },
                    { "for", modelExpression },
                },
                items: new Dictionary<object, object>(),
                uniqueId: "test",
                getChildContentAsync: () => {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.Append("Something");
                    return Task.FromResult((TagHelperContent)tagHelperContent);
                });
            var output = new TagHelperOutput(
                expectedTagName,
                attributes: new Dictionary<string, string>
                {
                    { "id", "myvalidationmessage" }
                },
                htmlEncoder: new HtmlEncoder());
            output.PreContent.Append(expectedPreContent);
            output.Content.Append(expectedContent);
            output.PostContent.Append(expectedPostContent);
            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);
            var viewContext = TestableHtmlGenerator.GetViewContext(model: null,
                                                                   htmlGenerator: htmlGenerator,
                                                                   metadataProvider: metadataProvider);
            validationMessageTagHelper.ViewContext = viewContext;
            validationMessageTagHelper.Generator = htmlGenerator;

            // Act
            await validationMessageTagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            Assert.Equal(4, output.Attributes.Count);
            var attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("id"));
            Assert.Equal("myvalidationmessage", attribute.Value);
            attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("class"));
            Assert.Equal("field-validation-valid", attribute.Value);
            attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("data-valmsg-for"));
            Assert.Equal("Name", attribute.Value);
            attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("data-valmsg-replace"));
            Assert.Equal("true", attribute.Value);
            Assert.Equal(expectedPreContent, output.PreContent.ToString());
            Assert.Equal(expectedContent, output.Content.ToString());
            Assert.Equal(expectedPostContent, output.PostContent.ToString());
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Fact]
        public async Task ProcessAsync_CallsIntoGenerateValidationMessageWithExpectedParameters()
        {
            // Arrange
            var validationMessageTagHelper = new ValidationMessageTagHelper
            {
                For = CreateModelExpression("Hello")
            };
            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var expectedPostContent = "original post-content";
            var context = new TagHelperContext(
                allAttributes: new Dictionary<string, object>(),
                items: new Dictionary<object, object>(),
                uniqueId: "test",
                getChildContentAsync: () => {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.Append("Something");
                    return Task.FromResult((TagHelperContent)tagHelperContent);
                });
            var output = new TagHelperOutput(
                "span",
                attributes: new Dictionary<string, string>(),
                htmlEncoder: new HtmlEncoder());
            output.PreContent.Append(expectedPreContent);
            output.Content.Append(expectedContent);
            output.PostContent.Append(expectedPostContent);

            var expectedViewContext = CreateViewContext();
            var generator = new Mock<IHtmlGenerator>();
            generator
                .Setup(mock =>
                    mock.GenerateValidationMessage(expectedViewContext, "Hello", null, null, null))
                .Returns(new TagBuilder("span", new HtmlEncoder()))
                .Verifiable();
            validationMessageTagHelper.Generator = generator.Object;
            validationMessageTagHelper.ViewContext = expectedViewContext;

            // Act & Assert
            await validationMessageTagHelper.ProcessAsync(context, output: output);

            generator.Verify();
            Assert.Equal("span", output.TagName);
            Assert.Empty(output.Attributes);
            Assert.Equal(expectedPreContent, output.PreContent.ToString());
            Assert.Equal(expectedContent, output.Content.ToString());
            Assert.Equal(expectedPostContent, output.PostContent.ToString());
        }

        [Theory]
        [InlineData("Content of validation message", "Some Content", "Some Content")]
        [InlineData("\r\n  \r\n", "\r\n Something Else \r\n", "\r\n Something Else \r\n")]
        [InlineData("\r\n  \r\n", "Some Content", "Some Content")]
        public async Task ProcessAsync_DoesNotOverrideOutputContent(
            string childContent, string outputContent, string expectedOutputContent)
        {
            // Arrange
            var validationMessageTagHelper = new ValidationMessageTagHelper
            {
                For = CreateModelExpression("Hello")
            };
            var output = new TagHelperOutput(
                "span",
                attributes: new Dictionary<string, string>(),
                htmlEncoder: new HtmlEncoder());
            output.Content.Append(outputContent);

            var context = new TagHelperContext(
                allAttributes: new Dictionary<string, object>(),
                items: new Dictionary<object, object>(),
                uniqueId: "test",
                getChildContentAsync: () => {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.Append(childContent);
                    return Task.FromResult((TagHelperContent)tagHelperContent);
                });
            var tagBuilder = new TagBuilder("span2", new HtmlEncoder())
            {
                InnerHtml = "New HTML"
            };
            tagBuilder.Attributes.Add("data-foo", "bar");
            tagBuilder.Attributes.Add("data-hello", "world");

            var generator = new Mock<IHtmlGenerator>(MockBehavior.Strict);
            var setup = generator
                .Setup(mock => mock.GenerateValidationMessage(
                    It.IsAny<ViewContext>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .Returns(tagBuilder);
            var viewContext = CreateViewContext();
            validationMessageTagHelper.ViewContext = viewContext;
            validationMessageTagHelper.Generator = generator.Object;

            // Act
            await validationMessageTagHelper.ProcessAsync(context, output: output);

            // Assert
            Assert.Equal("span", output.TagName);
            Assert.Equal(2, output.Attributes.Count);
            var attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("data-foo"));
            Assert.Equal("bar", attribute.Value);
            attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("data-hello"));
            Assert.Equal("world", attribute.Value);
            Assert.Equal(expectedOutputContent, output.Content.ToString());
        }

        [Theory]
        [InlineData("Content of validation message", "Content of validation message")]
        [InlineData("\r\n  \r\n", "New HTML")]
        public async Task ProcessAsync_MergesTagBuilderFromGenerateValidationMessage(
            string childContent, string expectedOutputContent)
        {
            // Arrange
            var validationMessageTagHelper = new ValidationMessageTagHelper
            {
                For = CreateModelExpression("Hello")
            };
            var output = new TagHelperOutput(
                "span",
                attributes: new Dictionary<string, string>(),
                htmlEncoder: new HtmlEncoder());

            var context = new TagHelperContext(
                allAttributes: new Dictionary<string, object>(),
                items: new Dictionary<object, object>(),
                uniqueId: "test",
                getChildContentAsync: () => {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.Append(childContent);
                    return Task.FromResult((TagHelperContent)tagHelperContent);
                });
            var tagBuilder = new TagBuilder("span2", new HtmlEncoder())
            {
                InnerHtml = "New HTML"
            };
            tagBuilder.Attributes.Add("data-foo", "bar");
            tagBuilder.Attributes.Add("data-hello", "world");

            var generator = new Mock<IHtmlGenerator>(MockBehavior.Strict);
            var setup = generator
                .Setup(mock => mock.GenerateValidationMessage(
                    It.IsAny<ViewContext>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .Returns(tagBuilder);
            var viewContext = CreateViewContext();
            validationMessageTagHelper.ViewContext = viewContext;
            validationMessageTagHelper.Generator = generator.Object;

            // Act
            await validationMessageTagHelper.ProcessAsync(context, output: output);

            // Assert
            Assert.Equal("span", output.TagName);
            Assert.Equal(2, output.Attributes.Count);
            var attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("data-foo"));
            Assert.Equal("bar", attribute.Value);
            attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("data-hello"));
            Assert.Equal("world", attribute.Value);
            Assert.Equal(expectedOutputContent, output.Content.ToString());
        }

        [Fact]
        public async Task ProcessAsync_DoesNothingIfNullFor()
        {
            // Arrange
            var validationMessageTagHelper = new ValidationMessageTagHelper();
            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var expectedPostContent = "original post-content";
            var output = new TagHelperOutput(
                "span",
                attributes: new Dictionary<string, string>(),
                htmlEncoder: new HtmlEncoder());
            output.PreContent.Append(expectedPreContent);
            output.Content.Append(expectedContent);
            output.PostContent.Append(expectedPostContent);

            var viewContext = CreateViewContext();
            var generator = new Mock<IHtmlGenerator>(MockBehavior.Strict);
            validationMessageTagHelper.ViewContext = viewContext;
            validationMessageTagHelper.Generator = generator.Object;

            // Act
            await validationMessageTagHelper.ProcessAsync(context: null, output: output);

            // Assert
            Assert.Equal("span", output.TagName);
            Assert.Empty(output.Attributes);
            Assert.Equal(expectedPreContent, output.PreContent.ToString());
            Assert.Equal(expectedContent, output.Content.ToString());
            Assert.Equal(expectedPostContent, output.PostContent.ToString());
        }

        private static ModelExpression CreateModelExpression(string name)
        {
            var modelMetadataProvider = new Mock<IModelMetadataProvider>().Object;
            return new ModelExpression(
                name,
                new ModelExplorer(
                    modelMetadataProvider,
                    new ModelMetadata(
                        modelMetadataProvider,
                        containerType: null,
                        modelType: typeof(object),
                        propertyName: string.Empty),
                    model: null));
        }

        private static ViewContext CreateViewContext()
        {
            var actionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor());

            return new ViewContext(
                actionContext,
                Mock.Of<IView>(),
                new ViewDataDictionary(
                    new DataAnnotationsModelMetadataProvider()),
                TextWriter.Null);
        }
    }
}