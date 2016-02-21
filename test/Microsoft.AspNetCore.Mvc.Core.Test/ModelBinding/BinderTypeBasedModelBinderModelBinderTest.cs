// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Test
{
    public class BinderTypeBasedModelBinderModelBinderTest
    {
        [Fact]
        public async Task BindModel_ReturnsNothing_IfNoBinderTypeIsSet()
        {
            // Arrange
            var bindingContext = GetBindingContext(typeof(Person));

            var binder = new BinderTypeBasedModelBinder();

            // Act
            var binderResult = await binder.BindModelResultAsync(bindingContext);

            // Assert
            Assert.Equal(default(ModelBindingResult), binderResult);
        }

        [Fact]
        public async Task BindModel_ReturnsFailedResult_EvenIfSelectedBinderReturnsNull()
        {
            // Arrange
            var bindingContext = GetBindingContext(typeof(Person), binderType: typeof(NullModelBinder));

            var binder = new BinderTypeBasedModelBinder();

            // Act
            var binderResult = await binder.BindModelResultAsync(bindingContext);

            // Assert
            Assert.NotEqual(default(ModelBindingResult), binderResult);
            Assert.False(binderResult.IsModelSet);
        }

        [Fact]
        public async Task BindModel_CallsBindAsync_OnProvidedModelBinder()
        {
            // Arrange
            var bindingContext = GetBindingContext(typeof(Person), binderType: typeof(NotNullModelBinder));

            var model = new Person();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IModelBinder, NullModelBinder>()
                .BuildServiceProvider();

            bindingContext.OperationBindingContext.HttpContext.RequestServices = serviceProvider;

            var binder = new BinderTypeBasedModelBinder();

            // Act
            var binderResult = await binder.BindModelResultAsync(bindingContext);

            // Assert
            var p = (Person)binderResult.Model;
            Assert.Equal(model.Age, p.Age);
            Assert.Equal(model.Name, p.Name);
        }

        [Fact]
        public async Task BindModel_ForNonModelBinder_Throws()
        {
            // Arrange
            var bindingContext = GetBindingContext(typeof(Person), binderType: typeof(Person));
            var binder = new BinderTypeBasedModelBinder();

            var expected = $"The type '{typeof(Person).FullName}' must implement " +
                $"'{typeof(IModelBinder).FullName}' to be used as a model binder.";

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => binder.BindModelResultAsync(bindingContext));

            // Assert
            Assert.Equal(expected, ex.Message);
        }

        private static DefaultModelBindingContext GetBindingContext(Type modelType, Type binderType = null)
        {
            var metadataProvider = new TestModelMetadataProvider();
            metadataProvider.ForType(modelType).BindingDetails(bd => bd.BinderType = binderType);

            var operationBindingContext = new OperationBindingContext
            {
                ActionContext = new ActionContext()
                {
                    HttpContext = new DefaultHttpContext(),
                },
                MetadataProvider = metadataProvider,
                ValidatorProvider = Mock.Of<IModelValidatorProvider>(),
            };

            var bindingContext = new DefaultModelBindingContext
            {
                ModelMetadata = metadataProvider.GetMetadataForType(modelType),
                ModelName = "someName",
                ValueProvider = Mock.Of<IValueProvider>(),
                ModelState = new ModelStateDictionary(),
                OperationBindingContext = operationBindingContext,
                BinderType = binderType
            };

            return bindingContext;
        }

        private class Person
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }

        private class NullModelBinder : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                return Task.FromResult(0);
            }
        }

        private class NotNullModelBinder : IModelBinder
        {
            private readonly object _model;

            public NotNullModelBinder()
            {
                _model = new Person();
            }

            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                bindingContext.Result = ModelBindingResult.Success(bindingContext.ModelName, _model);
                return Task.FromResult(0);
            }
        }
    }
}
