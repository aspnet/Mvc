// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class ArrayModelBinderTest
    {
        [Fact]
        public async Task BindModelAsync_ValueProviderContainPrefix_Succeeds()
        {
            // Arrange
            var valueProvider = new SimpleValueProvider
            {
                { "someName[0]", "42" },
                { "someName[1]", "84" },
            };

            var bindingContext = GetBindingContext(valueProvider);
            var metadataProvider = new TestModelMetadataProvider();
            bindingContext.ModelMetadata = metadataProvider.GetMetadataForProperty(
                typeof(ModelWithIntArrayProperty),
                nameof(ModelWithIntArrayProperty.ArrayProperty));

            var binder = new ArrayModelBinder<int>(new SimpleTypeModelBinder(typeof(int)));

            // Act
            var result = await binder.BindModelResultAsync(bindingContext);

            // Assert
            Assert.True(result.IsModelSet);

            var array = Assert.IsType<int[]>(result.Model);
            Assert.Equal(new[] { 42, 84 }, array);
        }

        [Fact]
        public async Task ArrayModelBinder_CreatesEmptyCollection_IfIsTopLevelObject()
        {
            // Arrange
            var binder = new ArrayModelBinder<string>(new SimpleTypeModelBinder(typeof(string)));

            var context = CreateContext();
            context.IsTopLevelObject = true;

            // Lack of prefix and non-empty model name both ignored.
            context.ModelName = "modelName";

            var metadataProvider = new TestModelMetadataProvider();
            context.ModelMetadata = metadataProvider.GetMetadataForType(typeof(string[]));

            context.ValueProvider = new TestValueProvider(new Dictionary<string, object>());

            // Act
            var result = await binder.BindModelResultAsync(context);

            // Assert
            Assert.Empty(Assert.IsType<string[]>(result.Model));
            Assert.True(result.IsModelSet);
        }

        [Theory]
        [InlineData("")]
        [InlineData("param")]
        public async Task ArrayModelBinder_DoesNotCreateCollection_IfNotIsTopLevelObject(string prefix)
        {
            // Arrange
            var binder = new ArrayModelBinder<string>(new SimpleTypeModelBinder(typeof(string)));

            var context = CreateContext();
            context.ModelName = ModelNames.CreatePropertyModelName(prefix, "ArrayProperty");

            var metadataProvider = new TestModelMetadataProvider();
            context.ModelMetadata = metadataProvider.GetMetadataForProperty(
                typeof(ModelWithArrayProperty),
                nameof(ModelWithArrayProperty.ArrayProperty));

            context.ValueProvider = new TestValueProvider(new Dictionary<string, object>());

            // Act
            var result = await binder.BindModelResultAsync(context);

            // Assert
            Assert.Equal(default(ModelBindingResult), result);
        }

        public static TheoryData<int[]> ArrayModelData
        {
            get
            {
                return new TheoryData<int[]>
                {
                    new int[0],
                    new [] { 357 },
                    new [] { 357, 357 },
                };
            }
        }

        // Here "fails silently" means the call does not update the array but also does not throw or set an error.
        [Theory]
        [MemberData(nameof(ArrayModelData))]
        public async Task BindModelAsync_ModelMetadataNotReadOnly_ModelNonNull_FailsSilently(int[] model)
        {
            // Arrange
            var arrayLength = model.Length;
            var valueProvider = new SimpleValueProvider
            {
                { "someName[0]", "42" },
                { "someName[1]", "84" },
            };

            var bindingContext = GetBindingContext(valueProvider);
            bindingContext.Model = model;

            var metadataProvider = new TestModelMetadataProvider();
            metadataProvider.ForProperty(
                typeof(ModelWithIntArrayProperty),
                nameof(ModelWithIntArrayProperty.ArrayProperty)).BindingDetails(bd => bd.IsReadOnly = false);
            bindingContext.ModelMetadata = metadataProvider.GetMetadataForProperty(
                typeof(ModelWithIntArrayProperty),
                nameof(ModelWithIntArrayProperty.ArrayProperty));

            var binder = new ArrayModelBinder<int>(new SimpleTypeModelBinder(typeof(int)));

            // Act
            var result = await binder.BindModelResultAsync(bindingContext);

            // Assert
            Assert.True(result.IsModelSet);
            Assert.Same(model, result.Model);

            for (var i = 0; i < arrayLength; i++)
            {
                // Array should be unchanged.
                Assert.Equal(357, model[i]);
            }
        }

        private static IModelBinder CreateIntBinder()
        {
            return new StubModelBinder(mbc =>
            {
                var value = mbc.ValueProvider.GetValue(mbc.ModelName);
                if (value != ValueProviderResult.None)
                {
                    var model = value.ConvertTo(mbc.ModelType);
                    return ModelBindingResult.Success(model);
                }
                return null;
            });
        }

        private static DefaultModelBindingContext GetBindingContext(IValueProvider valueProvider)
        {
            var bindingContext = new DefaultModelBindingContext()
            {
                ModelName = "someName",
                ModelState = new ModelStateDictionary(),
                ValueProvider = valueProvider,
            };
            return bindingContext;
        }

        private static DefaultModelBindingContext CreateContext()
        {
            var modelBindingContext = new DefaultModelBindingContext()
            {
                ActionContext = new ActionContext()
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            return modelBindingContext;
        }

        private class ModelWithArrayProperty
        {
            public string[] ArrayProperty { get; set; }
        }

        private class ModelWithIntArrayProperty
        {
            public int[] ArrayProperty { get; set; }
        }
    }
}

