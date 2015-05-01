﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding;
using Xunit;

namespace Microsoft.AspNet.Mvc.IntegrationTests
{
    // Integration tests targeting the behavior of the CollectionModelBinder with other model binders.
    //
    // Note that CollectionModelBinder handles both ICollection{T} and IList{T}
    public class CollectionModelBinderIntegrationTest
    {
        [Fact(Skip = "Extra ModelState key because of #2446")]
        public async Task CollectionModelBinder_BindsListOfSimpleType_WithPrefix_Success()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(List<int>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter[0]=10&parameter[1]=11");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<List<int>>(modelBindingResult.Model);
            Assert.Equal(new List<int>() { 10, 11 }, model);

            Assert.Equal(2, modelState.Count); // This fails due to #2446
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[0]").Value;
            Assert.Equal("10", entry.Value.AttemptedValue);
            Assert.Equal("10", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[1]").Value;
            Assert.Equal("11", entry.Value.AttemptedValue);
            Assert.Equal("11", entry.Value.RawValue);
        }

        [Fact(Skip = "Extra ModelState key because of #2446")]
        public async Task CollectionModelBinder_BindsListOfSimpleType_WithExplicitPrefix_Success()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                BindingInfo = new BindingInfo()
                {
                    BinderModelName = "prefix",
                },
                ParameterType = typeof(List<int>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?prefix[0]=10&prefix[1]=11");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<List<int>>(modelBindingResult.Model);
            Assert.Equal(new List<int>() { 10, 11 }, model);

            Assert.Equal(2, modelState.Count); // This fails due to #2446
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[0]").Value;
            Assert.Equal("10", entry.Value.AttemptedValue);
            Assert.Equal("10", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[1]").Value;
            Assert.Equal("11", entry.Value.AttemptedValue);
            Assert.Equal("11", entry.Value.RawValue);
        }

        [Fact(Skip = "Extra ModelState key because of #2446")]
        public async Task CollectionModelBinder_BindsCollectionOfSimpleType_EmptyPrefix_Success()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(ICollection<int>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?[0]=10&[1]=11");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<List<int>>(modelBindingResult.Model);
            Assert.Equal(new List<int> { 10, 11 }, model);

            Assert.Equal(2, modelState.Count); // This fails due to #2446
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "[0]").Value;
            Assert.Equal("10", entry.Value.AttemptedValue);
            Assert.Equal("10", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "[1]").Value;
            Assert.Equal("11", entry.Value.AttemptedValue);
            Assert.Equal("11", entry.Value.RawValue);
        }

        [Fact(Skip = "Empty collection should be created by the collection model binder #1579")]
        public async Task CollectionModelBinder_BindsListOfSimpleType_NoData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(List<int>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);  // This fails due to #1579
            Assert.False(modelBindingResult.IsModelSet);
            Assert.Empty(Assert.IsType<List<int>>(modelBindingResult.Model));

            Assert.Equal(0, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);
        }

        private class Person
        {
            public int Id { get; set; }
        }

        [Fact(Skip = "Extra ModelState key because of #2446")]
        public async Task CollectionModelBinder_BindsListOfComplexType_WithPrefix_Success()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(List<Person>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter[0].Id=10&parameter[1].Id=11");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<List<Person>>(modelBindingResult.Model);
            Assert.Equal(10, model[0].Id);
            Assert.Equal(11, model[1].Id);

            Assert.Equal(2, modelState.Count); // This fails due to #2446
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[0].Id").Value;
            Assert.Equal("10", entry.Value.AttemptedValue);
            Assert.Equal("10", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[1].Id").Value;
            Assert.Equal("11", entry.Value.AttemptedValue);
            Assert.Equal("11", entry.Value.RawValue);
        }

        [Fact(Skip = "Extra ModelState key because of #2446")]
        public async Task CollectionModelBinder_BindsListOfComplexType_WithExplicitPrefix_Success()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                BindingInfo = new BindingInfo()
                {
                    BinderModelName = "prefix",
                },
                ParameterType = typeof(List<Person>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?prefix[0].Id=10&prefix[1].Id=11");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<List<Person>>(modelBindingResult.Model);
            Assert.Equal(10, model[0].Id);
            Assert.Equal(11, model[1].Id);

            Assert.Equal(2, modelState.Count); // This fails due to #2446
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[0].Id").Value;
            Assert.Equal("10", entry.Value.AttemptedValue);
            Assert.Equal("10", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[1].Id").Value;
            Assert.Equal("11", entry.Value.AttemptedValue);
            Assert.Equal("11", entry.Value.RawValue);
        }

        [Fact(Skip = "Extra ModelState key because of #2446")]
        public async Task CollectionModelBinder_BindsCollectionOfComplexType_EmptyPrefix_Success()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(ICollection<int>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?[0].Id=10&[1].Id=11");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<List<Person>>(modelBindingResult.Model);
            Assert.Equal(10, model[0].Id);
            Assert.Equal(11, model[1].Id);

            Assert.Equal(2, modelState.Count); // This fails due to #2446
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "[0].Id").Value;
            Assert.Equal("10", entry.Value.AttemptedValue);
            Assert.Equal("10", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "[1].Id").Value;
            Assert.Equal("11", entry.Value.AttemptedValue);
            Assert.Equal("11", entry.Value.RawValue);
        }

        [Fact(Skip = "Empty collection should be created by the collection model binder #1579")]
        public async Task CollectionModelBinder_BindsListOfComplexType_NoData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(List<Person>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);  // This fails due to #1579
            Assert.False(modelBindingResult.IsModelSet);
            Assert.Empty(Assert.IsType<List<Person>>(modelBindingResult.Model));

            Assert.Equal(0, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);
        }

        private class Person2
        {
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }
        }

        [Fact]
        public async Task CollectionModelBinder_BindsListOfComplexType_WithRequiredProperty_WithPrefix_PartialData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(List<Person2>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter[0].Id=10&parameter[1].Id=11");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<List<Person2>>(modelBindingResult.Model);
            Assert.Equal(10, model[0].Id);
            Assert.Equal(11, model[1].Id);
            Assert.Null(model[0].Name);
            Assert.Null(model[1].Name);

            Assert.Equal(4, modelState.Count);
            Assert.Equal(2, modelState.ErrorCount);
            Assert.False(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[0].Id").Value;
            Assert.Equal("10", entry.Value.AttemptedValue);
            Assert.Equal("10", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[1].Id").Value;
            Assert.Equal("11", entry.Value.AttemptedValue);
            Assert.Equal("11", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[0].Name").Value;
            Assert.Null(entry.Value);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);

            entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[1].Name").Value;
            Assert.Null(entry.Value);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);
        }

        [Fact]
        public async Task CollectionModelBinder_BindsListOfComplexType_WithRequiredProperty_WithExplicitPrefix_PartialData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                BindingInfo = new BindingInfo()
                {
                    BinderModelName = "prefix",
                },
                ParameterType = typeof(List<Person2>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?prefix[0].Id=10&prefix[1].Id=11");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<List<Person2>>(modelBindingResult.Model);
            Assert.Equal(10, model[0].Id);
            Assert.Null(model[0].Name);
            Assert.Equal(11, model[1].Id);
            Assert.Null(model[1].Name);

            Assert.Equal(4, modelState.Count);
            Assert.Equal(2, modelState.ErrorCount);
            Assert.False(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[0].Id").Value;
            Assert.Equal("10", entry.Value.AttemptedValue);
            Assert.Equal("10", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[1].Id").Value;
            Assert.Equal("11", entry.Value.AttemptedValue);
            Assert.Equal("11", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[0].Name").Value;
            Assert.Null(entry.Value);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);

            entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[1].Name").Value;
            Assert.Null(entry.Value);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);
        }

        [Fact]
        public async Task CollectionModelBinder_BindsCollectionOfComplexType_WithRequiredProperty_EmptyPrefix_PartialData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(ICollection<Person2>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?[0].Id=10&[1].Id=11");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<List<Person2>>(modelBindingResult.Model);
            Assert.Equal(10, model[0].Id);
            Assert.Null(model[0].Name);
            Assert.Equal(11, model[1].Id);
            Assert.Null(model[1].Name);

            Assert.Equal(4, modelState.Count);
            Assert.Equal(2, modelState.ErrorCount);
            Assert.False(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "[0].Id").Value;
            Assert.Equal("10", entry.Value.AttemptedValue);
            Assert.Equal("10", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "[1].Id").Value;
            Assert.Equal("11", entry.Value.AttemptedValue);
            Assert.Equal("11", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "[0].Name").Value;
            Assert.Null(entry.Value);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);

            entry = Assert.Single(modelState, kvp => kvp.Key == "[1].Name").Value;
            Assert.Null(entry.Value);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);
        }

        [Fact(Skip = "Empty collection should be created by the collection model binder #1579")]
        public async Task CollectionModelBinder_BindsListOfComplexType_WithRequiredProperty_NoData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(List<Person2>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);  // This fails due to #1579
            Assert.False(modelBindingResult.IsModelSet);
            Assert.Empty(Assert.IsType<List<Person2>>(modelBindingResult.Model));

            Assert.Equal(0, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);
        }
    }
}