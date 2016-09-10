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
    public class CompareAttributeAdapterTest
    {
        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesDisplayName_WithoutLocalizer()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = metadataProvider.GetMetadataForProperty(typeof(PropertyDisplayNameModel), "MyProperty");

            var attribute = new CompareAttribute("OtherProperty");
            var adapter = new CompareAttributeAdapter(attribute, stringLocalizer: null);

            var expectedMessage = "'MyPropertyDisplayName' and 'OtherPropertyDisplayName' do not match.";

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(
                actionContext,
                metadata,
                metadataProvider,
                new AttributeDictionary());

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("true", kvp.Value); },
                kvp => { Assert.Equal("data-val-equalto", kvp.Key); Assert.Equal(expectedMessage, kvp.Value); },
                kvp =>
                {
                    Assert.Equal("data-val-equalto-other", kvp.Key);
                    Assert.Equal(kvp.Value, "*.OtherProperty");
                });
        }

        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesDisplayName()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = metadataProvider.GetMetadataForProperty(typeof(PropertyDisplayNameModel), "MyProperty");

            var attribute = new CompareAttribute("OtherProperty");
            attribute.ErrorMessage = "CompareAttributeErrorMessage";

            var stringLocalizer = new Mock<IStringLocalizer>();
            var expectedProperties = new object[] { "MyPropertyDisplayName", "OtherPropertyDisplayName" };

            var expectedMessage = "'MyPropertyDisplayName' and 'OtherPropertyDisplayName' do not match.";

            stringLocalizer.Setup(s => s[attribute.ErrorMessage, expectedProperties])
                .Returns(new LocalizedString(attribute.ErrorMessage, expectedMessage));

            var adapter = new CompareAttributeAdapter(attribute, stringLocalizer: stringLocalizer.Object);

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(
                actionContext,
                metadata,
                metadataProvider,
                new AttributeDictionary());

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("true", kvp.Value); },
                kvp => { Assert.Equal("data-val-equalto", kvp.Key); Assert.Equal(expectedMessage, kvp.Value); },
                kvp =>
                {
                    Assert.Equal("data-val-equalto-other", kvp.Key);
                    Assert.Equal(kvp.Value, "*.OtherProperty");
                });
        }

        [Fact]
        [ReplaceCulture]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesPropertyName()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = metadataProvider.GetMetadataForProperty(typeof(PropertyNameModel), "MyProperty");

            var attribute = new CompareAttribute("OtherProperty");
            var adapter = new CompareAttributeAdapter(attribute, stringLocalizer: null);

            var expectedMessage = "'MyProperty' and 'OtherProperty' do not match.";

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(
                actionContext,
                metadata,
                metadataProvider,
                new AttributeDictionary());

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("true", kvp.Value); },
                kvp => { Assert.Equal("data-val-equalto", kvp.Key); Assert.Equal(expectedMessage, kvp.Value); },
                kvp =>
                {
                    Assert.Equal("data-val-equalto-other", kvp.Key);
                    Assert.Equal(kvp.Value, "*.OtherProperty");
                });
        }

        [Fact]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesOverride()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = metadataProvider.GetMetadataForProperty(typeof(PropertyNameModel), "MyProperty");

            var attribute = new CompareAttribute("OtherProperty")
            {
                ErrorMessage = "Hello '{0}', goodbye '{1}'."
            };
            var adapter = new CompareAttributeAdapter(attribute, stringLocalizer: null);

            var expectedMessage = "Hello 'MyProperty', goodbye 'OtherProperty'.";

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(
                actionContext,
                metadata,
                metadataProvider,
                new AttributeDictionary());

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("true", kvp.Value); },
                kvp => { Assert.Equal("data-val-equalto", kvp.Key); Assert.Equal(expectedMessage, kvp.Value); },
                kvp =>
                {
                    Assert.Equal("data-val-equalto-other", kvp.Key);
                    Assert.Equal(kvp.Value, "*.OtherProperty");
                });
        }

        [Fact]
        public void ClientRulesWithCompareAttribute_ErrorMessageUsesResourceOverride()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = metadataProvider.GetMetadataForProperty(typeof(PropertyNameModel), "MyProperty");

            var attribute = new CompareAttribute("OtherProperty")
            {
                ErrorMessageResourceName = "CompareAttributeTestResource",
                ErrorMessageResourceType = typeof(DataAnnotations.Test.Resources),
            };
            var adapter = new CompareAttributeAdapter(attribute, stringLocalizer: null);

            var expectedMessage = "Comparing MyProperty to OtherProperty.";

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(
                actionContext,
                metadata,
                metadataProvider,
                new AttributeDictionary());

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("true", kvp.Value); },
                kvp => { Assert.Equal("data-val-equalto", kvp.Key); Assert.Equal(expectedMessage, kvp.Value); },
                kvp =>
                {
                    Assert.Equal("data-val-equalto-other", kvp.Key);
                    Assert.Equal(kvp.Value, "*.OtherProperty");
                });
        }

        [Fact]
        [ReplaceCulture]
        public void AddValidation_DoesNotTrounceExistingAttributes()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = metadataProvider.GetMetadataForProperty(typeof(PropertyNameModel), "MyProperty");

            var attribute = new CompareAttribute("OtherProperty");
            var adapter = new CompareAttributeAdapter(attribute, stringLocalizer: null);

            var actionContext = new ActionContext();
            var context = new ClientModelValidationContext(
                actionContext,
                metadata,
                metadataProvider,
                new AttributeDictionary());

            context.Attributes.Add("data-val", "original");
            context.Attributes.Add("data-val-equalto", "original");
            context.Attributes.Add("data-val-equalto-other", "original");

            // Act
            adapter.AddValidation(context);

            // Assert
            Assert.Collection(
                context.Attributes,
                kvp => { Assert.Equal("data-val", kvp.Key); Assert.Equal("original", kvp.Value); },
                kvp => { Assert.Equal("data-val-equalto", kvp.Key); Assert.Equal("original", kvp.Value); },
                kvp => { Assert.Equal("data-val-equalto-other", kvp.Key); Assert.Equal("original", kvp.Value); });
        }

        private class PropertyDisplayNameModel
        {
            [Display(Name = "MyPropertyDisplayName")]
            public string MyProperty { get; set; }

            [Display(Name = "OtherPropertyDisplayName")]
            public string OtherProperty { get; set; }
        }

        private class PropertyNameModel
        {
            public string MyProperty { get; set; }

            public string OtherProperty { get; set; }
        }
    }
}