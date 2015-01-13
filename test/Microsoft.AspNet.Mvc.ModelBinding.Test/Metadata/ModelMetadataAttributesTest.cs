// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelMetadataAttributesTest
    {
        [Fact]
        public void GetAttributesForBaseProperty_IncludeMetadataAttributes()
        {
            // Arrange
            var modelType = typeof(BaseViewModel);
            var property = modelType.GetRuntimeProperties().Where(p => p.Name == "BaseProperty").FirstOrDefault();

            // Act
            var attributes = ModelAttributes.GetAttributesForProperty(modelType, property);

            // Assert
            Assert.Single(attributes.OfType<RequiredAttribute>());
            Assert.Single(attributes.OfType<StringLengthAttribute>());
        }

        [Fact]
        public void GetAttributesForTestProperty_ModelOverridesMetadataAttributes()
        {
            // Arrange
            var modelType = typeof(BaseViewModel);
            var property = modelType.GetRuntimeProperties().Where(p => p.Name == "TestProperty").FirstOrDefault();

            // Act
            var attributes = ModelAttributes.GetAttributesForProperty(modelType, property);
            var rangeAttribute = attributes.OfType<RangeAttribute>().FirstOrDefault();

            // Assert
            Assert.NotNull(rangeAttribute);
            Assert.Equal(0, (int)rangeAttribute.Minimum);
            Assert.Equal(10, (int)rangeAttribute.Maximum);
        }

        [Fact]
        public void GetAttributesForBasePropertyFromDerivedModel_IncludeMetadataAttributes()
        {
            // Arrange
            var modelType = typeof(DerivedViewModel);
            var property = modelType.GetRuntimeProperties().Where(p => p.Name == "BaseProperty").FirstOrDefault();

            // Act
            var attributes = ModelAttributes.GetAttributesForProperty(modelType, property);

            // Assert
            Assert.Single(attributes.OfType<RequiredAttribute>());
            Assert.Single(attributes.OfType<StringLengthAttribute>());
        }

        [Fact]
        public void GetAttributesForTestPropertyFromDerived_IncludeMetadataAttributes()
        {
            // Arrange
            var modelType = typeof(DerivedViewModel);
            var property = modelType.GetRuntimeProperties().Where(p => p.Name == "TestProperty").FirstOrDefault();

            // Act
            var attributes = ModelAttributes.GetAttributesForProperty(modelType, property);

            // Assert
            Assert.Single(attributes.OfType<RequiredAttribute>());
            Assert.Single(attributes.OfType<StringLengthAttribute>());
            Assert.DoesNotContain(typeof(RangeAttribute), attributes);
        }

        [Fact]
        public void GetAttributesForVirtualPropertyFromDerived_IncludeMetadataAttributes()
        {
            // Arrange
            var modelType = typeof(DerivedViewModel);
            var property = modelType.GetRuntimeProperties().Where(p => p.Name == "VirtualProperty").FirstOrDefault();

            // Act
            var attributes = ModelAttributes.GetAttributesForProperty(modelType, property);

            // Assert
            Assert.Single(attributes.OfType<RequiredAttribute>());
            Assert.Single(attributes.OfType<RangeAttribute>());
        }

        [Fact]
        public void GetAttributesForType_IncludeMetadataAttributes()
        {
            // Arrange & Act
            var attributes = ModelAttributes.GetAttributesForType(typeof(BaseViewModel));

            // Assert
            Assert.Single(attributes.OfType<ClassValidator>());
        }

        // Helper classes

        [ClassValidator]
        private class BaseModel
        {
            [StringLength(10)]
            public string BaseProperty { get; set; }

            [Range(10,100)]
            public string TestProperty { get; set; }

            [Required]
            public virtual int VirtualProperty { get; set; }
        }

        private class DerivedModel : BaseModel
        {
            [Required]
            public string DerivedProperty { get; set; }

            [Required]
            public new string TestProperty { get; set; }

            [Range(10,100)]
            public override int VirtualProperty { get; set; }
            
        }

        [ModelMetadataType(typeof(BaseModel))]
        private class BaseViewModel
        {
            [Range(0,10)]
            public string TestProperty { get; set; }

            [Required]
            public string BaseProperty { get; set; }
        }

        [ModelMetadataType(typeof(DerivedModel))]
        private class DerivedViewModel : BaseViewModel
        {
            [StringLength(2)]
            public new string TestProperty { get; set; }

            public int VirtualProperty { get; set; }

        }

        private class ClassValidator : ValidationAttribute
        {
            public override Boolean IsValid(Object value)
            {
                return true;
            }
        }
    }
}