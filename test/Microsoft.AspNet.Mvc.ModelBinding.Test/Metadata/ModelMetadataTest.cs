﻿using System;
using System.Collections.Generic;
using System.Linq;
#if NET45
using Moq;
#endif
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Test
{
    public class ModelMetadataTest
    {
#if NET45
        // Constructor

        [Fact]
        public void DefaultValues()
        {
            // Arrange
            Mock<IModelMetadataProvider> provider = new Mock<IModelMetadataProvider>();

            // Act
            ModelMetadata metadata = new ModelMetadata(provider.Object, typeof(Exception), () => "model", typeof(string), "propertyName");

            // Assert
            Assert.Equal(typeof(Exception), metadata.ContainerType);
            Assert.True(metadata.ConvertEmptyStringToNull);
            Assert.Null(metadata.NullDisplayText);
            Assert.Equal(metadata.SimpleDisplayText, "model");
            Assert.Null(metadata.Description);
            Assert.Equal("model", metadata.Model);
            Assert.Equal(typeof(string), metadata.ModelType);
            Assert.Equal("propertyName", metadata.PropertyName);
            Assert.False(metadata.IsReadOnly);
        }
#endif

        // IsComplexType

        struct IsComplexTypeModel
        {
        }

#if NET45
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(Nullable<int>))]
        [InlineData(typeof(int))]
        public void IsComplexTypeTestsReturnsFalseForSimpleTypes(Type type)
        {
            // Arrange
            Mock<IModelMetadataProvider> provider = new Mock<IModelMetadataProvider>();

            // Act
            var modelMetadata = new ModelMetadata(provider.Object, null, null, type, null);

            // Assert
            Assert.False(modelMetadata.IsComplexType);
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(IDisposable))]
        [InlineData(typeof(IsComplexTypeModel))]
        [InlineData(typeof(Nullable<IsComplexTypeModel>))]
        public void IsComplexTypeTestsReturnsTrueForComplexTypes(Type type)
        {
            // Arrange
            Mock<IModelMetadataProvider> provider = new Mock<IModelMetadataProvider>();

            // Act
            var modelMetadata = new ModelMetadata(provider.Object, null, null, type, null);

            // Assert
            Assert.True(modelMetadata.IsComplexType);
        }

        // IsNullableValueType

        [Fact]
        public void IsNullableValueTypeTests()
        {
            // Arrange
            Mock<IModelMetadataProvider> provider = new Mock<IModelMetadataProvider>();

            // Act & Assert
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(string), null).IsNullableValueType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(IDisposable), null).IsNullableValueType);
            Assert.True(new ModelMetadata(provider.Object, null, null, typeof(Nullable<int>), null).IsNullableValueType);
            Assert.False(new ModelMetadata(provider.Object, null, null, typeof(int), null).IsNullableValueType);
        }

        // Properties

        [Fact]
        public void PropertiesCallsProvider()
        {
            // Arrange
            Type modelType = typeof(string);
            List<ModelMetadata> propertyMetadata = new List<ModelMetadata>();
            Mock<IModelMetadataProvider> provider = new Mock<IModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, modelType, null);
            provider.Setup(p => p.GetMetadataForProperties(null, modelType))
                .Returns(propertyMetadata)
                .Verifiable();

            // Act
            IEnumerable<ModelMetadata> result = metadata.Properties;

            // Assert
            Assert.Equal(propertyMetadata, result.ToList());
            provider.Verify();
        }
#endif

        [Fact]
        public void PropertiesListGetsResetWhenModelGetsReset()
        {
            // Dev10 Bug #923263
            // Arrange
            IModelMetadataProvider provider = new EmptyModelMetadataProvider();
            var metadata = new ModelMetadata(provider, null, () => new Class1(), typeof(Class1), null);

            // Act
            ModelMetadata[] originalProps = metadata.Properties.ToArray();
            metadata.Model = new Class2();
            ModelMetadata[] newProps = metadata.Properties.ToArray();

            // Assert
            ModelMetadata originalProp = Assert.Single(originalProps);
            Assert.Equal(typeof(string), originalProp.ModelType);
            Assert.Equal("Prop1", originalProp.PropertyName);
            ModelMetadata newProp = Assert.Single(newProps);
            Assert.Equal(typeof(int), newProp.ModelType);
            Assert.Equal("Prop2", newProp.PropertyName);
        }

        class Class1
        {
            public string Prop1 { get; set; }
        }

        class Class2
        {
            public int Prop2 { get; set; }
        }

        // GetDisplayName()

#if NET45
        [Fact]
        public void ReturnsPropertyNameWhenSetAndDisplayNameIsNull()
        {
            // Arrange
            Mock<IModelMetadataProvider> provider = new Mock<IModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), "PropertyName");

            // Act
            string result = metadata.GetDisplayName();

            // Assert
            Assert.Equal("PropertyName", result);
        }

        [Fact]
        public void ReturnsTypeNameWhenPropertyNameAndDisplayNameAreNull()
        {
            // Arrange
            Mock<IModelMetadataProvider> provider = new Mock<IModelMetadataProvider>();
            ModelMetadata metadata = new ModelMetadata(provider.Object, null, null, typeof(object), null);

            // Act
            string result = metadata.GetDisplayName();

            // Assert
            Assert.Equal("Object", result);
        }
#endif

        // Helpers

        private class DummyContactModel
        {
            public int IntField = 0;
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public Nullable<int> NullableIntValue { get; set; }
            public int[] Array { get; set; }

            public string this[int index]
            {
                get { return "Indexed into " + index; }
            }
        }

        private class DummyModelContainer
        {
            public DummyContactModel Model { get; set; }
        }
    }
}
