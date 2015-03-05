// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNET50
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.ModelBinding.Test
{
    public class ArrayModelBinderTest
    {
        [Fact]
        public async Task BindModel()
        {
            // Arrange
            var valueProvider = new SimpleHttpValueProvider
            {
                { "someName[0]", "42" },
                { "someName[1]", "84" }
            };
            var bindingContext = GetBindingContext(valueProvider);
            var binder = new ArrayModelBinder<int>();

            // Act
            var retVal = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.NotNull(retVal);

            int[] array = retVal.Model as int[];
            Assert.Equal(new[] { 42, 84 }, array);
        }

        [Fact]
        public async Task GetBinder_ValueProviderDoesNotContainPrefix_ReturnsNull()
        {
            // Arrange
            var bindingContext = GetBindingContext(new SimpleHttpValueProvider());
            var binder = new ArrayModelBinder<int>();

            // Act
            var bound = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.Null(bound);
        }

        [Fact]
        public async Task GetBinder_ModelMetadataReturnsReadOnly_ReturnsNull()
        {
            // Arrange
            var valueProvider = new SimpleHttpValueProvider
            {
                { "foo[0]", "42" },
            };
            var bindingContext = GetBindingContext(valueProvider, isReadOnly: true);
            var binder = new ArrayModelBinder<int>();

            // Act
            var bound = await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.Null(bound);
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
                        var model = value.ConvertTo(mbc.ModelType);
                        return new ModelBindingResult(model, key: null, isModelSet: true);
                    }
                    return null;
                });
            return mockIntBinder.Object;
        }

        private static ModelBindingContext GetBindingContext(
            IValueProvider valueProvider,
            bool isReadOnly = false)
        {
            var metadataProvider = new TestModelMetadataProvider();
            metadataProvider.ForType<int[]>().BindingDetails(bd => bd.IsReadOnly = isReadOnly);

            var bindingContext = new ModelBindingContext
            {
                ModelMetadata = metadataProvider.GetMetadataForType(typeof(int[])),
                ModelName = "someName",
                ValueProvider = valueProvider,
                OperationBindingContext = new OperationBindingContext
                {
                    ModelBinder = CreateIntBinder(),
                    MetadataProvider = metadataProvider
                },
            };
            return bindingContext;
        }
    }
}
#endif
