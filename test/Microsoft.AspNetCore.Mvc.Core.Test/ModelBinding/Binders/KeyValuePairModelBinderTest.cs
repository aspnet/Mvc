// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class KeyValuePairModelBinderTest
    {
        [Fact]
        public async Task BindModel_MissingKey_ReturnsResult_AndAddsModelValidationError()
        {
            // Arrange
            var valueProvider = new SimpleValueProvider();

            // Create string binder to create the value but not the key.
            var bindingContext = GetBindingContext(valueProvider, typeof(KeyValuePair<int, string>));
            var binder = new KeyValuePairModelBinder<int, string>(CreateIntBinder(false), CreateStringBinder());

            // Act
            var result = await binder.BindModelResultAsync(bindingContext);

            // Assert
            Assert.False(result.IsModelSet);
            Assert.Null(result.Model);
            Assert.False(bindingContext.ModelState.IsValid);
            Assert.Equal("someName", bindingContext.ModelName);
            var error = Assert.Single(bindingContext.ModelState["someName.Key"].Errors);
            Assert.Equal("A value is required.", error.ErrorMessage);
        }

        [Fact]
        public async Task BindModel_MissingValue_ReturnsResult_AndAddsModelValidationError()
        {
            // Arrange
            var valueProvider = new SimpleValueProvider();

            // Create int binder to create the value but not the key.
            var bindingContext = GetBindingContext(valueProvider, typeof(KeyValuePair<int, string>));
            var binder = new KeyValuePairModelBinder<int, string>(CreateIntBinder(), CreateStringBinder(false));
            
            // Act
            var result = await binder.BindModelResultAsync(bindingContext);

            // Assert
            Assert.Null(result.Model);
            Assert.False(bindingContext.ModelState.IsValid);
            Assert.Equal("someName", bindingContext.ModelName);
            var state = bindingContext.ModelState["someName.Value"];
            Assert.NotNull(state);
            var error = Assert.Single(state.Errors);
            Assert.Equal("A value is required.", error.ErrorMessage);
        }

        [Fact]
        public async Task BindModel_MissingKeyAndMissingValue_DoNotAddModelStateError()
        {
            // Arrange
            var valueProvider = new SimpleValueProvider();

            // Create int binder to create the value but not the key.
            var bindingContext = GetBindingContext(valueProvider, typeof(KeyValuePair<int, string>));
            var binder = new KeyValuePairModelBinder<int, string>(CreateIntBinder(false), CreateStringBinder(false));

            // Act
            var result = await binder.BindModelResultAsync(bindingContext);

            // Assert
            Assert.Equal(default(ModelBindingResult), result);
            Assert.True(bindingContext.ModelState.IsValid);
            Assert.Equal(0, bindingContext.ModelState.ErrorCount);
        }

        [Fact]
        public async Task BindModel_SubBindingSucceeds()
        {
            // Arrange
            var valueProvider = new SimpleValueProvider();

            var bindingContext = GetBindingContext(valueProvider, typeof(KeyValuePair<int, string>));
            var binder = new KeyValuePairModelBinder<int, string>(CreateIntBinder(), CreateStringBinder());

            // Act
            var result = await binder.BindModelResultAsync(bindingContext);

            // Assert
            Assert.True(result.IsModelSet);
            Assert.Equal(new KeyValuePair<int, string>(42, "some-value"), result.Model);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(null, true)]
        [InlineData(42, true)]
        public async Task TryBindStrongModel_InnerBinderReturnsAResult_ReturnsInnerBinderResult(
            object model,
            bool isSuccess)
        {
            // Arrange
            ModelBindingResult? innerResult;
            if (isSuccess)
            {
                innerResult = ModelBindingResult.Success(model);
            }
            else
            {
                innerResult = ModelBindingResult.Failed();
            }

            var innerBinder = new StubModelBinder(context =>
            {
                Assert.Equal("someName.Key", context.ModelName);
                return innerResult;
            });

            var valueProvider = new SimpleValueProvider();

            var bindingContext = GetBindingContext(valueProvider, typeof(KeyValuePair<int, string>));
            var binder = new KeyValuePairModelBinder<int, string>(innerBinder, innerBinder);

            // Act
            var result = await binder.TryBindStrongModel<int>(bindingContext, innerBinder, "Key", "someName.Key");

            // Assert
            Assert.Equal(innerResult.Value, result);
            Assert.Empty(bindingContext.ModelState);
        }

        [Fact]
        public async Task KeyValuePairModelBinder_CreatesEmptyCollection_IfIsTopLevelObject()
        {
            // Arrange
            var binder = new KeyValuePairModelBinder<string, string>(
                new SimpleTypeModelBinder(typeof(string)), 
                new SimpleTypeModelBinder(typeof(string)));

            var context = CreateContext();
            context.IsTopLevelObject = true;

            // Lack of prefix and non-empty model name both ignored.
            context.ModelName = "modelName";

            var metadataProvider = new TestModelMetadataProvider();
            context.ModelMetadata = metadataProvider.GetMetadataForType(typeof(KeyValuePair<string, string>));

            context.ValueProvider = new TestValueProvider(new Dictionary<string, object>());

            // Act
            var result = await binder.BindModelResultAsync(context);

            // Assert
            var model = Assert.IsType<KeyValuePair<string, string>>(result.Model);
            Assert.Equal(default(KeyValuePair<string, string>), model);
            Assert.True(result.IsModelSet);
        }

        [Theory]
        [InlineData("")]
        [InlineData("param")]
        public async Task KeyValuePairModelBinder_DoesNotCreateCollection_IfNotIsTopLevelObject(string prefix)
        {
            // Arrange
            var binder = new KeyValuePairModelBinder<string, string>(
                new SimpleTypeModelBinder(typeof(string)), 
                new SimpleTypeModelBinder(typeof(string)));

            var context = CreateContext();
            context.ModelName = ModelNames.CreatePropertyModelName(prefix, "KeyValuePairProperty");

            var metadataProvider = new TestModelMetadataProvider();
            context.ModelMetadata = metadataProvider.GetMetadataForProperty(
                typeof(ModelWithKeyValuePairProperty),
                nameof(ModelWithKeyValuePairProperty.KeyValuePairProperty));

            context.ValueProvider = new TestValueProvider(new Dictionary<string, object>());

            // Act
            var result = await binder.BindModelResultAsync(context);

            // Assert
            Assert.Equal(default(ModelBindingResult), result);
        }

        private static DefaultModelBindingContext CreateContext()
        {
            var modelBindingContext = new DefaultModelBindingContext()
            {
                ActionContext = new ActionContext()
                {
                    HttpContext = new DefaultHttpContext(),
                },
                ModelState = new ModelStateDictionary(),
            };

            return modelBindingContext;
        }

        private static DefaultModelBindingContext GetBindingContext(
            IValueProvider valueProvider,
            Type keyValuePairType)
        {
            var metadataProvider = new TestModelMetadataProvider();
            var bindingContext = new DefaultModelBindingContext
            {
                ModelMetadata = metadataProvider.GetMetadataForType(keyValuePairType),
                ModelName = "someName",
                ModelState = new ModelStateDictionary(),
                ValueProvider = valueProvider,
            };
            return bindingContext;
        }

        private static IModelBinder CreateIntBinder(bool success = true)
        {
            var mockIntBinder = new StubModelBinder(mbc =>
            {
                if (mbc.ModelType == typeof(int) && success)
                {
                    var model = 42;
                    return ModelBindingResult.Success(model);
                }
                return null;
            });
            return mockIntBinder;
        }

        private static IModelBinder CreateStringBinder(bool success = true)
        {
            return new StubModelBinder(mbc =>
            {
                if (mbc.ModelType == typeof(string) && success)
                {
                    var model = "some-value";
                    return ModelBindingResult.Success(model);
                }
                return null;
            });
        }

        private class ModelWithKeyValuePairProperty
        {
            public KeyValuePair<string, string> KeyValuePairProperty { get; set; }
        }
    }
}
