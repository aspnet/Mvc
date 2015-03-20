﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
#if DNX451
using Moq;
#endif
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Validation
{
    public class DataAnnotationsModelValidatorProviderTest
    {
        private readonly IModelMetadataProvider _metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();

#if DNX451
        [Fact]
        public void GetValidators_ReturnsValidatorForIValidatableObject()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var mockValidatable = Mock.Of<IValidatableObject>();
            var metadata = _metadataProvider.GetMetadataForType(mockValidatable.GetType());

            var providerContext = new ModelValidatorProviderContext(metadata);

            // Act
            provider.GetValidators(providerContext);

            // Assert
            var validator = Assert.Single(providerContext.Validators);
            Assert.IsType<ValidatableObjectAdapter>(validator);
        }
#endif

        [Fact]
        public void GetValidators_DoesNotAddRequiredAttribute_ForNonNullableValueTypes_IfAttributeIsSpecifiedExplicitly()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var metadata = _metadataProvider.GetMetadataForProperty(typeof(DummyRequiredAttributeHelperClass),
                                                                    "WithAttribute");

            var providerContext = new ModelValidatorProviderContext(metadata);

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
        public void AdapterFactory_RegistersAdapters_ForDataAnnotationAttributes(ValidationAttribute attribute,
                                                                                 Type expectedAdapterType)
        {
            // Arrange
            var adapters = new DataAnnotationsModelValidatorProvider().AttributeFactories;
            var adapterFactory = adapters.Single(kvp => kvp.Key == attribute.GetType()).Value;

            // Act
            var adapter = adapterFactory(attribute);

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
        public void AdapterFactory_RegistersAdapters_ForDataTypeAttributes(ValidationAttribute attribute,
                                                                           string expectedRuleName)
        {
            // Arrange
            var adapters = new DataAnnotationsModelValidatorProvider().AttributeFactories;
            var adapterFactory = adapters.Single(kvp => kvp.Key == attribute.GetType()).Value;

            // Act
            var adapter = adapterFactory(attribute);

            // Assert
            var dataTypeAdapter = Assert.IsType<DataTypeAttributeAdapter>(adapter);
            Assert.Equal(expectedRuleName, dataTypeAdapter.RuleName);
        }

        [Fact]
        public void UnknownValidationAttributeGetsDefaultAdapter()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var metadata = _metadataProvider.GetMetadataForType(typeof(DummyClassWithDummyValidationAttribute));

            var providerContext = new ModelValidatorProviderContext(metadata);

            // Act
            provider.GetValidators(providerContext);

            // Assert
            var validator = providerContext.Validators.Single();
            Assert.IsType<DataAnnotationsModelValidator>(validator);
        }

        private class DummyValidationAttribute : ValidationAttribute
        {
        }

        [DummyValidation]
        private class DummyClassWithDummyValidationAttribute
        {
        }

        // Default IValidatableObject adapter factory

#if DNX451
        [Fact]
        public void IValidatableObjectGetsAValidator()
        {
            // Arrange
            var provider = new DataAnnotationsModelValidatorProvider();
            var mockValidatable = new Mock<IValidatableObject>();
            var metadata = _metadataProvider.GetMetadataForType(mockValidatable.Object.GetType());

            var providerContext = new ModelValidatorProviderContext(metadata);

            // Act
            provider.GetValidators(providerContext);

            // Assert
            Assert.Single(providerContext.Validators);
        }
#endif

        private class ObservableModel
        {
            private bool _propertyWasRead;

            public string TheProperty
            {
                get
                {
                    _propertyWasRead = true;
                    return "Hello";
                }
            }

            public bool PropertyWasRead()
            {
                return _propertyWasRead;
            }
        }

        private class BaseModel
        {
            public virtual string MyProperty { get; set; }
        }

        private class DerivedModel : BaseModel
        {
            [StringLength(10)]
            public override string MyProperty
            {
                get { return base.MyProperty; }
                set { base.MyProperty = value; }
            }
        }

        private class DummyRequiredAttributeHelperClass
        {
            [Required(ErrorMessage = "Custom Required Message")]
            public int WithAttribute { get; set; }

            public int WithoutAttribute { get; set; }
        }
    }
}
