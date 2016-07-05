// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Microsoft.AspNetCore.Testing;

namespace Microsoft.AspNetCore.Mvc.Formatters.Xml.Internal
{
    public class EnumerableWrapperProviderTest
    {
        [Theory]
        [InlineData(typeof(IEnumerable<SerializableError>),
            typeof(DelegatingEnumerable<SerializableErrorWrapper, SerializableError>))]
        [InlineData(typeof(IQueryable<SerializableError>),
            typeof(DelegatingEnumerable<SerializableErrorWrapper, SerializableError>))]
        [InlineData(typeof(ICollection<SerializableError>),
            typeof(DelegatingEnumerable<SerializableErrorWrapper, SerializableError>))]
        [InlineData(typeof(IList<SerializableError>),
            typeof(DelegatingEnumerable<SerializableErrorWrapper, SerializableError>))]
        public void Gets_DelegatingWrappingType(Type declaredEnumerableOfT, Type expectedType)
        {
            // Arrange
            var wrapperProvider = new EnumerableWrapperProvider(
                                                            declaredEnumerableOfT,
                                                            new SerializableErrorWrapperProvider());

            // Act
            var wrappingType = wrapperProvider.WrappingType;

            // Assert
            Assert.NotNull(wrappingType);
            Assert.Equal(expectedType, wrappingType);
        }

        [Fact]
        public void Wraps_EmptyCollections()
        {
            // Arrange
            var declaredEnumerableOfT = typeof(IEnumerable<int>);
            var wrapperProvider = new EnumerableWrapperProvider(
                                                declaredEnumerableOfT,
                                                elementWrapperProvider: null);

            // Act
            var wrapped = wrapperProvider.Wrap(new int[] { });

            // Assert
            Assert.Equal(typeof(DelegatingEnumerable<int, int>), wrapperProvider.WrappingType);
            Assert.NotNull(wrapped);
            var delegatingEnumerable = wrapped as DelegatingEnumerable<int, int>;
            Assert.NotNull(delegatingEnumerable);
            Assert.Equal(0, delegatingEnumerable.Count());
        }

        [Fact]
        public void Ignores_NullInstances()
        {
            // Arrange
            var declaredEnumerableOfT = typeof(IEnumerable<int>);
            var wrapperProvider = new EnumerableWrapperProvider(
                                        declaredEnumerableOfT,
                                        elementWrapperProvider: null);

            // Act
            var wrapped = wrapperProvider.Wrap(null);

            // Assert
            Assert.Equal(typeof(DelegatingEnumerable<int, int>), wrapperProvider.WrappingType);
            Assert.Null(wrapped);
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(List<int>))]
        [InlineData(typeof(List<Person>))]
        [InlineData(typeof(List<SerializableError>))]
        [InlineData(typeof(PersonList))]
        public void ThrowsArugmentExceptionFor_ConcreteEnumerableOfT(Type declaredType)
        {
            // Act and Assert
            var ex = Assert.Throws<ArgumentException>(() => new EnumerableWrapperProvider(
                                                                            declaredType,
                                                                            elementWrapperProvider: null));

            Assert.Equal("sourceEnumerableOfT", ex.ParamName);
        }
    }
}