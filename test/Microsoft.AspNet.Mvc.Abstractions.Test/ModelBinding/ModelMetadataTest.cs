// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.AspNet.Mvc.ModelBinding.Metadata;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class ModelMetadataTest
    {
        // IsComplexType
        private struct IsComplexTypeModel
        {
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(Nullable<int>))]
        [InlineData(typeof(int))]
        public void IsComplexType_ReturnsFalseForSimpleTypes(Type type)
        {
            // Arrange
            var provider = new EmptyModelMetadataProvider();

            // Act
            var modelMetadata = new TestModelMetadata(type);

            // Assert
            Assert.False(modelMetadata.IsComplexType);
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(IDisposable))]
        [InlineData(typeof(IsComplexTypeModel))]
        [InlineData(typeof(Nullable<IsComplexTypeModel>))]
        public void IsComplexType_ReturnsTrueForComplexTypes(Type type)
        {
            // Arrange
            var provider = new EmptyModelMetadataProvider();

            // Act
            var modelMetadata = new TestModelMetadata(type);

            // Assert
            Assert.True(modelMetadata.IsComplexType);
        }

        // IsCollectionType / IsEnumerableType

        private class NonCollectionType
        {
        }

        private class DerivedList : List<int>
        {
        }

        private class JustEnumerable : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        public static TheoryData<Type> NonCollectionNonEnumerableData
        {
            get
            {
                return new TheoryData<Type>
                {
                    typeof(object),
                    typeof(int),
                    typeof(NonCollectionType),
                    typeof(string),
                };
            }
        }

        public static TheoryData<Type> CollectionAndEnumerableData
        {
            get
            {
                return new TheoryData<Type>
                {
                    typeof(int[]),
                    typeof(List<string>),
                    typeof(DerivedList),
                    typeof(Collection<int>),
                    typeof(Dictionary<object, object>),
                    typeof(CollectionImplementation),
                };
            }
        }

        [Theory]
        [MemberData(nameof(NonCollectionNonEnumerableData))]
        [InlineData(typeof(IEnumerable))]
        [InlineData(typeof(IEnumerable<string>))]
        [InlineData(typeof(JustEnumerable))]
        public void IsCollectionType_ReturnsFalseForNonCollectionTypes(Type type)
        {
            // Arrange
            var provider = new EmptyModelMetadataProvider();

            // Act
            var modelMetadata = new TestModelMetadata(type);

            // Assert
            Assert.False(modelMetadata.IsCollectionType);
        }

        [Theory]
        [MemberData(nameof(CollectionAndEnumerableData))]
        public void IsCollectionType_ReturnsTrueForCollectionTypes(Type type)
        {
            // Arrange
            var provider = new EmptyModelMetadataProvider();

            // Act
            var modelMetadata = new TestModelMetadata(type);

            // Assert
            Assert.True(modelMetadata.IsCollectionType);
        }

        [Theory]
        [MemberData(nameof(NonCollectionNonEnumerableData))]
        public void IsEnumerableType_ReturnsFalseForNonEnumerableTypes(Type type)
        {
            // Arrange
            var provider = new EmptyModelMetadataProvider();

            // Act
            var modelMetadata = new TestModelMetadata(type);

            // Assert
            Assert.False(modelMetadata.IsEnumerableType);
        }

        [Theory]
        [MemberData(nameof(CollectionAndEnumerableData))]
        [InlineData(typeof(IEnumerable))]
        [InlineData(typeof(IEnumerable<string>))]
        [InlineData(typeof(JustEnumerable))]
        public void IsEnumerableType_ReturnsTrueForEnumerableTypes(Type type)
        {
            // Arrange
            var provider = new EmptyModelMetadataProvider();

            // Act
            var modelMetadata = new TestModelMetadata(type);

            // Assert
            Assert.True(modelMetadata.IsEnumerableType);
        }

        // IsNullableValueType

        [Theory]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(IDisposable), false)]
        [InlineData(typeof(Nullable<int>), true)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(DerivedList), false)]
        [InlineData(typeof(IsComplexTypeModel), false)]
        [InlineData(typeof(Nullable<IsComplexTypeModel>), true)]
        public void IsNullableValueType_ReturnsExpectedValue(Type modelType, bool expected)
        {
            // Arrange
            var modelMetadata = new TestModelMetadata(modelType);

            // Act & Assert
            Assert.Equal(expected, modelMetadata.IsNullableValueType);
        }

        // IsReferenceOrNullableType

        [Theory]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(IDisposable), true)]
        [InlineData(typeof(Nullable<int>), true)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(DerivedList), true)]
        [InlineData(typeof(IsComplexTypeModel), false)]
        [InlineData(typeof(Nullable<IsComplexTypeModel>), true)]
        public void IsReferenceOrNullableType_ReturnsExpectedValue(Type modelType, bool expected)
        {
            // Arrange
            var modelMetadata = new TestModelMetadata(modelType);

            // Act & Assert
            Assert.Equal(expected, modelMetadata.IsReferenceOrNullableType);
        }

        // UnderlyingOrModelType

        [Theory]
        [InlineData(typeof(string), typeof(string))]
        [InlineData(typeof(IDisposable), typeof(IDisposable))]
        [InlineData(typeof(Nullable<int>), typeof(int))]
        [InlineData(typeof(int), typeof(int))]
        [InlineData(typeof(DerivedList), typeof(DerivedList))]
        [InlineData(typeof(IsComplexTypeModel), typeof(IsComplexTypeModel))]
        [InlineData(typeof(Nullable<IsComplexTypeModel>), typeof(IsComplexTypeModel))]
        public void UnderlyingOrModelType_ReturnsExpectedValue(Type modelType, Type expected)
        {
            // Arrange
            var modelMetadata = new TestModelMetadata(modelType);

            // Act & Assert
            Assert.Equal(expected, modelMetadata.UnderlyingOrModelType);
        }

        // GetDisplayName()

        [Fact]
        public void GetDisplayName_ReturnsDisplayName_IfSet()
        {
            // Arrange
            var provider = new EmptyModelMetadataProvider();
            var metadata = new TestModelMetadata(typeof(int), "Length", typeof(string));
            metadata.SetDisplayName("displayName");

            // Act
            var result = metadata.GetDisplayName();

            // Assert
            Assert.Equal("displayName", result);
        }

        [Fact]
        public void GetDisplayName_ReturnsPropertyName_WhenSetAndDisplayNameIsNull()
        {
            // Arrange
            var provider = new EmptyModelMetadataProvider();
            var metadata = new TestModelMetadata(typeof(int), "Length", typeof(string));

            // Act
            var result = metadata.GetDisplayName();

            // Assert
            Assert.Equal("Length", result);
        }

        [Fact]
        public void GetDisplayName_ReturnsTypeName_WhenPropertyNameAndDisplayNameAreNull()
        {
            // Arrange
            var provider = new EmptyModelMetadataProvider();
            var metadata = new TestModelMetadata(typeof(string));

            // Act
            var result = metadata.GetDisplayName();

            // Assert
            Assert.Equal("String", result);
        }

        private class TestModelMetadata : ModelMetadata
        {
            private string _displayName;

            public TestModelMetadata(Type modelType)
                : base(ModelMetadataIdentity.ForType(modelType))
            {
            }

            public TestModelMetadata(Type modelType, string propertyName, Type containerType)
                : base(ModelMetadataIdentity.ForProperty(modelType, propertyName, containerType))
            {
            }

            public override IReadOnlyDictionary<object, object> AdditionalValues
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override string BinderModelName
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override Type BinderType
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override BindingSource BindingSource
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override bool ConvertEmptyStringToNull
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override string DataTypeName
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override string Description
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override string DisplayFormatString
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override string DisplayName
            {
                get
                {
                    return _displayName;
                }
            }

            public void SetDisplayName(string displayName)
            {
                _displayName = displayName;
            }

            public override string EditFormatString
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override ModelMetadata ElementMetadata
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override IEnumerable<KeyValuePair<string, string>> EnumDisplayNamesAndValues
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override IReadOnlyDictionary<string, string> EnumNamesAndValues
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override bool HasNonDefaultEditFormat
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override bool HideSurroundingHtml
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override bool HtmlEncode
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override bool IsBindingAllowed
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override bool IsBindingRequired
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override bool IsEnum
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override bool IsFlagsEnum
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override bool IsRequired
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override IModelBindingMessages ModelBindingMessages
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override string NullDisplayText
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override int Order
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override ModelPropertyCollection Properties
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override IPropertyBindingPredicateProvider PropertyBindingPredicateProvider
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override bool ShowForDisplay
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override bool ShowForEdit
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override string SimpleDisplayProperty
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override string TemplateHint
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override IReadOnlyList<object> ValidatorMetadata
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override Func<object, object> PropertyGetter
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override Action<object, object> PropertySetter
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        private class CollectionImplementation : ICollection<string>
        {
            public int Count
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public void Add(string item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(string item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(string[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<string> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public bool Remove(string item)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}
