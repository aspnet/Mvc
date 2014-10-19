﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
    public class SelectTagHelperTest
    {
        // Model (List<Model> or Model instance), container type (Model or NestModel), model accessor,
        // property path / id, expected content.
        public static TheoryData<object, Type, Func<object>, NameAndId, string> GeneratesExpectedDataSet
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
                var noneSelected = "<option></option>" + Environment.NewLine +
                    "<option>outer text</option>" + Environment.NewLine +
                    "<option>inner text</option>" + Environment.NewLine +
                    "<option>other text</option>" + Environment.NewLine;
                var innerSelected = "<option></option>" + Environment.NewLine +
                    "<option>outer text</option>" + Environment.NewLine +
                    "<option selected=\"selected\">inner text</option>" + Environment.NewLine +
                    "<option>other text</option>" + Environment.NewLine;
                var outerSelected = "<option></option>" + Environment.NewLine +
                    "<option selected=\"selected\">outer text</option>" + Environment.NewLine +
                    "<option>inner text</option>" + Environment.NewLine +
                    "<option>other text</option>" + Environment.NewLine;

                return new TheoryData<object, Type, Func<object>, NameAndId, string>
                {
                    { null, typeof(Model), () => null, new NameAndId("Text", "Text"), noneSelected },

                    { modelWithNull, typeof(Model), () => modelWithNull.Text,
                        new NameAndId("Text", "Text"), noneSelected },
                    { modelWithText, typeof(Model), () => modelWithText.Text,
                        new NameAndId("Text", "Text"), outerSelected },

                    { modelWithNull, typeof(NestedModel), () => modelWithNull.NestedModel.Text,
                        new NameAndId("NestedModel.Text", "NestedModel_Text"), noneSelected },
                    { modelWithText, typeof(NestedModel), () => modelWithText.NestedModel.Text,
                        new NameAndId("NestedModel.Text", "NestedModel_Text"), innerSelected },

                    // Top-level indexing does not work end-to-end due to code generation issue #1345.
                    // TODO: Remove above comment when #1345 is fixed.
                    { models, typeof(Model), () => models[0].Text,
                        new NameAndId("[0].Text", "z0__Text"), noneSelected },
                    { models, typeof(NestedModel), () => models[0].NestedModel.Text,
                        new NameAndId("[0].NestedModel.Text", "z0__NestedModel_Text"), noneSelected },

                    // Skip last two test cases because DefaultHtmlGenerator evaluates expression name against
                    // ViewData, not using ModelMetadata.Model. ViewData.Eval() handles simple property paths and some
                    // dictionary lookups, but not indexing into an array or list. Will file a follow-up bug on this...
                    ////{ models, typeof(Model), () => models[1].Text,
                    ////    new NameAndId("[1].Text", "z1__Text"), outerSelected },
                    ////{ models, typeof(NestedModel), () => models[1].NestedModel.Text,
                    ////    new NameAndId("[1].NestedModel.Text", "z1__NestedModel_Text"), innerSelected },
                };
            }
        }

        // Items value, expected value (passed to generator)
        public static TheoryData<IEnumerable<SelectListItem>, IEnumerable<SelectListItem>> ItemsDataSet
        {
            get
            {
                var arrayItems = new[] { new SelectListItem() };
                var listItems = new List<SelectListItem>();
                var multiItems = new MultiSelectList(Enumerable.Range(0, 5));
                var selectItems = new SelectList(Enumerable.Range(0, 5));

                return new TheoryData<IEnumerable<SelectListItem>, IEnumerable<SelectListItem>>
                {
                    { null, Enumerable.Empty<SelectListItem>() },
                    { arrayItems, arrayItems },
                    { listItems, listItems },
                    { multiItems, multiItems },
                    { selectItems, selectItems },
                };
            }
        }

        // Model type, model, allowMultiple expected value
        public static TheoryData<Type, object, bool> RealModelTypeDataSet
        {
            get
            {
                return new TheoryData<Type, object, bool>
                {
                    { typeof(object), string.Empty, false },
                    { typeof(string), null, false },
                    { typeof(int?), null, false },
                    { typeof(int), 23, false },
                    { typeof(IEnumerable), null, true },
                    { typeof(IEnumerable<string>), null, true },
                    { typeof(List<int>), null, true },
                    { typeof(object), new[] { "", "", "" }, true },
                    { typeof(object), new List<string>(), true },
                };
            }
        }

        [Theory]
        [MemberData(nameof(GeneratesExpectedDataSet))]
        public async Task ProcessAsync_GeneratesExpectedOutput(
            object model,
            Type containerType,
            Func<object> modelAccessor,
            NameAndId nameAndId,
            string ignored)
        {
            // Arrange
            var originalAttributes = new Dictionary<string, string>
            {
                { "class", "form-control" },
            };
            var originalContent = "original content";
            var originalTagName = "not-select";

            var expectedAttributes = new Dictionary<string, string>(originalAttributes)
            {
                { "id", nameAndId.Id },
                { "name", nameAndId.Name },
                { "valid", "from validation attributes" },
            };
            var expectedContent = originalContent;
            var expectedTagName = "select";

            var metadataProvider = new DataAnnotationsModelMetadataProvider();

            // Property name is either nameof(Model.Text) or nameof(NestedModel.Text).
            var metadata = metadataProvider.GetMetadataForProperty(modelAccessor, containerType, propertyName: "Text");
            var modelExpression = new ModelExpression(nameAndId.Name, metadata);

            var tagHelperContext = new TagHelperContext(new Dictionary<string, object>());
            var output = new TagHelperOutput(originalTagName, originalAttributes, expectedContent)
            {
                SelfClosing = true,
            };

            var htmlGenerator = new TestableHtmlGenerator(metadataProvider)
            {
                ValidationAttributes =
                {
                    {  "valid", "from validation attributes" },
                }
            };
            var viewContext = TestableHtmlGenerator.GetViewContext(model, htmlGenerator, metadataProvider);
            var tagHelper = new SelectTagHelper
            {
                For = modelExpression,
                Generator = htmlGenerator,
                ViewContext = viewContext,
            };

            // Act
            await tagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            Assert.Equal(expectedAttributes, output.Attributes);
            Assert.Equal(expectedContent, output.Content);
            Assert.False(output.SelfClosing);
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Theory]
        [MemberData(nameof(GeneratesExpectedDataSet))]
        public async Task ProcessAsync_GeneratesExpectedOutput_WithItems(
            object model,
            Type containerType,
            Func<object> modelAccessor,
            NameAndId nameAndId,
            string expectedOptions)
        {
            // Arrange
            var originalAttributes = new Dictionary<string, string>
            {
                { "class", "form-control" },
            };
            var originalContent = "original content";
            var originalTagName = "not-select";

            var expectedAttributes = new Dictionary<string, string>(originalAttributes)
            {
                { "id", nameAndId.Id },
                { "name", nameAndId.Name },
                { "valid", "from validation attributes" },
            };
            var expectedContent = originalContent + expectedOptions;
            var expectedTagName = "select";

            var metadataProvider = new DataAnnotationsModelMetadataProvider();

            // Property name is either nameof(Model.Text) or nameof(NestedModel.Text).
            var metadata = metadataProvider.GetMetadataForProperty(modelAccessor, containerType, propertyName: "Text");
            var modelExpression = new ModelExpression(nameAndId.Name, metadata);

            var tagHelperContext = new TagHelperContext(new Dictionary<string, object>());
            var output = new TagHelperOutput(originalTagName, originalAttributes, originalContent)
            {
                SelfClosing = true,
            };

            var items = new SelectList(new[] { "", "outer text", "inner text", "other text" });
            var htmlGenerator = new TestableHtmlGenerator(metadataProvider)
            {
                ValidationAttributes =
                {
                    {  "valid", "from validation attributes" },
                }
            };
            var viewContext = TestableHtmlGenerator.GetViewContext(model, htmlGenerator, metadataProvider);
            var tagHelper = new SelectTagHelper
            {
                For = modelExpression,
                Generator = htmlGenerator,
                Items = items,
                ViewContext = viewContext,
            };

            // Act
            await tagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            Assert.Equal(expectedAttributes, output.Attributes);
            Assert.Equal(expectedContent, output.Content);
            Assert.False(output.SelfClosing);
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Theory]
        [MemberData(nameof(ItemsDataSet))]
        public async Task TagHelper_CallsGeneratorWithExpectedValues_Items(
            IEnumerable<SelectListItem> inputItems,
            IEnumerable<SelectListItem> expectedItems)
        {
            // Arrange
            var contextAttributes = new Dictionary<string, object>();
            var originalAttributes = new Dictionary<string, string>();
            var content = "original content";
            var propertyName = "Property1";
            var tagName = "original tag name";

            var tagHelperContext = new TagHelperContext(contextAttributes);
            var output = new TagHelperOutput(tagName, originalAttributes, content);

            // TODO: In real (model => model) scenario, ModelExpression should have name "" and
            // TemplateInfo.HtmlFieldPrefix should be "Property1" but empty ModelExpression name is not currently
            // supported, see #1408.
            var metadataProvider = new EmptyModelMetadataProvider();
            string model = null;
            var metadata = metadataProvider.GetMetadataForType(() => model, typeof(string));
            var modelExpression = new ModelExpression(propertyName, metadata);

            var htmlGenerator = new Mock<IHtmlGenerator>(MockBehavior.Strict);
            var viewContext = TestableHtmlGenerator.GetViewContext(model, htmlGenerator.Object, metadataProvider);
            htmlGenerator
                .Setup(real => real.GenerateSelect(
                    viewContext,
                    metadata,
                    null,         // optionLabel
                    propertyName, // name
                    expectedItems,
                    false,
                    null))        // htmlAttributes
                .Returns((TagBuilder)null)
                .Verifiable();

            var tagHelper = new SelectTagHelper
            {
                For = modelExpression,
                Items = inputItems,
                Generator = htmlGenerator.Object,
                ViewContext = viewContext,
            };

            // Act
            await tagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            htmlGenerator.Verify();
        }

        [Theory]
        [InlineData(null, false)] // allowMultiple determined by string datatype.
        [InlineData("", false)]   // allowMultiple determined by string datatype.
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("multiple", true)]
        [InlineData("Multiple", true)]
        [InlineData("MULTIPLE", true)]
        public async Task TagHelper_CallsGeneratorWithExpectedValues_Multiple(string multiple, bool allowMultiple)
        {
            // Arrange
            var contextAttributes = new Dictionary<string, object>
            {
                // Attribute will be restored if value matches "multiple".
                { "multiple", multiple },
            };
            var originalAttributes = new Dictionary<string, string>();
            var content = "original content";
            var propertyName = "Property1";
            var tagName = "original tag name";

            var tagHelperContext = new TagHelperContext(contextAttributes);
            var output = new TagHelperOutput(tagName, originalAttributes, content);

            var metadataProvider = new EmptyModelMetadataProvider();
            string model = null;
            var metadata = metadataProvider.GetMetadataForType(() => model, typeof(string));
            var modelExpression = new ModelExpression(propertyName, metadata);

            var htmlGenerator = new Mock<IHtmlGenerator>(MockBehavior.Strict);
            var viewContext = TestableHtmlGenerator.GetViewContext(model, htmlGenerator.Object, metadataProvider);
            htmlGenerator
                .Setup(real => real.GenerateSelect(
                    viewContext,
                    metadata,
                    null,         // optionLabel
                    propertyName, // name
                    It.IsAny<IEnumerable<SelectListItem>>(),
                    allowMultiple,
                    null))        // htmlAttributes
                .Returns((TagBuilder)null)
                .Verifiable();

            var tagHelper = new SelectTagHelper
            {
                For = modelExpression,
                Generator = htmlGenerator.Object,
                Multiple = multiple,
                ViewContext = viewContext,
            };

            // Act
            await tagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            htmlGenerator.Verify();
        }

        [Theory]
        [MemberData(nameof(RealModelTypeDataSet))]
        public async Task TagHelper_CallsGeneratorWithExpectedValues_RealModelType(
            Type modelType,
            object model,
            bool allowMultiple)
        {
            // Arrange
            var contextAttributes = new Dictionary<string, object>();
            var originalAttributes = new Dictionary<string, string>();
            var content = "original content";
            var propertyName = "Property1";
            var tagName = "original tag name";

            var tagHelperContext = new TagHelperContext(contextAttributes);
            var output = new TagHelperOutput(tagName, originalAttributes, content);

            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForType(() => model, modelType);
            var modelExpression = new ModelExpression(propertyName, metadata);

            var htmlGenerator = new Mock<IHtmlGenerator>(MockBehavior.Strict);
            var viewContext = TestableHtmlGenerator.GetViewContext(model, htmlGenerator.Object, metadataProvider);
            htmlGenerator
                .Setup(real => real.GenerateSelect(
                    viewContext,
                    metadata,
                    null,         // optionLabel
                    propertyName, // name
                    It.IsAny<IEnumerable<SelectListItem>>(),
                    allowMultiple,
                    null))        // htmlAttributes
                .Returns((TagBuilder)null)
                .Verifiable();

            var tagHelper = new SelectTagHelper
            {
                For = modelExpression,
                Generator = htmlGenerator.Object,
                ViewContext = viewContext,
            };

            // Act
            await tagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            htmlGenerator.Verify();
        }

        [Theory]
        [InlineData("multiple")]
        [InlineData("mUlTiPlE")]
        [InlineData("MULTIPLE")]
        public async Task TagHelper_RestoresMultiple_IfForNotBound(string attributeName)
        {
            // Arrange
            var contextAttributes = new Dictionary<string, object>
            {
                { attributeName, "I'm more than one" },
            };
            var originalAttributes = new Dictionary<string, string>
            {
                { "class", "form-control" },
                { "size", "2" },
            };
            var expectedAttributes = new Dictionary<string, string>(originalAttributes);
            expectedAttributes[attributeName] = (string)contextAttributes[attributeName];
            var expectedContent = "original content";
            var expectedTagName = "original tag name";

            var tagHelperContext = new TagHelperContext(contextAttributes);
            var output = new TagHelperOutput(expectedTagName, originalAttributes, expectedContent)
            {
                SelfClosing = true,
            };

            var metadataProvider = new EmptyModelMetadataProvider();
            var htmlGenerator = new TestableHtmlGenerator(metadataProvider)
            {
                ValidationAttributes =
                {
                    {  "valid", "from validation attributes" },
                }
            };

            Model model = null;
            var viewContext = TestableHtmlGenerator.GetViewContext(model, htmlGenerator, metadataProvider);
            var tagHelper = new SelectTagHelper
            {
                Generator = htmlGenerator,
                Multiple = "I'm more than one",
                ViewContext = viewContext,
            };

            // Act
            await tagHelper.ProcessAsync(tagHelperContext, output);

            // Assert
            Assert.Equal(expectedAttributes, output.Attributes);
            Assert.Equal(expectedContent, output.Content);
            Assert.True(output.SelfClosing);
            Assert.Equal(expectedTagName, output.TagName);
        }

        [Fact]
        public async Task TagHelper_Throws_IfForNotBoundButItemsIs()
        {
            // Arrange
            var contextAttributes = new Dictionary<string, object>();
            var originalAttributes = new Dictionary<string, string>();
            var content = "original content";
            var tagName = "original tag name";
            var expectedMessage = "Cannot determine body for <select>. 'items' must be null if 'for' is null.";

            var tagHelperContext = new TagHelperContext(contextAttributes);
            var output = new TagHelperOutput(tagName, originalAttributes, content);

            var metadataProvider = new EmptyModelMetadataProvider();
            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);

            Model model = null;
            var viewContext = TestableHtmlGenerator.GetViewContext(model, htmlGenerator, metadataProvider);
            var tagHelper = new SelectTagHelper
            {
                Generator = htmlGenerator,
                Items = Enumerable.Empty<SelectListItem>(),
                ViewContext = viewContext,
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => tagHelper.ProcessAsync(tagHelperContext, output));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("0")]
        [InlineData("1")]
        [InlineData("__true")]
        [InlineData("false__")]
        [InlineData("__Multiple")]
        [InlineData("Multiple__")]
        public async Task TagHelper_Throws_IfMultipleInvalid(string multiple)
        {
            // Arrange
            var contextAttributes = new Dictionary<string, object>();
            var originalAttributes = new Dictionary<string, string>();
            var content = "original content";
            var tagName = "original tag name";
            var expectedMessage = "Cannot parse 'multiple' value '" + multiple +
                "' for <select>. Acceptable values are 'false', 'true' and 'multiple'.";

            var tagHelperContext = new TagHelperContext(contextAttributes);
            var output = new TagHelperOutput(tagName, originalAttributes, content);

            var metadataProvider = new EmptyModelMetadataProvider();
            var htmlGenerator = new TestableHtmlGenerator(metadataProvider);

            string model = null;
            var metadata = metadataProvider.GetMetadataForType(() => model, typeof(string));
            var modelExpression = new ModelExpression("Property1", metadata);

            var viewContext = TestableHtmlGenerator.GetViewContext(model, htmlGenerator, metadataProvider);
            var tagHelper = new SelectTagHelper
            {
                For = modelExpression,
                Generator = htmlGenerator,
                Multiple = multiple,
                ViewContext = viewContext,
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => tagHelper.ProcessAsync(tagHelperContext, output));
            Assert.Equal(expectedMessage, exception.Message);
        }

        public class NameAndId
        {
            public NameAndId(string name, string id)
            {
                Name = name;
                Id = id;
            }

            public string Name { get; private set; }

            public string Id { get; private set; }
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
