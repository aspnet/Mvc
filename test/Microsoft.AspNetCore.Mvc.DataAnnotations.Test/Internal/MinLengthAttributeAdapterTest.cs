// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.DataAnnotations.Internal
{
    public class MinLengthAttributeAdapterTest
    {
        [Fact]
        [ReplaceCulture]
        public void MinLengthAttribute_AddValidation_Localize()
        {
            // Arrange
            var provider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), "Length");

            var attribute = new MinLengthAttribute(6);
            attribute.ErrorMessage = "Property must be at least '{1}' characters long.";

            var expectedProperties = new object[] { "Length", 6 };
            var expectedMessage = "Property must be at least '6' characters long.";

            var stringLocalizer = new Mock<IStringLocalizer>();
            stringLocalizer.Setup(s => s[attribute.ErrorMessage, expectedProperties])
                .Returns(new LocalizedString(attribute.ErrorMessage, expectedMessage));

            var adapter = new MinLengthAttributeAdapter(attribute, stringLocalizer: stringLocalizer.Object);

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(actionContext, metadata, provider, new AttributeDictionary());

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("true", kvp.Value); },
                kvp => { Assert.Equal("data-val-minlength", kvp.Key); Assert.Equal(expectedMessage, kvp.Value); },
                kvp => { Assert.Equal("data-val-minlength-min", kvp.Key); Assert.Equal("6", kvp.Value); });
        }

        [Fact]
        [ReplaceCulture]
        public void MinLengthAttribute_AddValidation_Attribute()
        {
            // Arrange
            var provider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), "Length");

            var attribute = new MinLengthAttribute(6);
            var adapter = new MinLengthAttributeAdapter(attribute, stringLocalizer: null);

            var expectedMessage = attribute.FormatErrorMessage("Length");

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(actionContext, metadata, provider, new AttributeDictionary());

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("true", kvp.Value); },
                kvp => { Assert.Equal("data-val-minlength", kvp.Key); Assert.Equal(expectedMessage, kvp.Value); },
                kvp => { Assert.Equal("data-val-minlength-min", kvp.Key); Assert.Equal("6", kvp.Value); });
        }

        [Fact]
        [ReplaceCulture]
        public void MinLengthAttribute_AddValidation_AttributeAndCustomMessage()
        {
            // Arrange
            var propertyName = "Length";
            var provider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = provider.GetMetadataForProperty(typeof(string), propertyName);

            var attribute = new MinLengthAttribute(2) { ErrorMessage = "Array must have at least {1} items." };
            var adapter = new MinLengthAttributeAdapter(attribute, stringLocalizer: null);

            var expectedMessage = "Array must have at least 2 items.";

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(actionContext, metadata, provider, new AttributeDictionary());

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("true", kvp.Value); },
                kvp => { Assert.Equal("data-val-minlength", kvp.Key); Assert.Equal(expectedMessage, kvp.Value); },
                kvp => { Assert.Equal("data-val-minlength-min", kvp.Key); Assert.Equal("2", kvp.Value); });
        }
    }
}