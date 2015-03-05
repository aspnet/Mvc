﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.WebEncoders;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    public class LabelTagHelperTest
    {
        // Model (List<Model> or Model instance), container type (Model or NestModel), model accessor,
        // property path, TagHelperOutput.Content values.
        public static TheoryData<object, Type, Func<object>, string, TagHelperOutputContent> TestDataSet
        {
            get
            {
                var modelWithNull = new Model
                {
                    NestedModel = new NestedModel
                    {
                        Text = null,
                    },
                    Text = null,
                };
                var modelWithText = new Model
                {
                    NestedModel = new NestedModel
                    {
                        Text = "inner text",
                    },
                    Text = "outer text",
                };
                var models = new List<Model>
                {
                    modelWithNull,
                    modelWithText,
                };

                return new TheoryData<object, Type, Func<object>, string, TagHelperOutputContent>
                {
                    { null, typeof(Model), () => null, "Text",
                        new TagHelperOutputContent(string.Empty, Environment.NewLine, Environment.NewLine, "Text") },
                    { null, typeof(Model), () => null, "Text",
                        new TagHelperOutputContent(Environment.NewLine, string.Empty, "Text", "Text") },

                    { modelWithNull, typeof(Model), () => modelWithNull.Text, "Text",
                        new TagHelperOutputContent(string.Empty, Environment.NewLine, Environment.NewLine, "Text") },
                    { modelWithNull, typeof(Model), () => modelWithNull.Text, "Text",
                        new TagHelperOutputContent(Environment.NewLine, string.Empty, "Text", "Text") },
                    { modelWithNull, typeof(Model), () => modelWithNull.Text, "Text",
                        new TagHelperOutputContent(Environment.NewLine, "Hello World", "Hello World", "Text") },
                    { modelWithText, typeof(Model), () => modelWithText.Text, "Text",
                        new TagHelperOutputContent(string.Empty, Environment.NewLine, Environment.NewLine, "Text") },
                    { modelWithText, typeof(Model), () => modelWithText.Text, "Text",
                        new TagHelperOutputContent(Environment.NewLine, string.Empty, "Text", "Text") },
                    { modelWithText, typeof(Model), () => modelWithText.Text, "Text",
                        new TagHelperOutputContent(Environment.NewLine, "Hello World", "Hello World", "Text") },
                    { modelWithText, typeof(Model), () => modelWithNull.Text, "Text",
                        new TagHelperOutputContent(string.Empty, "Hello World", "Hello World", "Text") },
                    { modelWithText, typeof(Model), () => modelWithText.Text, "Text",
                        new TagHelperOutputContent(string.Empty, "Hello World", "Hello World", "Text") },
                    { modelWithText, typeof(Model), () => modelWithNull.Text, "Text",
                        new TagHelperOutputContent("Hello World", string.Empty, "Hello World", "Text") },
                    { modelWithText, typeof(Model), () => modelWithText.Text, "Text",
                        new TagHelperOutputContent("Hello World", string.Empty, "Hello World", "Text") },
                    { modelWithText, typeof(Model), () => modelWithNull.Text, "Text",
                        new TagHelperOutputContent("Hello World1", "Hello World2", "Hello World2", "Text") },
                    { modelWithText, typeof(Model), () => modelWithText.Text, "Text",
                        new TagHelperOutputContent("Hello World1", "Hello World2", "Hello World2", "Text") },

                    { modelWithNull, typeof(NestedModel), () => modelWithNull.NestedModel.Text, "NestedModel.Text",
                        new TagHelperOutputContent(Environment.NewLine, string.Empty, "Text", "NestedModel_Text") },
                    { modelWithNull, typeof(NestedModel), () => modelWithNull.NestedModel.Text, "NestedModel.Text",
                        new TagHelperOutputContent(Environment.NewLine, "Hello World", "Hello World", "NestedModel_Text") },
                    { modelWithNull, typeof(NestedModel), () => modelWithNull.NestedModel.Text, "NestedModel.Text",
                        new TagHelperOutputContent(string.Empty, Environment.NewLine, Environment.NewLine, "NestedModel_Text") },
                    { modelWithText, typeof(NestedModel), () => modelWithText.NestedModel.Text, "NestedModel.Text",
                        new TagHelperOutputContent(Environment.NewLine, string.Empty, "Text", "NestedModel_Text") },
                    { modelWithText, typeof(NestedModel), () => modelWithText.NestedModel.Text, "NestedModel.Text",
                        new TagHelperOutputContent(Environment.NewLine, "Hello World", "Hello World", "NestedModel_Text") },
                    { modelWithText, typeof(NestedModel), () => modelWithText.NestedModel.Text, "NestedModel.Text",
                        new TagHelperOutputContent(string.Empty, Environment.NewLine, Environment.NewLine, "NestedModel_Text") },
                    { modelWithNull, typeof(NestedModel), () => modelWithNull.NestedModel.Text, "NestedModel.Text",
                        new TagHelperOutputContent(string.Empty, "Hello World", "Hello World", "NestedModel_Text") },
                    { modelWithText, typeof(NestedModel), () => modelWithText.NestedModel.Text, "NestedModel.Text",
                        new TagHelperOutputContent(string.Empty, "Hello World", "Hello World", "NestedModel_Text") },
                    { modelWithNull, typeof(NestedModel), () => modelWithNull.NestedModel.Text, "NestedModel.Text",
                        new TagHelperOutputContent("Hello World", string.Empty, "Hello World", "NestedModel_Text") },
                    { modelWithText, typeof(NestedModel), () => modelWithText.NestedModel.Text, "NestedModel.Text",
                        new TagHelperOutputContent("Hello World", string.Empty, "Hello World", "NestedModel_Text") },
                    { modelWithNull, typeof(NestedModel), () => modelWithNull.NestedModel.Text, "NestedModel.Text",
                        new TagHelperOutputContent("Hello World1", "Hello World2", "Hello World2", "NestedModel_Text") },
                    { modelWithText, typeof(NestedModel), () => modelWithText.NestedModel.Text, "NestedModel.Text",
                        new TagHelperOutputContent("Hello World1", "Hello World2", "Hello World2", "NestedModel_Text") },

                    // Note: Tests cases below here will not work in practice due to current limitations on indexing
                    // into ModelExpressions. Will be fixed in https://github.com/aspnet/Mvc/issues/1345.
                    { models, typeof(Model), () => models[0].Text, "[0].Text",
                        new TagHelperOutputContent(Environment.NewLine, string.Empty, "Text", "z0__Text") },
                    { models, typeof(Model), () => models[0].Text, "[0].Text",
                        new TagHelperOutputContent(Environment.NewLine, "Hello World", "Hello World", "z0__Text") },
                    { models, typeof(Model), () => models[0].Text, "[0].Text",
                        new TagHelperOutputContent(string.Empty, Environment.NewLine, Environment.NewLine, "z0__Text") },
                    { models, typeof(Model), () => models[1].Text, "[1].Text",
                        new TagHelperOutputContent(Environment.NewLine, string.Empty, "Text", "z1__Text") },
                    { models, typeof(Model), () => models[1].Text, "[1].Text",
                        new TagHelperOutputContent(Environment.NewLine, "Hello World", "Hello World", "z1__Text") },
                    { models, typeof(Model), () => models[1].Text, "[1].Text",
                        new TagHelperOutputContent(string.Empty, Environment.NewLine, Environment.NewLine, "z1__Text") },
                    { models, typeof(Model), () => models[0].Text, "[0].Text",
                        new TagHelperOutputContent(string.Empty, "Hello World", "Hello World", "z0__Text") },
                    { models, typeof(Model), () => models[1].Text, "[1].Text",
                        new TagHelperOutputContent(string.Empty, "Hello World", "Hello World", "z1__Text") },
                    { models, typeof(Model), () => models[0].Text, "[0].Text",
                        new TagHelperOutputContent("Hello World", string.Empty, "Hello World", "z0__Text") },
                    { models, typeof(Model), () => models[1].Text, "[1].Text",
                        new TagHelperOutputContent("Hello World", string.Empty, "Hello World", "z1__Text") },
                    { models, typeof(Model), () => models[0].Text, "[0].Text",
                        new TagHelperOutputContent("Hello World1", "Hello World2", "Hello World2", "z0__Text") },
                    { models, typeof(Model), () => models[1].Text, "[1].Text",
                        new TagHelperOutputContent("Hello World1", "Hello World2", "Hello World2", "z1__Text") },

                    { models, typeof(NestedModel), () => models[0].NestedModel.Text, "[0].NestedModel.Text",
                        new TagHelperOutputContent(Environment.NewLine, string.Empty, "Text", "z0__NestedModel_Text") },
                    { models, typeof(NestedModel), () => models[0].NestedModel.Text, "[0].NestedModel.Text",
                        new TagHelperOutputContent(Environment.NewLine, "Hello World", "Hello World", "z0__NestedModel_Text") },
                    { models, typeof(NestedModel), () => models[0].NestedModel.Text, "[0].NestedModel.Text",
                        new TagHelperOutputContent(string.Empty, Environment.NewLine, Environment.NewLine, "z0__NestedModel_Text") },
                    { models, typeof(NestedModel), () => models[1].NestedModel.Text, "[1].NestedModel.Text",
                        new TagHelperOutputContent(Environment.NewLine, string.Empty, "Text", "z1__NestedModel_Text") },
                    { models, typeof(NestedModel), () => models[1].NestedModel.Text, "[1].NestedModel.Text",
                        new TagHelperOutputContent(Environment.NewLine, "Hello World", "Hello World", "z1__NestedModel_Text") },
                    { models, typeof(NestedModel), () => models[1].NestedModel.Text, "[1].NestedModel.Text",
                        new TagHelperOutputContent(string.Empty, Environment.NewLine, Environment.NewLine, "z1__NestedModel_Text") },
                    { models, typeof(NestedModel), () => models[0].NestedModel.Text, "[0].NestedModel.Text",
                        new TagHelperOutputContent(string.Empty, "Hello World", "Hello World", "z0__NestedModel_Text") },
                    { models, typeof(NestedModel), () => models[1].NestedModel.Text, "[1].NestedModel.Text",
                        new TagHelperOutputContent(string.Empty, "Hello World", "Hello World", "z1__NestedModel_Text") },
                    { models, typeof(NestedModel), () => models[0].NestedModel.Text, "[0].NestedModel.Text",
                        new TagHelperOutputContent("Hello World", string.Empty, "Hello World", "z0__NestedModel_Text") },
                    { models, typeof(NestedModel), () => models[1].NestedModel.Text, "[1].NestedModel.Text",
                        new TagHelperOutputContent("Hello World", string.Empty, "Hello World", "z1__NestedModel_Text") },
                    { models, typeof(NestedModel), () => models[0].NestedModel.Text, "[0].NestedModel.Text",
                        new TagHelperOutputContent("Hello World1", "Hello World2", "Hello World2", "z0__NestedModel_Text") },
                    { models, typeof(NestedModel), () => models[1].NestedModel.Text, "[1].NestedModel.Text",
                        new TagHelperOutputContent("Hello World1", "Hello World2", "Hello World2", "z1__NestedModel_Text") },
                };
            }
        }

        [Theory]
        [MemberData(nameof(TestDataSet))]
        public async Task ProcessAsync_GeneratesExpectedOutput(
            object model,
            Type containerType,
            Func<object> modelAccessor,
            string propertyPath,
            TagHelperOutputContent tagHelperOutputContent)
        {
            // Arrange
            var expectedTagName = "not-label";
            var expectedAttributes = new Dictionary<string, string>
            {
                { "class", "form-control" },
                { "for", tagHelperOutputContent.ExpectedId }
            };
            var metadataProvider = new TestModelMetadataProvider();

            var containerMetadata = metadataProvider.GetMetadataForType(containerType);
            var containerExplorer = metadataProvider.GetModelExplorerForType(containerType, model);

            var propertyMetadata = metadataProvider.GetMetadataForProperty(containerType, "Text");
            var modelExplorer = containerExplorer.GetExplorerForExpression(propertyMetadata, modelAccessor());

            var modelExpression = new ModelExpression(propertyPath, modelExplorer);
            var tagHelper = new LabelTagHelper
            {
                For = modelExpression,
            };
            var expectedPreContent = "original pre-content";
            var expectedPostContent = "original post-content";

            var tagHelperContext = new TagHelperContext(
                allAttributes: new Dictionary<string, object>(),
                items: new Dictionary<object, object>(),
                uniqueId: "test",
                getChildContentAsync: () =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent(tagHelperOutputContent.OriginalChildContent);
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            var htmlAttributes = new Dictionary<string, string>
            {
                { "class", "form-control" },
            };
            var output = new TagHelperOutput(expectedTagName, htmlAttributes, new HtmlEncoder());
            output.PreContent.SetContent(expectedPreContent);
            output.PostContent.SetContent(expectedPostContent);

            // LabelTagHelper checks IsContentModified so we don't want to forcibly set it if 
            // tagHelperOutputContent.OriginalContent is going to be null or empty.
            if (!string.IsNullOrEmpty(tagHelperOutputContent.OriginalContent))
            {
                output.Content.SetContent(tagHelperOutputContent.OriginalContent);
            }

            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);
            var viewContext = TestableHtmlGenerator.GetViewContext(model, htmlGenerator, metadataProvider);
            tagHelper.ViewContext = viewContext;
            tagHelper.Generator = htmlGenerator;

            // Act
            await tagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            Assert.Equal(expectedAttributes, output.Attributes);
            Assert.Equal(expectedPreContent, output.PreContent.GetContent());
            Assert.Equal(tagHelperOutputContent.ExpectedContent, output.Content.GetContent());
            Assert.Equal(expectedPostContent, output.PostContent.GetContent());
            Assert.False(output.SelfClosing);
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Fact]
        public async Task TagHelper_LeavesOutputUnchanged_IfForNotBound2()
        {
            // Arrange
            var expectedAttributes = new Dictionary<string, string>
            {
                { "class", "form-control" },
            };
            var expectedPreContent = "original pre-content";
            var expectedContent = "original content";
            var expectedPostContent = "original post-content";
            var expectedTagName = "label";

            var metadataProvider = new TestModelMetadataProvider();
            var modelExplorer = metadataProvider
                .GetModelExplorerForType(typeof(Model), model: null)
                .GetExplorerForProperty(nameof(Model.Text));
            var modelExpression = new ModelExpression(nameof(Model.Text), modelExplorer);

            var tagHelper = new LabelTagHelper();

            var tagHelperContext = new TagHelperContext(
                allAttributes: new Dictionary<string, object>(),
                items: new Dictionary<object, object>(),
                uniqueId: "test",
                getChildContentAsync: () =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetContent("Something");
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
            var output = new TagHelperOutput(expectedTagName, expectedAttributes, new HtmlEncoder());
            output.PreContent.SetContent(expectedPreContent);
            output.Content.SetContent(expectedContent);
            output.PostContent.SetContent(expectedPostContent);

            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);
            Model model = null;
            var viewContext = TestableHtmlGenerator.GetViewContext(model, htmlGenerator, metadataProvider);
            tagHelper.ViewContext = viewContext;
            tagHelper.Generator = htmlGenerator;

            // Act
            await tagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            Assert.Equal(expectedAttributes, output.Attributes);
            Assert.Equal(expectedPreContent, output.PreContent.GetContent());
            Assert.Equal(expectedContent, output.Content.GetContent());
            Assert.Equal(expectedPostContent, output.PostContent.GetContent());
            Assert.Equal(expectedTagName, output.TagName);
        }

        public class TagHelperOutputContent
        {
            public TagHelperOutputContent(string originalChildContent,
                                          string outputContent,
                                          string expectedContent,
                                          string expectedId)
            {
                OriginalChildContent = originalChildContent;
                OriginalContent = outputContent;
                ExpectedContent = expectedContent;
                ExpectedId = expectedId;
            }

            public string OriginalChildContent { get; set; }

            public string OriginalContent { get; set; }

            public string ExpectedContent { get; set; }

            public string ExpectedId { get; set; }
        }

        private class Model
        {
            public string Text { get; set; }

            public NestedModel NestedModel { get; set; }
        }

        private class NestedModel
        {
            public string Text { get; set; }
        }
    }
}