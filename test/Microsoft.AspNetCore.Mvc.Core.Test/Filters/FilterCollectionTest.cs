// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    public class FilterCollectionTest
    {
        [Fact]
        public void Add_UsesTypeFilterAttribute()
        {
            // Arrange
            var collection = new FilterCollection();

            // Act
            var added = collection.Add(typeof(MyFilter));

            // Assert
            var typeFilter = Assert.IsType<TypeFilterAttribute>(added);
            Assert.Equal(typeof(MyFilter), typeFilter.ImplementationType);
            Assert.Same(typeFilter, Assert.Single(collection));
        }

        [Fact]
        public void GenericAdd_UsesTypeFilterAttribute()
        {
            // Arrange
            var collection = new FilterCollection();

            // Act
            var added = collection.Add<MyFilter>();

            // Assert
            var typeFilter = Assert.IsType<TypeFilterAttribute>(added);
            Assert.Equal(typeof(MyFilter), typeFilter.ImplementationType);
            Assert.Same(typeFilter, Assert.Single(collection));
        }

        [Fact]
        public void Add_WithOrder_SetsOrder()
        {
            // Arrange
            var collection = new FilterCollection();

            // Act
            var added = collection.Add(typeof(MyFilter), 17);

            // Assert
            Assert.Equal(17, Assert.IsAssignableFrom<IOrderedFilter>(added).Order);
        }

        [Fact]
        public void GenericAdd_WithOrder_SetsOrder()
        {
            // Arrange
            var collection = new FilterCollection();

            // Act
            var added = collection.Add<MyFilter>(17);

            // Assert
            Assert.Equal(17, Assert.IsAssignableFrom<IOrderedFilter>(added).Order);
        }

        [Fact]
        public void Add_ThrowsOnNonIFilter()
        {
            // Arrange
            var collection = new FilterCollection();
            var implementationType = typeof(NonFilter);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => new TypeFilterAttribute(implementationType));
            Assert.Equal(
                $"The type '{implementationType}' provided to '{typeof(TypeFilterAttribute)}' must implement '{typeof(IFilterMetadata)}'.",
                exception.Message);
        }

        [Fact]
        public void AddService_UsesServiceFilterAttribute()
        {
            // Arrange
            var collection = new FilterCollection();

            // Act
            var added = collection.AddService(typeof(MyFilter));

            // Assert
            var serviceFilter = Assert.IsType<ServiceFilterAttribute>(added);
            Assert.Equal(typeof(MyFilter), serviceFilter.ServiceType);
            Assert.Same(serviceFilter, Assert.Single(collection));
        }

        [Fact]
        public void GenericAddService_UsesServiceFilterAttribute()
        {
            // Arrange
            var collection = new FilterCollection();

            // Act
            var added = collection.AddService<MyFilter>();

            // Assert
            var serviceFilter = Assert.IsType<ServiceFilterAttribute>(added);
            Assert.Equal(typeof(MyFilter), serviceFilter.ServiceType);
            Assert.Same(serviceFilter, Assert.Single(collection));
        }

        [Fact]
        public void AddService_SetsOrder()
        {
            // Arrange
            var collection = new FilterCollection();

            // Act
            var added = collection.AddService(typeof(MyFilter), 17);

            // Assert
            Assert.Equal(17, Assert.IsAssignableFrom<IOrderedFilter>(added).Order);
        }

        [Fact]
        public void GenericAddService_SetsOrder()
        {
            // Arrange
            var collection = new FilterCollection();

            // Act
            var added = collection.AddService<MyFilter>(17);

            // Assert
            Assert.Equal(17, Assert.IsAssignableFrom<IOrderedFilter>(added).Order);
        }

        [Fact]
        public void AddService_ThrowsOnNonIFilter()
        {
            // Arrange
            var collection = new FilterCollection();
            var serviceType = typeof(NonFilter);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => new ServiceFilterAttribute(serviceType));
            Assert.Equal(
                $"The type '{serviceType}' provided to '{typeof(ServiceFilterAttribute)}' must implement '{typeof(IFilterMetadata)}'.",
                exception.Message);
        }

        [Fact]
        public void Add_ThrowsException_IfTypeImplementsIFilterFactory()
        {
            // Arrange
            var collection = new FilterCollection();
            var implementationType = typeof(FilterFactoryImplementingType);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.Add(implementationType));
            Assert.Equal(
                $"Cannot use a filter factory type '{implementationType}' from within another" +
                $" filter factory type '{typeof(TypeFilterAttribute)}'.",
                exception.Message);
        }

        [Fact]
        public void GenericAdd_ThrowsException_IfTypeImplementsIFilterFactory()
        {
            // Arrange
            var collection = new FilterCollection();
            var implementationType = typeof(FilterFactoryImplementingType);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.Add<FilterFactoryImplementingType>());
            Assert.Equal(
                $"Cannot use a filter factory type '{implementationType}' from within another" +
                $" filter factory type '{typeof(TypeFilterAttribute)}'.",
                exception.Message);
        }

        private class MyFilter : IFilterMetadata, IOrderedFilter
        {
            public int Order
            {
                get;
                set;
            }
        }

        private class NonFilter
        {
        }

        private class FilterFactoryImplementingType : IFilterFactory
        {
            public bool IsReusable => throw new NotImplementedException();

            public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
            {
                throw new NotImplementedException();
            }
        }
    }
}
