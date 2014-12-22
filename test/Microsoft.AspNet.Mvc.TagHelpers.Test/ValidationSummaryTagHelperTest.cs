﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.PipelineCore;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    public class ValidationSummaryTagHelperTest
    {
        [Fact]
        public async Task ProcessAsync_GeneratesExpectedOutput()
        {
            // Arrange
            var expectedTagName = "not-div";
            var metadataProvider = new DataAnnotationsModelMetadataProvider();
            var validationSummaryTagHelper = new ValidationSummaryTagHelper
            {
                ValidationSummaryValue = "All"
            };

            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var context = new TagHelperContext(allAttributes: new Dictionary<string, object>(),
                                               uniqueId: "test",
                                               getChildContentAsync: () => Task.FromResult("Something"));
            var output = new TagHelperOutput(
                expectedTagName,
                attributes: new Dictionary<string, string>
                {
                    { "class", "form-control" }
                })
            {
                PreContent = expectedPreContent,
                Content = expectedContent,
                PostContent = "Custom Content",
            };

            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);
            Model model = null;
            var viewContext = TestableHtmlGenerator.GetViewContext(model, htmlGenerator, metadataProvider);
            validationSummaryTagHelper.ViewContext = viewContext;
            validationSummaryTagHelper.Generator = htmlGenerator;

            // Act
            await validationSummaryTagHelper.ProcessAsync(context, output);

            // Assert
            Assert.Equal(2, output.Attributes.Count);
            var attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("class"));
            Assert.Equal("form-control validation-summary-valid", attribute.Value);
            attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("data-valmsg-summary"));
            Assert.Equal("true", attribute.Value);
            Assert.Equal(expectedPreContent, output.PreContent);
            Assert.Equal(expectedContent, output.Content);
            Assert.Equal("Custom Content<ul><li style=\"display:none\"></li>" + Environment.NewLine + "</ul>",
                         output.PostContent);
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Fact]
        public async Task ProcessAsync_CallsIntoGenerateValidationSummaryWithExpectedParameters()
        {
            // Arrange
            var validationSummaryTagHelper = new ValidationSummaryTagHelper
            {
                ValidationSummaryValue = "ModelOnly",
            };
            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var expectedPostContent = "original post-content";
            var output = new TagHelperOutput(
                "div",
                attributes: new Dictionary<string, string>())
            {
                PreContent = expectedPreContent,
                Content = expectedContent,
                PostContent = expectedPostContent,
            };
            var expectedViewContext = CreateViewContext();
            var generator = new Mock<IHtmlGenerator>();
            generator
                .Setup(mock => mock.GenerateValidationSummary(expectedViewContext, true, null, null, null))
                .Returns(new TagBuilder("div"))
                .Verifiable();
            validationSummaryTagHelper.ViewContext = expectedViewContext;
            validationSummaryTagHelper.Generator = generator.Object;

            // Act & Assert
            await validationSummaryTagHelper.ProcessAsync(context: null, output: output);

            generator.Verify();
            Assert.Equal("div", output.TagName);
            Assert.Empty(output.Attributes);
            Assert.Equal(expectedPreContent, output.PreContent);
            Assert.Equal(expectedContent, output.Content);
            Assert.Equal(expectedPostContent, output.PostContent);
        }

        [Fact]
        public async Task ProcessAsync_MergesTagBuilderFromGenerateValidationSummary()
        {
            // Arrange
            var validationSummaryTagHelper = new ValidationSummaryTagHelper
            {
                ValidationSummaryValue = "ModelOnly"
            };
            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var output = new TagHelperOutput(
                "div",
                attributes: new Dictionary<string, string>())
            {
                PreContent = expectedPreContent,
                Content = expectedContent,
                PostContent = "Content of validation summary"
            };
            var tagBuilder = new TagBuilder("span2")
            {
                InnerHtml = "New HTML"
            };

            tagBuilder.Attributes.Add("data-foo", "bar");
            tagBuilder.Attributes.Add("data-hello", "world");
            tagBuilder.Attributes.Add("anything", "something");

            var generator = new Mock<IHtmlGenerator>(MockBehavior.Strict);
            generator
                .Setup(mock => mock.GenerateValidationSummary(
                    It.IsAny<ViewContext>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .Returns(tagBuilder);
            var viewContext = CreateViewContext();
            validationSummaryTagHelper.ViewContext = viewContext;
            validationSummaryTagHelper.Generator = generator.Object;

            // Act
            await validationSummaryTagHelper.ProcessAsync(context: null, output: output);

            // Assert
            Assert.Equal("div", output.TagName);
            Assert.Equal(3, output.Attributes.Count);
            var attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("data-foo"));
            Assert.Equal("bar", attribute.Value);
            attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("data-hello"));
            Assert.Equal("world", attribute.Value);
            attribute = Assert.Single(output.Attributes, kvp => kvp.Key.Equals("anything"));
            Assert.Equal("something", attribute.Value);
            Assert.Equal(expectedPreContent, output.PreContent);
            Assert.Equal(expectedContent, output.Content);
            Assert.Equal("Content of validation summaryNew HTML", output.PostContent);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task ProcessAsync_DoesNothingIfNullOrEmptyValidationSummaryValue(string validationSummaryValue)
        {
            // Arrange
            var validationSummaryTagHelper = new ValidationSummaryTagHelper
            {
                ValidationSummaryValue = validationSummaryValue
            };
            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var expectedPostContent = "original post-content";
            var output = new TagHelperOutput(
                "div",
                attributes: new Dictionary<string, string>())
            {
                PreContent = expectedPreContent,
                Content = expectedContent,
                PostContent = expectedPostContent,
            };

            var generator = new Mock<IHtmlGenerator>(MockBehavior.Strict);
            var viewContext = CreateViewContext();
            validationSummaryTagHelper.ViewContext = viewContext;
            validationSummaryTagHelper.Generator = generator.Object;

            // Act
            await validationSummaryTagHelper.ProcessAsync(context: null, output: output);

            // Assert
            Assert.Equal("div", output.TagName);
            Assert.Empty(output.Attributes);
            Assert.Equal(expectedPreContent, output.PreContent);
            Assert.Equal(expectedContent, output.Content);
            Assert.Equal(expectedPostContent, output.PostContent);
        }

        [Theory]
        [InlineData("All")]
        [InlineData("all")]
        [InlineData("ModelOnly")]
        [InlineData("modelonly")]
        public async Task ProcessAsync_GeneratesValidationSummaryWhenNotNone_IgnoresCase(string validationSummary)
        {
            // Arrange
            var validationSummaryTagHelper = new ValidationSummaryTagHelper
            {
                ValidationSummaryValue = validationSummary
            };
            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var output = new TagHelperOutput(
                "div",
                attributes: new Dictionary<string, string>())
            {
                PreContent = expectedPreContent,
                Content = expectedContent,
                PostContent = "Content of validation message",
            };
            var tagBuilder = new TagBuilder("span2")
            {
                InnerHtml = "New HTML"
            };

            var generator = new Mock<IHtmlGenerator>();
            generator
                .Setup(mock => mock.GenerateValidationSummary(
                    It.IsAny<ViewContext>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>()))
                .Returns(tagBuilder)
                .Verifiable();
            var viewContext = CreateViewContext();
            validationSummaryTagHelper.ViewContext = viewContext;
            validationSummaryTagHelper.Generator = generator.Object;

            // Act
            await validationSummaryTagHelper.ProcessAsync(context: null, output: output);

            // Assert
            Assert.Equal("div", output.TagName);
            Assert.Empty(output.Attributes);
            Assert.Equal(expectedPreContent, output.PreContent);
            Assert.Equal(expectedContent, output.Content);
            Assert.Equal("Content of validation messageNew HTML", output.PostContent);
            generator.Verify();
        }

        [Theory]
        [InlineData("None")]
        [InlineData("none")]
        public async Task ProcessAsync_DoesNotGenerateValidationSummaryWhenNone_IgnoresCase(string validationSummary)
        {
            // Arrange
            var validationSummaryTagHelper = new ValidationSummaryTagHelper
            {
                ValidationSummaryValue = validationSummary
            };
            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var expectedPostContent = "original post-content";
            var output = new TagHelperOutput(
                "div",
                attributes: new Dictionary<string, string>())
            {
                PreContent = expectedPreContent,
                Content = expectedContent,
                PostContent = expectedPostContent,
            };
            var tagBuilder = new TagBuilder("span2")
            {
                InnerHtml = "New HTML"
            };

            var generator = new Mock<IHtmlGenerator>(MockBehavior.Strict);

            // Act
            await validationSummaryTagHelper.ProcessAsync(context: null, output: output);

            // Assert
            Assert.Equal("div", output.TagName);
            Assert.Empty(output.Attributes);
            Assert.Equal(expectedPreContent, output.PreContent);
            Assert.Equal(expectedContent, output.Content);
            Assert.Equal(expectedPostContent, output.PostContent);
        }

        [Fact]
        public async Task ProcessAsync_ThrowsWhenInvalidValidationSummaryValue()
        {
            // Arrange
            var validationSummaryTagHelper = new ValidationSummaryTagHelper
            {
                ValidationSummaryValue = "Hello World"
            };
            var output = new TagHelperOutput(
                "div",
                attributes: new Dictionary<string, string>());
            var expectedViewContext = CreateViewContext();
            var expectedMessage = "Cannot parse 'asp-validation-summary' value 'Hello World' for <div>. Acceptable " +
                                  "values are 'All', 'ModelOnly' and 'None'.";

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => validationSummaryTagHelper.ProcessAsync(context: null, output: output));

            // Assert
            Assert.Equal(expectedMessage, ex.Message);
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
                    new EmptyModelMetadataProvider()),
                TextWriter.Null);
        }

        private class Model
        {
            public string Text { get; set; }
        }
    }
}