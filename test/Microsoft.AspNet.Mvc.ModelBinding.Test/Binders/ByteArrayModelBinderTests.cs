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

            var bindingContext = GetBindingContext(valueProvider);
            var binder = new ByteArrayModelBinder();

            // Act
            var binderResult = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.False(binderResult);
            Assert.Null(bindingContext.Model);
        }

        [Theory]
        [InlineData("Fys1", new byte[] { 23, 43, 53 })]
        [InlineData("\"Fys1\"", new byte[] { 23, 43, 53 })]
        public async Task BindModel(string input, byte[] expectedOutput)
        {
            // Arrange
            var valueProvider = new SimpleHttpValueProvider()
            {
                { "foo", input }
            };

            var bindingContext = GetBindingContext(valueProvider);
            var binder = new ByteArrayModelBinder();

            // Act
            var binderResult = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(binderResult);
            Assert.Equal(expectedOutput, bindingContext.Model as byte[]);
        }

        [Fact]
        public void BindModelThrowsOnNullBindingContext()
        {
            // Arrange
            var binder = new ByteArrayModelBinder();

            // Act & assert
            Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                return binder.BindModelAsync(null);
            });
        }

        [Fact]
        public async Task BindModelReturnsFalseWhenValueNotFound()
        {
            // Arrange
            var valueProvider = new SimpleHttpValueProvider()
            {
                { "someName", "" }
            };

            var bindingContext = GetBindingContext(valueProvider);
            var binder = new ByteArrayModelBinder();

            // Act
            var binderResult = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.False(binderResult);
        }

        private static IModelBinder CreateIntBinder()
        {
            var mockIntBinder = new Mock<IModelBinder>();
            mockIntBinder
                .Setup(o => o.BindModelAsync(It.IsAny<ModelBindingContext>()))
                .Returns(async (ModelBindingContext mbc) =>
                {
                    var value = await mbc.ValueProvider.GetValueAsync(mbc.ModelName);
                    if (value != null)
                    {
                        mbc.Model = value.ConvertTo(mbc.ModelType);
                        return true;
                    }
                    return false;
                });
            return mockIntBinder.Object;
        }

        private static ModelBindingContext GetBindingContext(IValueProvider valueProvider)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            ModelBindingContext bindingContext = new ModelBindingContext
            {
                ModelMetadata = metadataProvider.GetMetadataForType(null, typeof(int[])),
                ModelName = "foo",
                ValueProvider = valueProvider,
                ModelBinder = CreateIntBinder(),
                MetadataProvider = metadataProvider
            };
            return bindingContext;
        }
    }
}