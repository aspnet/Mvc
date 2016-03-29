﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class CollectionModelBinderProviderTest
    {
        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        [InlineData(typeof(Person))]
        [InlineData(typeof(int[]))]
        public void Create_ForNonSupportedTypes_ReturnsNull(Type modelType)
        {
            // Arrange
            var provider = new CollectionModelBinderProvider();

            var context = new TestModelBinderProviderContext(modelType);

            // Act
            var result = provider.Create(context);

            // Assert
            Assert.Null(result);
        }

        [Theory]

        // These aren't ICollection<> - we can handle them by creating a List<>
        [InlineData(typeof(IEnumerable<int>))]
        [InlineData(typeof(IReadOnlyCollection<>))]
        [InlineData(typeof(IReadOnlyList<int>))]

        // These are ICollection<> - we can handle them by adding items to the existing collection or
        // creating a new one.
        [InlineData(typeof(ICollection<int>))]
        [InlineData(typeof(IList<int>))]
        [InlineData(typeof(List<int>))]
        [InlineData(typeof(Collection<int>))]
        public void Create_ForSupportedTypes_ReturnsBinder(Type modelType)
        {
            // Arrange
            var provider = new CollectionModelBinderProvider();

            var context = new TestModelBinderProviderContext(modelType);

            Type elementType = null;
            context.OnCreatingBinder(m =>
            {
                if (m.ModelType == typeof(int))
                {
                    elementType = m.ModelType;
                    return Mock.Of<IModelBinder>();
                }
                else
                {
                    Assert.False(false, "Not the right model type");
                    return null;
                }
            });

            // Act
            var result = provider.Create(context);

            // Assert
            Assert.NotNull(elementType);
            Assert.IsType<CollectionModelBinder<int>>(result);
        }

        // These aren't ICollection<> - we can handle them by creating a List<> - but in this case
        // we can't set the property so we can't bind.
        [Theory]
        [InlineData(nameof(ReadonlyProperties.Enumerable))]
        [InlineData(typeof(IReadOnlyCollection<>))]
        [InlineData(typeof(IReadOnlyList<int>))]
        public void Create_ForNonICollectionTypes_ReadOnlyProperty_ReturnsNull(string propertyName)
        {
            // Arrange
            var provider = new CollectionModelBinderProvider();

            var metadataProvider = TestModelBinderProviderContext.CachedMetadataProvider;

            var metadata = metadataProvider.GetMetadataForProperty(typeof(ReadonlyProperties), propertyName);
            Assert.NotNull(metadata);
            Assert.True(metadata.IsReadOnly);

            var context = new TestModelBinderProviderContext(metadata, bindingInfo: null);

            // Act
            var result = provider.Create(context);

            // Assert
            Assert.Null(result);
        }

        private class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        private class ReadonlyProperties
        {
            public IEnumerable<int> Enumerable { get; }

            public IReadOnlyCollection<int> ReadOnlyCollection { get; }

            public IReadOnlyList<int> ReadOnlyList { get; }
        }
    }
}
