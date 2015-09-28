// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Validation
{
    public class DataAnnotationsClientModelValidatorProviderTest
    {
        private readonly IModelMetadataProvider _metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();

        [Fact]
        public void GetValidators_AddsRequiredAttribute_ForIsRequiredTrue()
        {
            // Arrange
            var provider = new DataAnnotationsClientModelValidatorProvider(
                new TestOptionsManager<MvcDataAnnotationsLocalizationOptions>(),
                stringLocalizerFactory: null);

            var metadata = _metadataProvider.GetMetadataForProperty(
                typeof(DummyRequiredAttributeHelperClass),
                nameof(DummyRequiredAttributeHelperClass.ValueTypeWithoutAttribute));

            var providerContext = new ClientValidatorProviderContext(metadata);

            // Act
            provider.GetValidators(providerContext);

            // Assert
            var validator = Assert.Single(providerContext.Validators);
            Assert.IsType<RequiredAttributeAdapter>(validator);
        }

        [Fact]
        public void GetValidators_DoesNotAddRequiredAttribute_ForIsRequiredFalse()
        {
            // Arrange
            var provider = new DataAnnotationsClientModelValidatorProvider(
                new TestOptionsManager<MvcDataAnnotationsLocalizationOptions>(),
                stringLocalizerFactory: null);

            var metadata = _metadataProvider.GetMetadataForProperty(
                typeof(DummyRequiredAttributeHelperClass),
                nameof(DummyRequiredAttributeHelperClass.ReferenceTypeWithoutAttribute));

            var providerContext = new ClientValidatorProviderContext(metadata);

            // Act
            provider.GetValidators(providerContext);

            // Assert
            Assert.Empty(providerContext.Validators);
        }

        [Fact]
        public void GetValidators_DoesNotAddExtraRequiredAttribute_IfAttributeIsSpecifiedExplicitly()
        {
            // Arrange
            var provider = new DataAnnotationsClientModelValidatorProvider(
                new TestOptionsManager<MvcDataAnnotationsLocalizationOptions>(),
                stringLocalizerFactory: null);

            var metadata = _metadataProvider.GetMetadataForProperty(
                typeof(DummyRequiredAttributeHelperClass),
                nameof(DummyRequiredAttributeHelperClass.WithAttribute));

            var providerContext = new ClientValidatorProviderContext(metadata);

            // Act
            provider.GetValidators(providerContext);

            // Assert
            var validator = Assert.Single(providerContext.Validators);
            var adapter = Assert.IsType<RequiredAttributeAdapter>(validator);
            Assert.Equal("Custom Required Message", adapter.Attribute.ErrorMessage);
        }

        public static IEnumerable<object[]> DataAnnotationAdapters
        {
            get
            {
                yield return new object[]
                {
                    new RegularExpressionAttribute("abc"),
                    typeof(RegularExpressionAttributeAdapter)
                };

                yield return new object[]
                {
                    new MaxLengthAttribute(),
                    typeof(MaxLengthAttributeAdapter)
                };

                yield return new object[]
                {
                   new MinLengthAttribute(1),
                  typeof(MinLengthAttributeAdapter)
                };

                yield return new object[]
                {
                    new RangeAttribute(1, 100),
                    typeof(RangeAttributeAdapter)
                };

                yield return new object[]
                {
                    new StringLengthAttribute(6),
                    typeof(StringLengthAttributeAdapter)
                };

                yield return new object[]
                {
                    new RequiredAttribute(),
                    typeof(RequiredAttributeAdapter)
                };
            }
        }

        [Theory]
        [MemberData(nameof(DataAnnotationAdapters))]
        public void AdapterFactory_RegistersAdapters_ForDataAnnotationAttributes(
            ValidationAttribute attribute,
            Type expectedAdapterType)
        {
            // Arrange
            var adapters = new DataAnnotationsClientModelValidatorProvider(
                new TestOptionsManager<MvcDataAnnotationsLocalizationOptions>(),
                stringLocalizerFactory: null)
                .AttributeFactories;
            var adapterFactory = adapters.Single(kvp => kvp.Key == attribute.GetType()).Value;

            // Act
            var adapter = adapterFactory(attribute, stringLocalizer: null);

            // Assert
            Assert.IsType(expectedAdapterType, adapter);
        }

        public static IEnumerable<object[]> DataTypeAdapters
        {
            get
            {
                yield return new object[] { new UrlAttribute(), "url" };
                yield return new object[] { new CreditCardAttribute(), "creditcard" };
                yield return new object[] { new EmailAddressAttribute(), "email" };
                yield return new object[] { new PhoneAttribute(), "phone" };
            }
        }

        [Theory]
        [MemberData(nameof(DataTypeAdapters))]
        public void AdapterFactory_RegistersAdapters_ForDataTypeAttributes(
            ValidationAttribute attribute,
            string expectedRuleName)
        {
            // Arrange
            var adapters = new DataAnnotationsClientModelValidatorProvider(
                new TestOptionsManager<MvcDataAnnotationsLocalizationOptions>(),
                stringLocalizerFactory: null)
                .AttributeFactories;
            var adapterFactory = adapters.Single(kvp => kvp.Key == attribute.GetType()).Value;

            // Act
            var adapter = adapterFactory(attribute, stringLocalizer: null);

            // Assert
            var dataTypeAdapter = Assert.IsType<DataTypeAttributeAdapter>(adapter);
            Assert.Equal(expectedRuleName, dataTypeAdapter.RuleName);
        }

        [Fact]
        public void UnknownValidationAttribute_IsNotAddedAsValidator()
        {
            // Arrange
            var provider = new DataAnnotationsClientModelValidatorProvider(
                new TestOptionsManager<MvcDataAnnotationsLocalizationOptions>(),
                stringLocalizerFactory: null);
            var metadata = _metadataProvider.GetMetadataForType(typeof(DummyClassWithDummyValidationAttribute));

            var providerContext = new ClientValidatorProviderContext(metadata);

            // Act
            provider.GetValidators(providerContext);

            // Assert
            Assert.Empty(providerContext.Validators);
        }

        private class DummyValidationAttribute : ValidationAttribute
        {
        }

        [DummyValidation]
        private class DummyClassWithDummyValidationAttribute
        {
        }

        private class DummyRequiredAttributeHelperClass
        {
            [Required(ErrorMessage = "Custom Required Message")]
            public int WithAttribute { get; set; }

            public int ValueTypeWithoutAttribute { get; set; }

            public string ReferenceTypeWithoutAttribute { get; set; }
        }
    }
}
