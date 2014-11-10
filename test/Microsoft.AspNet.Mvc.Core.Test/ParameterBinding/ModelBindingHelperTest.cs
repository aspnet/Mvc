// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNET50
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class ModelBindingHelperTest
    {
        [Fact]
        public async Task TryUpdateModel_ReturnsFalse_IfBinderReturnsFalse()
        {
            // Arrange
            var metadataProvider = new Mock<IModelMetadataProvider>();
            metadataProvider.Setup(m => m.GetMetadataForType(null, It.IsAny<Type>()))
                            .Returns(new ModelMetadata(metadataProvider.Object, null, null, typeof(MyModel), null))
                            .Verifiable();

            var binder = new Mock<IModelBinder>();
            binder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
                  .Returns(Task.FromResult(false));
            var model = new MyModel();

            // Act
            var result = await ModelBindingHelper.TryUpdateModelAsync(
                                                    model,
                                                    null,
                                                    Mock.Of<HttpContext>(),
                                                    new ModelStateDictionary(),
                                                    metadataProvider.Object,
                                                    GetCompositeBinder(binder.Object),
                                                    Mock.Of<IValueProvider>(),
                                                    Mock.Of<IModelValidatorProvider>());

            // Assert
            Assert.False(result);
            Assert.Null(model.MyProperty);
            metadataProvider.Verify();
        }

        [Fact]
        public async Task TryUpdateModel_ReturnsFalse_IfModelValidationFails()
        {
            // Arrange
            var expectedMessage = TestPlatformHelper.IsMono ? "The field MyProperty is invalid." :
                                                               "The MyProperty field is required.";
            var binders = new IModelBinder[]
            {
                new TypeConverterModelBinder(),
                new ComplexModelDtoModelBinder(),
                new MutableObjectModelBinder()
            };

            var validator = new DataAnnotationsModelValidatorProvider();
            var model = new MyModel();
            var modelStateDictionary = new ModelStateDictionary();
            var values = new Dictionary<string, object>
            {
                { "", null }
            };
            var valueProvider = new DictionaryBasedValueProvider<TestValueBinderMetadata>(values);

            // Act
            var result = await ModelBindingHelper.TryUpdateModelAsync(
                                                    model,
                                                    "",
                                                    Mock.Of<HttpContext>(),
                                                    modelStateDictionary,
                                                    new DataAnnotationsModelMetadataProvider(),
                                                    GetCompositeBinder(binders),
                                                    valueProvider,
                                                    validator);

            // Assert
            Assert.False(result);
            var error = Assert.Single(modelStateDictionary["MyProperty"].Errors);
            Assert.Equal(expectedMessage, error.ErrorMessage);
        }

        [Fact]
        public async Task TryUpdateModel_ReturnsTrue_IfModelBindsAndValidatesSuccessfully()
        {
            // Arrange
            var binders = new IModelBinder[]
            {
                new TypeConverterModelBinder(),
                new ComplexModelDtoModelBinder(),
                new MutableObjectModelBinder()
            };

            var validator = new DataAnnotationsModelValidatorProvider();
            var model = new MyModel { MyProperty = "Old-Value" };
            var modelStateDictionary = new ModelStateDictionary();
            var values = new Dictionary<string, object>
            {
                { "", null },
                { "MyProperty", "MyPropertyValue" }
            };
            var valueProvider = new DictionaryBasedValueProvider<TestValueBinderMetadata>(values);

            // Act
            var result = await ModelBindingHelper.TryUpdateModelAsync(
                                                    model,
                                                    "",
                                                    Mock.Of<HttpContext>(),
                                                    modelStateDictionary,
                                                    new DataAnnotationsModelMetadataProvider(),
                                                    GetCompositeBinder(binders),
                                                    valueProvider,
                                                    validator);

            // Assert
            Assert.True(result);
            Assert.Equal("MyPropertyValue", model.MyProperty);
        }

        [Fact]
        public async Task TryUpdateModel_UsingIncludePredicateOverload_ReturnsFalse_IfBinderReturnsFalse()
        {
            // Arrange
            var metadataProvider = new Mock<IModelMetadataProvider>();
            metadataProvider.Setup(m => m.GetMetadataForType(null, It.IsAny<Type>()))
                            .Returns(new ModelMetadata(metadataProvider.Object, null, null, typeof(MyModel), null))
                            .Verifiable();

            var binder = new Mock<IModelBinder>();
            binder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
                  .Returns(Task.FromResult(false));
            var model = new MyModel();
            Func<ModelBindingContext, string, bool> includePredicate =
               (context, propertyName) => string.Equals(propertyName, "IncludedProperty", StringComparison.OrdinalIgnoreCase) &&
                               !string.Equals(propertyName, "ExcludedProperty", StringComparison.OrdinalIgnoreCase);
            // Act
            var result = await ModelBindingHelper.TryUpdateModelAsync(
                                                    model,
                                                    null,
                                                    Mock.Of<HttpContext>(),
                                                    new ModelStateDictionary(),
                                                    metadataProvider.Object,
                                                    GetCompositeBinder(binder.Object),
                                                    Mock.Of<IValueProvider>(),
                                                    Mock.Of<IModelValidatorProvider>(),
                                                    includePredicate);

            // Assert
            Assert.False(result);
            Assert.Null(model.MyProperty);
            Assert.Null(model.IncludedProperty);
            Assert.Null(model.ExcludedProperty);
            metadataProvider.Verify();
        }

        [Fact]
        public async Task TryUpdateModel_UsingIncludePredicateOverload_ReturnsTrue_ModelBindsAndValidatesSuccessfully()
        {
            // Arrange
            var binders = new IModelBinder[]
            {
                new TypeConverterModelBinder(),
                new ComplexModelDtoModelBinder(),
                new MutableObjectModelBinder()
            };

            var validator = new DataAnnotationsModelValidatorProvider();
            var model = new MyModel {
                MyProperty = "Old-Value",
                IncludedProperty = "Old-IncludedPropertyValue",
                ExcludedProperty = "Old-ExcludedPropertyValue"
            };

            var modelStateDictionary = new ModelStateDictionary();
            var values = new Dictionary<string, object>
            {
                { "", null },
                { "MyProperty", "MyPropertyValue" },
                { "IncludedProperty", "IncludedPropertyValue" },
                { "ExcludedProperty", "ExcludedPropertyValue" }
            };

            Func<ModelBindingContext, string, bool> includePredicate =
                (context, propertyName) => 
                                (string.Equals(propertyName, "IncludedProperty", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(propertyName, "MyProperty", StringComparison.OrdinalIgnoreCase)) &&
                                !string.Equals(propertyName, "ExcludedProperty", StringComparison.OrdinalIgnoreCase);

            var valueProvider = new DictionaryBasedValueProvider<TestValueBinderMetadata>(values);

            // Act
            var result = await ModelBindingHelper.TryUpdateModelAsync(
                                                    model,
                                                    "",
                                                    Mock.Of<HttpContext>(),
                                                    modelStateDictionary,
                                                    new DataAnnotationsModelMetadataProvider(),
                                                    GetCompositeBinder(binders),
                                                    valueProvider,
                                                    validator,
                                                    includePredicate);

            // Assert
            Assert.True(result);
            Assert.Equal("MyPropertyValue", model.MyProperty);
            Assert.Equal("IncludedPropertyValue", model.IncludedProperty);
            Assert.Equal("Old-ExcludedPropertyValue", model.ExcludedProperty);
        }

        [Fact]
        public async Task TryUpdateModel_UsingIncludeExpressionOverload_ReturnsFalse_IfBinderReturnsFalse()
        {
            // Arrange
            var metadataProvider = new Mock<IModelMetadataProvider>();
            metadataProvider.Setup(m => m.GetMetadataForType(null, It.IsAny<Type>()))
                            .Returns(new ModelMetadata(metadataProvider.Object, null, null, typeof(MyModel), null))
                            .Verifiable();

            var binder = new Mock<IModelBinder>();
            binder.Setup(b => b.BindModelAsync(It.IsAny<ModelBindingContext>()))
                  .Returns(Task.FromResult(false));
            var model = new MyModel();

            // Act
            var result = await ModelBindingHelper.TryUpdateModelAsync(
                                                    model,
                                                    null,
                                                    Mock.Of<HttpContext>(),
                                                    new ModelStateDictionary(),
                                                    metadataProvider.Object,
                                                    GetCompositeBinder(binder.Object),
                                                    Mock.Of<IValueProvider>(),
                                                    Mock.Of<IModelValidatorProvider>(),
                                                    m => m.IncludedProperty );

            // Assert
            Assert.False(result);
            Assert.Null(model.MyProperty);
            Assert.Null(model.IncludedProperty);
            Assert.Null(model.ExcludedProperty);
            metadataProvider.Verify();
        }

        [Fact]
        public async Task TryUpdateModel_UsingIncludeExpressionOverload_ReturnsTrue_ModelBindsAndValidatesSuccessfully()
        {
            // Arrange
            var binders = new IModelBinder[]
            {
                new TypeConverterModelBinder(),
                new ComplexModelDtoModelBinder(),
                new MutableObjectModelBinder()
            };

            var validator = new DataAnnotationsModelValidatorProvider();
            var model = new MyModel
            {
                MyProperty = "Old-Value",
                IncludedProperty = "Old-IncludedPropertyValue",
                ExcludedProperty = "Old-ExcludedPropertyValue"
            };

            var modelStateDictionary = new ModelStateDictionary();
            var values = new Dictionary<string, object>
            {
                { "", null },
                { "MyProperty", "MyPropertyValue" },
                { "IncludedProperty", "IncludedPropertyValue" },
                { "ExcludedProperty", "ExcludedPropertyValue" }
            };

            var valueProvider = new DictionaryBasedValueProvider<TestValueBinderMetadata>(values);

            // Act
            var result = await ModelBindingHelper.TryUpdateModelAsync(
                                                    model,
                                                    "",
                                                    Mock.Of<HttpContext>(),
                                                    modelStateDictionary,
                                                    new DataAnnotationsModelMetadataProvider(),
                                                    GetCompositeBinder(binders),
                                                    valueProvider,
                                                    validator,
                                                    m => m.IncludedProperty, m => m.MyProperty);

            // Assert
            Assert.True(result);
            Assert.Equal("MyPropertyValue", model.MyProperty);
            Assert.Equal("IncludedPropertyValue", model.IncludedProperty);
            Assert.Equal("Old-ExcludedPropertyValue", model.ExcludedProperty);
        }

        [Fact]
        public async Task TryUpdateModel_UsingDefaultIncludeOverload_IncludesAllProperties()
        {
            // Arrange
            var binders = new IModelBinder[]
            {
                new TypeConverterModelBinder(),
                new ComplexModelDtoModelBinder(),
                new MutableObjectModelBinder()
            };

            var validator = new DataAnnotationsModelValidatorProvider();
            var model = new MyModel
            {
                MyProperty = "Old-Value",
                IncludedProperty = "Old-IncludedPropertyValue",
                ExcludedProperty = "Old-ExcludedPropertyValue"
            };

            var modelStateDictionary = new ModelStateDictionary();
            var values = new Dictionary<string, object>
            {
                { "", null },
                { "MyProperty", "MyPropertyValue" },
                { "IncludedProperty", "IncludedPropertyValue" },
                { "ExcludedProperty", "ExcludedPropertyValue" }
            };

            var valueProvider = new DictionaryBasedValueProvider<TestValueBinderMetadata>(values);

            // Act
            var result = await ModelBindingHelper.TryUpdateModelAsync(
                                                    model,
                                                    "",
                                                    Mock.Of<HttpContext>(),
                                                    modelStateDictionary,
                                                    new DataAnnotationsModelMetadataProvider(),
                                                    GetCompositeBinder(binders),
                                                    valueProvider,
                                                    validator);

            // Assert
            // Includes everything.
            Assert.True(result);
            Assert.Equal("MyPropertyValue", model.MyProperty);
            Assert.Equal("IncludedPropertyValue", model.IncludedProperty);
            Assert.Equal("ExcludedPropertyValue", model.ExcludedProperty);
        }

        private static IModelBinder GetCompositeBinder(params IModelBinder[] binders)
        {
            var binderProvider = new Mock<IModelBinderProvider>();
            binderProvider.SetupGet(p => p.ModelBinders)
                          .Returns(binders);
            return new CompositeModelBinder(binderProvider.Object);
        }

        private class MyModel
        {
            [Required]
            public string MyProperty { get; set; }

            public string IncludedProperty { get; set; }

            public string ExcludedProperty { get; set; }
        }

        private class TestValueBinderMetadata : IValueProviderMetadata
        {
        }
    }
}
#endif
