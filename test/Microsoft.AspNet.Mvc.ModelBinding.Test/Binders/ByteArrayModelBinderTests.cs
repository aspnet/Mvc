// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Test
{
    public class ByteArrayModelBinderTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task BindModelSetsModelToNullOnNullOrEmptyString(string value)
        {
            // Arrange
            var valueProvider = new SimpleHttpValueProvider()
            {
                { "foo", value }
            };

            var bindingContext = GetBindingContext(valueProvider, typeof(byte[]));
            var binder = new ByteArrayModelBinder();

            // Act
            var binderResult = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.False(binderResult);
            Assert.Null(bindingContext.Model);
        }

        [Fact]
        public async Task BindModel()
        {
            // Arrange
            var valueProvider = new SimpleHttpValueProvider()
            {
                { "foo", "Fys1" }
            };

            var bindingContext = GetBindingContext(valueProvider, typeof(byte[]));
            var binder = new ByteArrayModelBinder();

            // Act
            var binderResult = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(binderResult);
            Assert.IsType<byte[]>(bindingContext.Model);
            Assert.Equal(new byte[] { 23, 43, 53 }, bindingContext.Model as byte[]);
        }

        [Fact]
        public async Task BindModelThrowsOnInvalidCharacters()
        {
            // Arrange
            var valueProvider = new SimpleHttpValueProvider()
            {
                { "foo", "\"Fys1\"" }
            };

            var bindingContext = GetBindingContext(valueProvider, typeof(byte[]));
            var binder = new ByteArrayModelBinder();

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(() => binder.BindModelAsync(bindingContext));
        }

        [Fact]
        public async Task BindModelThrowsOnNullBindingContext()
        {
            // Arrange
            var binder = new ByteArrayModelBinder();

            // Act & assert
            await Assert.ThrowsAsync<NullReferenceException>(() => binder.BindModelAsync(null));
        }

        [Fact]
        public async Task BindModelReturnsFalseWhenValueNotFound()
        {
            // Arrange
            var valueProvider = new SimpleHttpValueProvider()
            {
                { "someName", "" }
            };

            var bindingContext = GetBindingContext(valueProvider, typeof(byte[]));
            var binder = new ByteArrayModelBinder();

            // Act
            var binderResult = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.False(binderResult);
        }

        [Fact]
        public async Task ByteArrayModelBinderReturnsFalseForOtherTypes()
        {
            // Arrange
            var bindingContext = GetBindingContext(null, typeof(int[]));
            var binder = new ByteArrayModelBinder();

            // Act
            var binderResult = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.False(binderResult);
        }

        private static ModelBindingContext GetBindingContext(IValueProvider valueProvider, Type modelType)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = metadataProvider.GetMetadataForType(null, modelType),
                ModelName = "foo",
                ValueProvider = valueProvider,
                MetadataProvider = metadataProvider
            };
            return bindingContext;
        }
    }
}