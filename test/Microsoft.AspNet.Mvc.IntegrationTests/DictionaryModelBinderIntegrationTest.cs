// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding;
using Xunit;

namespace Microsoft.AspNet.Mvc.IntegrationTests
{
    // Integration tests targeting the behavior of the DictionaryModelBinder with other model binders.
    public class DictionaryModelBinderIntegrationTest
    {
        [Fact(Skip = "Extra ModelState key because of #2446")]
        public async Task DictionaryModelBinder_BindsDictionaryOfSimpleType_WithPrefix_Success()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Dictionary<string, int>)
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString("?parameter[key0]=0&parameter[key1]=1");

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(httpContext);
            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Dictionary<string, int>>(modelBindingResult.Model);
            Assert.Equal(new Dictionary<string, int>() { { "key0", 0 }, { "key1", 1 } }, model);

            Assert.Equal(2, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[key0]").Value;
            Assert.Equal("0", entry.Value.AttemptedValue);
            Assert.Equal("0", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[key1]").Value;
            Assert.Equal("1", entry.Value.AttemptedValue);
            Assert.Equal("1", entry.Value.RawValue);
        }

        [Fact(Skip = "Extra ModelState key because of #2446")]
        public async Task DictionaryModelBinder_BindsDictionaryOfSimpleType_WithExplicitPrefix_Success()
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
                ParameterType = typeof(Dictionary<string, int>)
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString("?prefix[key0]=0&prefix[key1]=1");

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(httpContext);
            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Dictionary<string, int>>(modelBindingResult.Model);
            Assert.Equal(new Dictionary<string, int>() { { "key0", 0 }, { "key1", 1 } }, model);

            Assert.Equal(2, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[key0]").Value;
            Assert.Equal("0", entry.Value.AttemptedValue);
            Assert.Equal("0", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[key1]").Value;
            Assert.Equal("1", entry.Value.AttemptedValue);
            Assert.Equal("1", entry.Value.RawValue);
        }

        [Fact(Skip = "Extra ModelState key because of #2446")]
        public async Task DictionaryModelBinder_BindsDictionaryOfSimpleType_EmptyPrefix_Success()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Dictionary<string, int>)
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString("?[0]=0&[1]=1");

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(httpContext);
            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Dictionary<string, int>>(modelBindingResult.Model);
            Assert.Equal(new Dictionary<string, int>() { { "key0", 0 }, { "key1", 1 } }, model);

            Assert.Equal(2, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "[key0]").Value;
            Assert.Equal("0", entry.Value.AttemptedValue);
            Assert.Equal("0", entry.Value.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "[key1]").Value;
            Assert.Equal("1", entry.Value.AttemptedValue);
            Assert.Equal("1", entry.Value.RawValue);
        }

        [Fact(Skip = "Empty collection should be created by the collection model binder #1579")]
        public async Task DictionaryModelBinder_BindsDictionaryOfSimpleType_NoData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Dictionary<string, int>)
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.QueryString = new QueryString("?");

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(httpContext);
            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.Null(modelBindingResult);
            Assert.False(modelBindingResult.IsModelSet);

            Assert.Equal(new int[0], modelBindingResult.Model);

            Assert.Equal(0, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);
        }
    }
}