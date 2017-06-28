// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    public class ModelBinderProviderCollectionTests
    {
        [Fact]
        public void RemoveType_RemovesAllOfType()
        {
            // Arrange
            var collection = new ModelBinderProviderCollection
            {
                new FooModelBinderProvider(),
                new BarModelBinderProvider(),
                new FooModelBinderProvider()
            };

            // Act
            collection.RemoveType(typeof(FooModelBinderProvider));

            // Assert
            var formatter = Assert.Single(collection);
            Assert.IsType<BarModelBinderProvider>(formatter);
        }

        [Fact]
        public void GenericRemoveType_RemovesAllOfType()
        {
            // Arrange
            var collection = new ModelBinderProviderCollection
            {
                new FooModelBinderProvider(),
                new BarModelBinderProvider(),
                new FooModelBinderProvider()
            };

            // Act
            collection.RemoveType<FooModelBinderProvider>();

            // Assert
            var formatter = Assert.Single(collection);
            Assert.IsType<BarModelBinderProvider>(formatter);
        }

        private class FooModelBinderProvider : IModelBinderProvider
        {
            public IModelBinder GetBinder(ModelBinderProviderContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class BarModelBinderProvider : IModelBinderProvider
        {
            public IModelBinder GetBinder(ModelBinderProviderContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
