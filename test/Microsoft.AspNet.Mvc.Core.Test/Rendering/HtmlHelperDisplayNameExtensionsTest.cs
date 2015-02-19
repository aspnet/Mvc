// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core
{
    /// <summary>
    /// Test the <see cref="HtmlHelperDisplayNameExtensions" /> class.
    /// </summary>
    public class HtmlHelperDisplayNameExtensionsTest
    {
        [Fact]
        public void DisplayNameHelpers_ReturnEmptyForModel()
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            var enumerableHelper =
                DefaultTemplatesUtilities.GetHtmlHelper<IEnumerable<DefaultTemplatesUtilities.ObjectTemplateModel>>(model: null);

            // Act
            var displayNameResult = helper.DisplayName(expression: string.Empty);
            var displayNameNullResult = helper.DisplayName(expression: null);   // null is another alias for current model
            var displayNameForResult = helper.DisplayNameFor(m => m);
            var displayNameForEnumerableModelResult = enumerableHelper.DisplayNameFor(m => m);
            var displayNameForModelResult = helper.DisplayNameForModel();

            // Assert
            Assert.Empty(displayNameResult);
            Assert.Empty(displayNameNullResult);
            Assert.Empty(displayNameForResult);
            Assert.Empty(displayNameForEnumerableModelResult);
            Assert.Empty(displayNameForModelResult);
        }

        [Fact]
        public void DisplayNameHelpers_ReturnPropertyName()
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            var enumerableHelper =
                DefaultTemplatesUtilities.GetHtmlHelper<IEnumerable<DefaultTemplatesUtilities.ObjectTemplateModel>>(model: null);

            // Act
            var displayNameResult = helper.DisplayName("Property1");
            var displayNameForResult = helper.DisplayNameFor(m => m.Property1);
            var displayNameForEnumerableResult = enumerableHelper.DisplayNameFor(m => m.Property1);

            // Assert
            Assert.Equal("Property1", displayNameResult);
            Assert.Equal("Property1", displayNameForResult);
            Assert.Equal("Property1", displayNameForEnumerableResult);
        }

        [Fact]
        public void DisplayNameHelpers_ReturnPropertyName_ForNestedProperty()
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper<OuterClass>(model: null);
            var enumerableHelper =
                DefaultTemplatesUtilities.GetHtmlHelper<IEnumerable<OuterClass>>(model: null);

            // Act
            var displayNameResult = helper.DisplayName("Inner.Id");
            var displayNameForResult = helper.DisplayNameFor(m => m.Inner.Id);
            var displayNameForEnumerableResult = enumerableHelper.DisplayNameFor(m => m.Inner.Id);

            // Assert
            Assert.Equal("Id", displayNameResult);
            Assert.Equal("Id", displayNameForResult);
            Assert.Equal("Id", displayNameForEnumerableResult);
        }

        [Fact]
        public void DisplayNameHelpers_ReturnMetadataPropertyName_AsFallback()
        {
            // Arrange
            var provider = new EmptyModelMetadataProvider();

            var modelExplorer = provider.GetModelExplorerForType(
                typeof(DefaultTemplatesUtilities.ObjectTemplateModel),
                model: null)
                .GetExplorerForProperty("Property1");

            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            helper.ViewData.ModelExplorer = modelExplorer;

            var enumerableHelper =
                DefaultTemplatesUtilities.GetHtmlHelper<IEnumerable<DefaultTemplatesUtilities.ObjectTemplateModel>>(model: null);
            enumerableHelper.ViewData.ModelExplorer = modelExplorer;

            // Act
            var displayNameResult = helper.DisplayName(expression: string.Empty);
            var displayNameForResult = helper.DisplayNameFor(m => m);
            var displayNameForEnumerableResult = enumerableHelper.DisplayNameFor(m => m);
            var displayNameForModelResult = helper.DisplayNameForModel();

            // Assert
            Assert.Equal("Property1", displayNameResult);
            Assert.Equal("Property1", displayNameForResult);
            Assert.Equal("Property1", displayNameForEnumerableResult);
            Assert.Equal("Property1", displayNameForModelResult);
        }

        [Fact]
        public void DisplayNameHelpers_ReturnMetadataPropertyNameForProperty_AsFallback()
        {
            // Arrange
            var provider = new EmptyModelMetadataProvider();

            var modelExplorer = provider.GetModelExplorerForType(
                typeof(DefaultTemplatesUtilities.ObjectTemplateModel),
                model: null);

            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            helper.ViewData.ModelExplorer = modelExplorer;

            var enumerableHelper =
                DefaultTemplatesUtilities.GetHtmlHelper<IEnumerable<DefaultTemplatesUtilities.ObjectTemplateModel>>(model: null);
            helper.ViewData.ModelExplorer = modelExplorer;

            // Act
            var displayNameForResult = helper.DisplayNameFor(m => m.Property1);
            var displayNameForEnumerableResult = enumerableHelper.DisplayNameFor(m => m.Property1);

            // Assert
            Assert.Equal("Property1", displayNameForResult);
            Assert.Equal("Property1", displayNameForEnumerableResult);
        }

        [Theory]
        [InlineData("")]    // Empty display name wins over non-empty property name.
        [InlineData("Custom display name from metadata")]
        public void DisplayNameHelpers_ReturnDisplayName_IfNonNull(string displayName)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            helper.ViewData.ModelMetadata.DisplayName = displayName;
            var enumerableHelper =
                DefaultTemplatesUtilities.GetHtmlHelper<IEnumerable<DefaultTemplatesUtilities.ObjectTemplateModel>>(model: null);
            enumerableHelper.ViewData.ModelMetadata.DisplayName = displayName;

            // Act
            var displayNameResult = helper.DisplayName(expression: string.Empty);
            var displayNameForResult = helper.DisplayNameFor(m => m);
            var displayNameForEnumerableResult = enumerableHelper.DisplayNameFor(m => m);
            var displayNameForModelResult = helper.DisplayNameForModel();

            // Assert
            Assert.Equal(displayName, displayNameResult);
            Assert.Equal(displayName, displayNameForResult);
            Assert.Equal(displayName, displayNameForEnumerableResult);
            Assert.Equal(displayName, displayNameForModelResult);
        }

        [Fact]
        public void DisplayNameHelpers_ReturnDisplayNameForProperty_IfNonNull()
        {
            // Arrange
            var displayName = "CoolDisplayName";

            var helper = DefaultTemplatesUtilities.GetHtmlHelper<DisplayNameOnProperty>(model: null);
            var enumerableHelper = DefaultTemplatesUtilities.GetHtmlHelper<DisplayNameOnProperty>(model: null);

            // Act
            var displayNameResult = helper.DisplayName("Property1");
            var displayNameForResult = helper.DisplayNameFor(m => m.Property1);
            var displayNameForEnumerableResult = enumerableHelper.DisplayNameFor(m => m.Property1);

            // Assert
            Assert.Equal(displayName, displayNameResult);
            Assert.Equal(displayName, displayNameForResult);
            Assert.Equal(displayName, displayNameForEnumerableResult);
        }

        [Theory]
        [InlineData("A", "A")]
        [InlineData("A[23]", "A[23]")]
        [InlineData("A[0].B", "B")]
        [InlineData("A.B.C.D", "D")]
        public void DisplayName_ReturnsRightmostExpressionSegment_IfPropertiesNotFound(
            string expression,
            string expectedResult)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();

            // Act
            var result = helper.DisplayName(expression);

            // Assert
            // DisplayName() falls back to expression name when DisplayName and PropertyName are null.
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void DisplayNameFor_ThrowsInvalidOperation_IfExpressionUnsupported()
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => helper.DisplayNameFor(model => new { foo = "Bar" }));
            Assert.Equal(
                "Templates can be used only with field access, property access, single-dimension array index, or single-parameter custom indexer expressions.",
                exception.Message);
        }

        [Fact]
        public void EnumerableDisplayNameFor_ThrowsInvalidOperation_IfExpressionUnsupported()
        {
            // Arrange
            var helper =
                DefaultTemplatesUtilities.GetHtmlHelper<IEnumerable<DefaultTemplatesUtilities.ObjectTemplateModel>>(model: null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => helper.DisplayNameFor(model => new { foo = "Bar" }));
            Assert.Equal(
                "Templates can be used only with field access, property access, single-dimension array index, or single-parameter custom indexer expressions.",
                exception.Message);
        }

        [Fact]
        public void DisplayNameFor_ReturnsVariableName()
        {
            // Arrange
            var unknownKey = "this is a dummy parameter value";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();

            // Act
            var result = helper.DisplayNameFor(model => unknownKey);

            // Assert
            Assert.Equal("unknownKey", result);
        }

        [Fact]
        public void EnumerableDisplayNameFor_ReturnsVariableName()
        {
            // Arrange
            var unknownKey = "this is a dummy parameter value";
            var helper =
                DefaultTemplatesUtilities.GetHtmlHelper<IEnumerable<DefaultTemplatesUtilities.ObjectTemplateModel>>(model: null);

            // Act
            var result = helper.DisplayNameFor(model => unknownKey);

            // Assert
            Assert.Equal("unknownKey", result);
        }

        private sealed class InnerClass
        {
            public int Id { get; set; }
        }

        private sealed class OuterClass
        {
            public InnerClass Inner { get; set; }
        }

        private class DisplayNameOnProperty
        {
            [Display(Name = "CoolDisplayName")]
            public string Property1 { get; set; }
        }
    }
}