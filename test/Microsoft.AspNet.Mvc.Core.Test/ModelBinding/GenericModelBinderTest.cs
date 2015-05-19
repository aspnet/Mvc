// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc.ModelBinding;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class GenericModelBinderTest
    {
        [Fact]
        public async Task GenericModelBinder_DoesNotCreateCollection_ForTopLevelModel_OnFirstPass()
        {
            // Arrange
            var binder = new GenericModelBinder();

            var context = CreateContext();
            context.ModelName = "param";

            var metadataProvider = context.OperationBindingContext.MetadataProvider;
            context.ModelMetadata = metadataProvider.GetMetadataForType(typeof(List<string>));

            context.ValueProvider = new TestValueProvider(new Dictionary<string, object>());

            // Act
            var result = await binder.BindModelAsync(context);

            // Assert
            Assert.NotNull(result);

            Assert.Null(result.Model);
            Assert.Equal("param", result.Key);
            Assert.False(result.IsModelSet);
            Assert.Null(result.ValidationNode);
        }

        [Fact]
        public async Task GenericModelBinder_CreatesEmptyCollection_ForTopLevelModel_OnFallback()
        {
            // Arrange
            var binder = new GenericModelBinder();

            var context = CreateContext();
            context.ModelName = string.Empty;

            var metadataProvider = context.OperationBindingContext.MetadataProvider;
            context.ModelMetadata = metadataProvider.GetMetadataForType(typeof(List<string>));

            context.ValueProvider = new TestValueProvider(new Dictionary<string, object>());

            // Act
            var result = await binder.BindModelAsync(context);

            // Assert
            Assert.NotNull(result);

            Assert.Empty(Assert.IsType<List<string>>(result.Model));
            Assert.Equal(string.Empty, result.Key);
            Assert.True(result.IsModelSet);

            Assert.Same(result.ValidationNode.Model, result.Model);
            Assert.Same(result.ValidationNode.Key, result.Key);
            Assert.Same(result.ValidationNode.ModelMetadata, context.ModelMetadata);
        }

        [Fact]
        public async Task GenericModelBinder_CreatesEmptyCollection_ForTopLevelModel_WithExplicitPrefix()
        {
            // Arrange
            var binder = new GenericModelBinder();

            var context = CreateContext();
            context.ModelName = "prefix";
            context.BinderModelName = "prefix";

            var metadataProvider = context.OperationBindingContext.MetadataProvider;
            context.ModelMetadata = metadataProvider.GetMetadataForType(typeof(List<string>));

            context.ValueProvider = new TestValueProvider(new Dictionary<string, object>());

            // Act
            var result = await binder.BindModelAsync(context);

            // Assert
            Assert.NotNull(result);

            Assert.Empty(Assert.IsType<List<string>>(result.Model));
            Assert.Equal("prefix", result.Key);
            Assert.True(result.IsModelSet);

            Assert.Same(result.ValidationNode.Model, result.Model);
            Assert.Same(result.ValidationNode.Key, result.Key);
            Assert.Same(result.ValidationNode.ModelMetadata, context.ModelMetadata);
        }

        [Theory]
        [InlineData("")]
        [InlineData("param")]
        public async Task GenericModelBinder_DoesNotCreateCollection_ForNonTopLevelModel(string prefix)
        {
            // Arrange
            var binder = new GenericModelBinder();

            var context = CreateContext();
            context.ModelName = ModelNames.CreatePropertyModelName(prefix, "ListProperty");

            var metadataProvider = context.OperationBindingContext.MetadataProvider;
            context.ModelMetadata = metadataProvider.GetMetadataForProperty(
                typeof(ModelWithListProperty),
                nameof(ModelWithListProperty.ListProperty));

            context.ValueProvider = new TestValueProvider(new Dictionary<string, object>());

            // Act
            var result = await binder.BindModelAsync(context);

            // Assert
            Assert.NotNull(result);

            Assert.Null(result.Model);
            Assert.Same(context.ModelName, result.Key);
            Assert.False(result.IsModelSet);
            Assert.Null(result.ValidationNode);
        }

        private class ModelWithListProperty
        {
            public List<string> ListProperty { get; set; }
        }

        private static ModelBindingContext CreateContext()
        {
            var modelBindingContext = new ModelBindingContext()
            {
                OperationBindingContext = new OperationBindingContext()
                {
                    HttpContext = new DefaultHttpContext(),
                    MetadataProvider = new TestModelMetadataProvider(),
                }
            };

            return modelBindingContext;
        }
    }
}
