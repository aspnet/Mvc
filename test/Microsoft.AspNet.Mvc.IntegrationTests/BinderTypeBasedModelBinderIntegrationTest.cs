// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Testing;
using Xunit;

namespace Microsoft.AspNet.Mvc.IntegrationTests
{
    public class BinderTypeBasedModelBinderIntegrationTest
    {
        // The NullModelBinder and NullModelNotSetModelBinder return a non null ModelBindingResult but a null model.
        [Theory(Skip = "ModelBindingResult should be non null if a model binder returns a non null resul #2473.")]
        [InlineData(typeof(NullModelBinder), true)]
        [InlineData(typeof(NullModelNotSetModelBinder), false)]
        public async Task BindParameter_WithModelBinderType_NoData(Type modelBinderType, bool isModelSet)
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "Parameter1",
                BindingInfo = new BindingInfo()
                {
                    BinderType = modelBinderType
                },

                ParameterType = typeof(string)
            };

            // No data is passed.
            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request => { });
            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert

            // ModelBindingResult
            Assert.NotNull(modelBindingResult);
            Assert.Null(modelBindingResult.Model);
            Assert.Equal(isModelSet, modelBindingResult.IsModelSet);

            // ModelState
            Assert.True(modelState.IsValid);
            var key = Assert.Single(modelState.Keys);
            Assert.Equal("CustomParameter", key);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.Equal(ModelValidationState.Valid, modelState[key].ValidationState);
            Assert.Null(modelState[key].Value); // value is only set if the custom model binder sets it.
        }

        private class Person2
        {
        }

        // Since the NullResultModelBinder returns a null ModelBindingResult, it acts
        // as a non greedy model binder, however since it is applied using a BinderTypeBasedModelBinder, 
        // which wraps this model binder and behaves as a greed model binder, we get a non null result.
        [Fact(Skip = "ModelBindingResult should be non null if a model binder returns a non null resul #2473.")]
        public async Task BindParameter_WithModelBinderType_NonGreedy_NoData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "Parameter1",
                BindingInfo = new BindingInfo()
                {
                    BinderType = typeof(NullResultModelBinder)
                },

                ParameterType = typeof(Person2)
            };

            // No data is passed.
            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request => { });
            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert

            // ModelBindingResult
            Assert.NotNull(modelBindingResult);

            // ModelState
            Assert.True(modelState.IsValid);
            Assert.Empty(modelState.Keys);
        }
        
        // ModelBinderAttribute can be used without specifing the binder type. 
        // In such cases BinderTypeBasedModelBinder acts like a non greedy binder where
        // it returns a null ModelBindingResult allowing other ModelBinders to run.
        [Fact]
        public async Task BindParameter_WithOutModelBinderType_NoData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "Parameter1",
                BindingInfo = new BindingInfo()
                {
                    BinderType = typeof(NullResultModelBinder)
                },

                ParameterType = typeof(Person2)
            };

            // No data is passed.
            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request => { });
            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert

            // ModelBindingResult
            Assert.Null(modelBindingResult);

            // ModelState
            Assert.True(modelState.IsValid);
            Assert.Empty(modelState.Keys);
        }

        // Ensures that prefix is part of the result returned back.
        [Fact]
        [ReplaceCulture]
        public async Task BindParameter_WithData_WithPrefix_GetsBound()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "Parameter1",
                BindingInfo = new BindingInfo()
                {
                    BinderType = typeof(SuccessModelBinder),
                    BinderModelName = "CustomParameter"
                },

                ParameterType = typeof(Person2)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request => { });
            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert

            // ModelBindingResult
            Assert.NotNull(modelBindingResult);
            Assert.Equal("Success", modelBindingResult.Model);
            Assert.Equal("CustomParameter", modelBindingResult.Key);

            // ModelState
            Assert.True(modelState.IsValid);
            var key = Assert.Single(modelState.Keys);
            Assert.Equal("CustomParameter", key);
            Assert.Equal(ModelValidationState.Valid, modelState[key].ValidationState);
            Assert.NotNull(modelState[key].Value); // Value is set by test model binder, no need to validate it.
        }

        private class Person
        {
            public Address Address { get; set; }
        }

        [ModelBinder(BinderType = typeof(AddressModelBinder))]
        private class Address
        {
            public string Street { get; set; }
        }

        [Fact]
        public async Task BindProperty_WithData_EmptyPrefix_GetsBound()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "Parameter1",
                BindingInfo = new BindingInfo(),
                ParameterType = typeof(Person)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request => { });
            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert

            // ModelBindingResult
            Assert.NotNull(modelBindingResult);
            Assert.Equal("Parameter1", modelBindingResult.Key);

            // Model
            var boundPerson = Assert.IsType<Person>(modelBindingResult.Model);
            Assert.NotNull(boundPerson.Address);
            Assert.Equal("SomeStreet", boundPerson.Address.Street);

            // ModelState
            Assert.True(modelState.IsValid);

            Assert.Equal(1, modelState.Keys.Count);
            var key = Assert.Single(modelState.Keys, k => k == "Parameter1.Address.Street");
            Assert.Equal(ModelValidationState.Valid, modelState[key].ValidationState);
            Assert.NotNull(modelState[key].Value); // Value is set by test model binder, no need to validate it.
        }

        [Fact]
        public async Task BindProperty_WithData_WithPrefix_GetsBound()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "Parameter1",
                BindingInfo = new BindingInfo()
                {
                    BinderModelName = "CustomParameter"
                },
                ParameterType = typeof(Person)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request => { });
            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert

            // ModelBindingResult
            Assert.NotNull(modelBindingResult);
            Assert.Equal("CustomParameter", modelBindingResult.Key);

            // Model
            var boundPerson = Assert.IsType<Person>(modelBindingResult.Model);
            Assert.NotNull(boundPerson.Address);
            Assert.Equal("SomeStreet", boundPerson.Address.Street);

            // ModelState
            Assert.True(modelState.IsValid);
            Assert.Equal(1, modelState.Keys.Count);
            var key = Assert.Single(modelState.Keys, k => k == "CustomParameter.Address.Street");
            Assert.Equal(ModelValidationState.Valid, modelState[key].ValidationState);
            Assert.NotNull(modelState[key].Value); // Value is set by test model binder, no need to validate it.
        }

        private class AddressModelBinder : IModelBinder
        {
            public Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
            {
                if (bindingContext.ModelType != typeof(Address))
                {
                    return null;
                }

                var address = new Address() { Street = "SomeStreet" };

                bindingContext.ModelState.SetModelValue(
                  ModelNames.CreatePropertyModelName(bindingContext.ModelName, "Street"),
                  new ValueProviderResult(
                      address.Street,
                      address.Street,
                      CultureInfo.CurrentCulture));

                var validationNode = new ModelValidationNode(
                  bindingContext.ModelName,
                  bindingContext.ModelMetadata,
                  address)
                {
                    ValidateAllProperties = true
                };

                return Task.FromResult(new ModelBindingResult(address, bindingContext.ModelName, true, validationNode));
            }
        }

        private class SuccessModelBinder : IModelBinder
        {
            public Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
            {
                var model = "Success";
                bindingContext.ModelState.SetModelValue(
                    bindingContext.ModelName,
                    new ValueProviderResult(model, model, CultureInfo.CurrentCulture));

                var modelValidationNode = new ModelValidationNode(
                    bindingContext.ModelName,
                    bindingContext.ModelMetadata,
                    model);
                return Task.FromResult(new ModelBindingResult(model, bindingContext.ModelName, true, modelValidationNode));
            }
        }

        private class NullModelBinder : IModelBinder
        {
            public Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
            {
                return Task.FromResult(new ModelBindingResult(null, bindingContext.ModelName, true));
            }
        }

        private class NullModelNotSetModelBinder : IModelBinder
        {
            public Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
            {
                return Task.FromResult(new ModelBindingResult(null, bindingContext.ModelName, false));
            }
        }

        private class NullResultModelBinder : IModelBinder
        {
            public Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
            {
                return Task.FromResult<ModelBindingResult>(null);
            }
        }
    }
}