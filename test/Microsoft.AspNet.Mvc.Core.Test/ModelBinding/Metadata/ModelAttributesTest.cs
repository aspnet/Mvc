// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelAttributesTest
    {
        [Fact]
        public void GetAttributesForBaseProperty_IncludesMetadataAttributes()
        {
            // Arrange
            var modelType = typeof(BaseViewModel);
            var property = modelType.GetRuntimeProperties().FirstOrDefault(p => p.Name == "BaseProperty");

            // Act
            var attributes = ModelAttributes.GetAttributesForProperty(modelType, property);

            // Assert
            Assert.Single(attributes.Attributes.OfType<RequiredAttribute>());
            Assert.Single(attributes.Attributes.OfType<StringLengthAttribute>());

            Assert.Single(attributes.PropertyAttributes.OfType<RequiredAttribute>());
            Assert.Single(attributes.PropertyAttributes.OfType<StringLengthAttribute>());
        }

        [Fact]
        public void GetAttributesForTestProperty_ModelOverridesMetadataAttributes()
        {
            // Arrange
            var modelType = typeof(BaseViewModel);
            var property = modelType.GetRuntimeProperties().FirstOrDefault(p => p.Name == "TestProperty");

            // Act
            var attributes = ModelAttributes.GetAttributesForProperty(modelType, property);

            // Assert
            var rangeAttributes = attributes.Attributes.OfType<RangeAttribute>().ToArray();
            Assert.NotNull(rangeAttributes[0]);
            Assert.Equal(0, (int)rangeAttributes[0].Minimum);
            Assert.Equal(10, (int)rangeAttributes[0].Maximum);
            Assert.NotNull(rangeAttributes[1]);
            Assert.Equal(10, (int)rangeAttributes[1].Minimum);
            Assert.Equal(100, (int)rangeAttributes[1].Maximum);
            Assert.Single(attributes.Attributes.OfType<FromHeaderAttribute>());

            rangeAttributes = attributes.PropertyAttributes.OfType<RangeAttribute>().ToArray();
            Assert.NotNull(rangeAttributes[0]);
            Assert.Equal(0, (int)rangeAttributes[0].Minimum);
            Assert.Equal(10, (int)rangeAttributes[0].Maximum);
            Assert.NotNull(rangeAttributes[1]);
            Assert.Equal(10, (int)rangeAttributes[1].Minimum);
            Assert.Equal(100, (int)rangeAttributes[1].Maximum);
            Assert.Single(attributes.PropertyAttributes.OfType<FromHeaderAttribute>());
        }

        [Fact]
        public void GetAttributesForBasePropertyFromDerivedModel_IncludesMetadataAttributes()
        {
            // Arrange
            var modelType = typeof(DerivedViewModel);
            var property = modelType.GetRuntimeProperties().FirstOrDefault(p => p.Name == "BaseProperty");

            // Act
            var attributes = ModelAttributes.GetAttributesForProperty(modelType, property);

            // Assert
            Assert.Single(attributes.Attributes.OfType<RequiredAttribute>());
            Assert.Single(attributes.Attributes.OfType<StringLengthAttribute>());

            Assert.Single(attributes.PropertyAttributes.OfType<RequiredAttribute>());
            Assert.Single(attributes.PropertyAttributes.OfType<StringLengthAttribute>());
        }

        [Fact]
        public void GetAttributesForTestPropertyFromDerived_IncludesMetadataAttributes()
        {
            // Arrange
            var modelType = typeof(DerivedViewModel);
            var property = modelType.GetRuntimeProperties().FirstOrDefault(p => p.Name == "TestProperty");

            // Act
            var attributes = ModelAttributes.GetAttributesForProperty(modelType, property);

            // Assert
            Assert.Single(attributes.Attributes.OfType<RequiredAttribute>());
            Assert.Single(attributes.Attributes.OfType<StringLengthAttribute>());
            Assert.DoesNotContain(typeof(RangeAttribute), attributes.Attributes);

            Assert.Single(attributes.PropertyAttributes.OfType<RequiredAttribute>());
            Assert.Single(attributes.PropertyAttributes.OfType<StringLengthAttribute>());
            Assert.DoesNotContain(typeof(RangeAttribute), attributes.PropertyAttributes);
        }

        [Fact]
        public void GetAttributesForVirtualPropertyFromDerived_IncludesMetadataAttributes()
        {
            // Arrange
            var modelType = typeof(DerivedViewModel);
            var property = modelType.GetRuntimeProperties().FirstOrDefault(p => p.Name == "VirtualProperty");

            // Act
            var attributes = ModelAttributes.GetAttributesForProperty(modelType, property);

            // Assert
            Assert.Single(attributes.Attributes.OfType<RequiredAttribute>());
            Assert.Single(attributes.Attributes.OfType<RangeAttribute>());

            Assert.Single(attributes.PropertyAttributes.OfType<RequiredAttribute>());
            Assert.Single(attributes.PropertyAttributes.OfType<RangeAttribute>());
        }

        [Fact]
        public void GetFromServiceAttributeFromBase_IncludesMetadataAttributes()
        {
            // Arrange
            var modelType = typeof(DerivedViewModel);
            var property = modelType.GetRuntimeProperties().FirstOrDefault(p => p.Name == "Calculator");

            // Act
            var attributes = ModelAttributes.GetAttributesForProperty(modelType, property);

            // Assert
            Assert.Single(attributes.Attributes.OfType<RequiredAttribute>());
            Assert.Single(attributes.Attributes.OfType<FromServicesAttribute>());

            Assert.Single(attributes.PropertyAttributes.OfType<RequiredAttribute>());
            Assert.Single(attributes.PropertyAttributes.OfType<FromServicesAttribute>());
        }

        [Fact]
        public void GetAttributesForType_IncludesMetadataAttributes()
        {
            // Arrange & Act
            var attributes = ModelAttributes.GetAttributesForType(typeof(BaseViewModel));

            // Assert
            Assert.Single(attributes.Attributes.OfType<ClassValidator>());

            Assert.Single(attributes.TypeAttributes.OfType<ClassValidator>());
        }

        [Fact]
        public void GetAttributesForType_PropertyAttributes_IsNull()
        {
            // Arrange & Act
            var attributes = ModelAttributes.GetAttributesForType(typeof(BaseViewModel));

            // Assert
            Assert.Null(attributes.PropertyAttributes);
        }

        [Fact]
        public void GetAttributesForProperty_MergedAttributes()
        {
            // Arrange
            var property = typeof(MergedAttributes).GetRuntimeProperty(nameof(MergedAttributes.Property));

            // Act
            var attributes = ModelAttributes.GetAttributesForProperty(typeof(MergedAttributes), property);

            // Assert
            Assert.Equal(3, attributes.Attributes.Count);
            Assert.IsType<RequiredAttribute>(attributes.Attributes[0]);
            Assert.IsType<RangeAttribute>(attributes.Attributes[1]);
            Assert.IsType<ClassValidator>(attributes.Attributes[2]);

            Assert.Equal(2, attributes.PropertyAttributes.Count);
            Assert.IsType<RequiredAttribute>(attributes.PropertyAttributes[0]);
            Assert.IsType<RangeAttribute>(attributes.PropertyAttributes[1]);

            var attribute = Assert.Single(attributes.TypeAttributes);
            Assert.IsType<ClassValidator>(attribute);
        }

        [ClassValidator]
        private class BaseModel
        {
            [StringLength(10)]
            public string BaseProperty { get; set; }

            [Range(10,100)]
            [FromHeader]
            public string TestProperty { get; set; }

            [Required]
            public virtual int VirtualProperty { get; set; }

            [FromServices]
            public ICalculator Calculator { get; set; }
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

            [Required]
            public ICalculator Calculator { get; set; }
        }

        [ModelMetadataType(typeof(DerivedModel))]
        private class DerivedViewModel : BaseViewModel
        {
            [StringLength(2)]
            public new string TestProperty { get; set; }

            public int VirtualProperty { get; set; }

        }

        public interface ICalculator
        {
            int Operation(char @operator, int left, int right);
        }

        private class ClassValidator : ValidationAttribute
        {
            public override Boolean IsValid(Object value)
            {
                return true;
            }
        }

        [ModelMetadataType(typeof(MergedAttributesMetadata))]
        private class MergedAttributes
        {
            [Required]
            public PropertyType Property { get; set; }
        }

        private class MergedAttributesMetadata
        {
            [Range(0, 10)]
            public MetadataPropertyType Property { get; set; }
        }

        [ClassValidator]
        private class PropertyType
        {
        }

        [Bind]
        private class MetadataPropertyType
        {
        }
    }
}