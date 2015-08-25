// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc.ModelBinding;
using Xunit;

namespace Microsoft.AspNet.Mvc.IntegrationTests
{
    // Integration tests targeting the behavior of the CollectionModelBinder with other model binders.
    //
    // Note that CollectionModelBinder handles both ICollection{T} and IList{T}
    public class CollectionModelBinderIntegrationTest
    {
        [Fact]
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

            Assert.Equal(2, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[0]").Value;
            Assert.Equal("10", entry.AttemptedValue);
            Assert.Equal("10", entry.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[1]").Value;
            Assert.Equal("11", entry.AttemptedValue);
            Assert.Equal("11", entry.RawValue);
        }

        [Fact]
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

            Assert.Equal(2, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[0]").Value;
            Assert.Equal("10", entry.AttemptedValue);
            Assert.Equal("10", entry.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[1]").Value;
            Assert.Equal("11", entry.AttemptedValue);
            Assert.Equal("11", entry.RawValue);
        }

        [Fact]
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

            Assert.Equal(2, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "[0]").Value;
            Assert.Equal("10", entry.AttemptedValue);
            Assert.Equal("10", entry.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "[1]").Value;
            Assert.Equal("11", entry.AttemptedValue);
            Assert.Equal("11", entry.RawValue);
        }

        [Fact]
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
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);
            Assert.Empty(Assert.IsType<List<int>>(modelBindingResult.Model));

            Assert.Equal(0, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);
        }

        private class Person
        {
            public int Id { get; set; }
        }

        [Fact]
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

            Assert.Equal(2, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[0].Id").Value;
            Assert.Equal("10", entry.AttemptedValue);
            Assert.Equal("10", entry.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[1].Id").Value;
            Assert.Equal("11", entry.AttemptedValue);
            Assert.Equal("11", entry.RawValue);
        }

        [Fact]
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

            Assert.Equal(2, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[0].Id").Value;
            Assert.Equal("10", entry.AttemptedValue);
            Assert.Equal("10", entry.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[1].Id").Value;
            Assert.Equal("11", entry.AttemptedValue);
            Assert.Equal("11", entry.RawValue);
        }

        [Fact]
        public async Task CollectionModelBinder_BindsCollectionOfComplexType_EmptyPrefix_Success()
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

            Assert.Equal(2, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "[0].Id").Value;
            Assert.Equal("10", entry.AttemptedValue);
            Assert.Equal("10", entry.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "[1].Id").Value;
            Assert.Equal("11", entry.AttemptedValue);
            Assert.Equal("11", entry.RawValue);
        }

        [Fact]
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
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);
            Assert.Empty(Assert.IsType<List<Person>>(modelBindingResult.Model));

            Assert.Equal(0, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);
        }

        private class Person2
        {
            public int Id { get; set; }

            [BindRequired]
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
            Assert.Equal("10", entry.AttemptedValue);
            Assert.Equal("10", entry.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[1].Id").Value;
            Assert.Equal("11", entry.AttemptedValue);
            Assert.Equal("11", entry.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[0].Name").Value;
            Assert.Null(entry.RawValue);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);
            var error = Assert.Single(entry.Errors);
            Assert.Equal("A value for the 'Name' property was not provided.", error.ErrorMessage);

            entry = Assert.Single(modelState, kvp => kvp.Key == "parameter[1].Name").Value;
            Assert.Null(entry.RawValue);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);
            error = Assert.Single(entry.Errors);
            Assert.Equal("A value for the 'Name' property was not provided.", error.ErrorMessage);
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
            Assert.Equal("10", entry.AttemptedValue);
            Assert.Equal("10", entry.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[1].Id").Value;
            Assert.Equal("11", entry.AttemptedValue);
            Assert.Equal("11", entry.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[0].Name").Value;
            Assert.Null(entry.RawValue);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);

            entry = Assert.Single(modelState, kvp => kvp.Key == "prefix[1].Name").Value;
            Assert.Null(entry.RawValue);
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
            Assert.Equal("10", entry.AttemptedValue);
            Assert.Equal("10", entry.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "[1].Id").Value;
            Assert.Equal("11", entry.AttemptedValue);
            Assert.Equal("11", entry.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "[0].Name").Value;
            Assert.Null(entry.RawValue);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);

            entry = Assert.Single(modelState, kvp => kvp.Key == "[1].Name").Value;
            Assert.Null(entry.RawValue);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);
        }

        [Fact]
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
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);
            Assert.Empty(Assert.IsType<List<Person2>>(modelBindingResult.Model));

            Assert.Equal(0, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);
        }

        private class Person4
        {
            public IList<Address4> Addresses { get; set; }
        }

        private class Address4
        {
            public int Zip { get; set; }

            public string Street { get; set; }
        }

        [Fact]
        public async Task CollectionModelBinder_UsesCustomIndexes()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Person4)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                var formCollection = new FormCollection(new Dictionary<string, string[]>()
                {
                    { "Addresses.index", new [] { "Key1", "Key2" } },
                    { "Addresses[Key1].Street", new [] { "Street1" } },
                    { "Addresses[Key2].Street", new [] { "Street2" } },
                });

                request.Form = formCollection;
                request.ContentType = "application/x-www-form-urlencoded";
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);
            Assert.IsType<Person4>(modelBindingResult.Model);

            Assert.Equal(2, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);
            var entry = Assert.Single(modelState, kvp => kvp.Key == "Addresses[Key1].Street").Value;
            Assert.Equal("Street1", entry.AttemptedValue);
            Assert.Equal("Street1", entry.RawValue);

            entry = Assert.Single(modelState, kvp => kvp.Key == "Addresses[Key2].Street").Value;
            Assert.Equal("Street2", entry.AttemptedValue);
            Assert.Equal("Street2", entry.RawValue);
        }

        private class Person5
        {
            public IList<Address5> Addresses { get; set; }
        }

        private class Address5
        {
            public int Zip { get; set; }

            [StringLength(3)]
            public string Street { get; set; }
        }

        [Fact]
        public async Task CollectionModelBinder_UsesCustomIndexes_AddsErrorsWithCorrectKeys()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Person5)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                var formCollection = new FormCollection(new Dictionary<string, string[]>()
                {
                    { "Addresses.index", new [] { "Key1" } },
                    { "Addresses[Key1].Street", new [] { "Street1" } },
                });

                request.Form = formCollection;
                request.ContentType = "application/x-www-form-urlencoded";
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);
            Assert.IsType<Person5>(modelBindingResult.Model);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(1, modelState.ErrorCount);
            Assert.False(modelState.IsValid);

            var entry = Assert.Single(modelState, kvp => kvp.Key == "Addresses[Key1].Street").Value;
            var error = Assert.Single(entry.Errors);
            Assert.Equal("The field Street must be a string with a maximum length of 3.", error.ErrorMessage);
        }

        // parameter type, form content, expected type
        public static TheoryData<Type, IDictionary<string, string[]>, Type> CollectionTypeData
        {
            get
            {
                return new TheoryData<Type, IDictionary<string, string[]>, Type>
                {
                    {
                        typeof(IEnumerable<string>),
                        new Dictionary<string, string[]>
                        {
                            { "[0]", new[] { "hello" } },
                            { "[1]", new[] { "world" } },
                        },
                        typeof(List<string>)
                    },
                    {
                        typeof(ICollection<string>),
                        new Dictionary<string, string[]>
                        {
                            { "index", new[] { "low", "high" } },
                            { "[low]", new[] { "hello" } },
                            { "[high]", new[] { "world" } },
                        },
                        typeof(List<string>)
                    },
                    {
                        typeof(IList<string>),
                        new Dictionary<string, string[]>
                        {
                            { "[0]", new[] { "hello" } },
                            { "[1]", new[] { "world" } },
                        },
                        typeof(List<string>)
                    },
                    {
                        typeof(List<string>),
                        new Dictionary<string, string[]>
                        {
                            { "index", new[] { "low", "high" } },
                            { "[low]", new[] { "hello" } },
                            { "[high]", new[] { "world" } },
                        },
                        typeof(List<string>)
                    },
                    {
                        typeof(ClosedGenericCollection),
                        new Dictionary<string, string[]>
                        {
                            { "[0]", new[] { "hello" } },
                            { "[1]", new[] { "world" } },
                        },
                        typeof(ClosedGenericCollection)
                    },
                    {
                        typeof(ClosedGenericList),
                        new Dictionary<string, string[]>
                        {
                            { "index", new[] { "low", "high" } },
                            { "[low]", new[] { "hello" } },
                            { "[high]", new[] { "world" } },
                        },
                        typeof(ClosedGenericList)
                    },
                    {
                        typeof(ExplicitClosedGenericCollection),
                        new Dictionary<string, string[]>
                        {
                            { "[0]", new[] { "hello" } },
                            { "[1]", new[] { "world" } },
                        },
                        typeof(ExplicitClosedGenericCollection)
                    },
                    {
                        typeof(ExplicitClosedGenericList),
                        new Dictionary<string, string[]>
                        {
                            { "index", new[] { "low", "high" } },
                            { "[low]", new[] { "hello" } },
                            { "[high]", new[] { "world" } },
                        },
                        typeof(ExplicitClosedGenericList)
                    },
                    {
                        typeof(ExplicitCollection<string>),
                        new Dictionary<string, string[]>
                        {
                            { "[0]", new[] { "hello" } },
                            { "[1]", new[] { "world" } },
                        },
                        typeof(ExplicitCollection<string>)
                    },
                    {
                        typeof(ExplicitList<string>),
                        new Dictionary<string, string[]>
                        {
                            { "index", new[] { "low", "high" } },
                            { "[low]", new[] { "hello" } },
                            { "[high]", new[] { "world" } },
                        },
                        typeof(ExplicitList<string>)
                    },
                    {
                        typeof(IEnumerable<string>),
                        new Dictionary<string, string[]>
                        {
                            { string.Empty, new[] { "hello", "world" } },
                        },
                        typeof(List<string>)
                    },
                    {
                        typeof(ICollection<string>),
                        new Dictionary<string, string[]>
                        {
                            { "[]", new[] { "hello", "world" } },
                        },
                        typeof(List<string>)
                    },
                    {
                        typeof(IList<string>),
                        new Dictionary<string, string[]>
                        {
                            { string.Empty, new[] { "hello", "world" } },
                        },
                        typeof(List<string>)
                    },
                    {
                        typeof(List<string>),
                        new Dictionary<string, string[]>
                        {
                            { "[]", new[] { "hello", "world" } },
                        },
                        typeof(List<string>)
                    },
                    {
                        typeof(ClosedGenericCollection),
                        new Dictionary<string, string[]>
                        {
                            { string.Empty, new[] { "hello", "world" } },
                        },
                        typeof(ClosedGenericCollection)
                    },
                    {
                        typeof(ClosedGenericList),
                        new Dictionary<string, string[]>
                        {
                            { "[]", new[] { "hello", "world" } },
                        },
                        typeof(ClosedGenericList)
                    },
                    {
                        typeof(ExplicitClosedGenericCollection),
                        new Dictionary<string, string[]>
                        {
                            { string.Empty, new[] { "hello", "world" } },
                        },
                        typeof(ExplicitClosedGenericCollection)
                    },
                    {
                        typeof(ExplicitClosedGenericList),
                        new Dictionary<string, string[]>
                        {
                            { "[]", new[] { "hello", "world" } },
                        },
                        typeof(ExplicitClosedGenericList)
                    },
                    {
                        typeof(ExplicitCollection<string>),
                        new Dictionary<string, string[]>
                        {
                            { string.Empty, new[] { "hello", "world" } },
                        },
                        typeof(ExplicitCollection<string>)
                    },
                    {
                        typeof(ExplicitList<string>),
                        new Dictionary<string, string[]>
                        {
                            { "[]", new[] { "hello", "world" } },
                        },
                        typeof(ExplicitList<string>)
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(CollectionTypeData))]
        public async Task CollectionModelBinder_BindsParameterToExpectedType(
            Type parameterType,
            IDictionary<string, string[]> formContent,
            Type expectedType)
        {
            // Arrange
            var expectedCollection = new List<string> { "hello", "world" };
            var parameter = new ParameterDescriptor
            {
                Name = "parameter",
                ParameterType = parameterType,
            };

            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var modelState = new ModelStateDictionary();
            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.Form = new FormCollection(formContent);
            });

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            Assert.IsType(expectedType, modelBindingResult.Model);

            var model = modelBindingResult.Model as IEnumerable<string>;
            Assert.NotNull(model); // Guard
            Assert.Equal(expectedCollection, model);

            Assert.True(modelState.IsValid);
            Assert.NotEmpty(modelState);
            Assert.Equal(0, modelState.ErrorCount);
        }

        private class ClosedGenericCollection : Collection<string>
        {
        }

        private class ClosedGenericList : List<string>
        {
        }

        private class ExplicitClosedGenericCollection : ICollection<string>
        {
            private List<string> _data = new List<string>();

            int ICollection<string>.Count
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            bool ICollection<string>.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            void ICollection<string>.Add(string item)
            {
                _data.Add(item);
            }

            void ICollection<string>.Clear()
            {
                _data.Clear();
            }

            bool ICollection<string>.Contains(string item)
            {
                throw new NotImplementedException();
            }

            void ICollection<string>.CopyTo(string[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_data).GetEnumerator();
            }

            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                return _data.GetEnumerator();
            }

            bool ICollection<string>.Remove(string item)
            {
                throw new NotImplementedException();
            }
        }

        private class ExplicitClosedGenericList : IList<string>
        {
            private List<string> _data = new List<string>();

            string IList<string>.this[int index]
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            int ICollection<string>.Count
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            bool ICollection<string>.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            void ICollection<string>.Add(string item)
            {
                _data.Add(item);
            }

            void ICollection<string>.Clear()
            {
                _data.Clear();
            }

            bool ICollection<string>.Contains(string item)
            {
                throw new NotImplementedException();
            }

            void ICollection<string>.CopyTo(string[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_data).GetEnumerator();
            }

            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                return _data.GetEnumerator();
            }

            int IList<string>.IndexOf(string item)
            {
                throw new NotImplementedException();
            }

            void IList<string>.Insert(int index, string item)
            {
                throw new NotImplementedException();
            }

            bool ICollection<string>.Remove(string item)
            {
                throw new NotImplementedException();
            }

            void IList<string>.RemoveAt(int index)
            {
                throw new NotImplementedException();
            }
        }

        private class ExplicitCollection<T> : ICollection<T>
        {
            private List<T> _data = new List<T>();

            int ICollection<T>.Count
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            bool ICollection<T>.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            void ICollection<T>.Add(T item)
            {
                _data.Add(item);
            }

            void ICollection<T>.Clear()
            {
                _data.Clear();
            }

            bool ICollection<T>.Contains(T item)
            {
                throw new NotImplementedException();
            }

            void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_data).GetEnumerator();
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return _data.GetEnumerator();
            }

            bool ICollection<T>.Remove(T item)
            {
                throw new NotImplementedException();
            }
        }

        private class ExplicitList<T> : IList<T>
        {
            private List<T> _data = new List<T>();

            T IList<T>.this[int index]
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            int ICollection<T>.Count
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            bool ICollection<T>.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            void ICollection<T>.Add(T item)
            {
                _data.Add(item);
            }

            void ICollection<T>.Clear()
            {
                _data.Clear();
            }

            bool ICollection<T>.Contains(T item)
            {
                throw new NotImplementedException();
            }

            void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_data).GetEnumerator();
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return _data.GetEnumerator();
            }

            int IList<T>.IndexOf(T item)
            {
                throw new NotImplementedException();
            }

            void IList<T>.Insert(int index, T item)
            {
                throw new NotImplementedException();
            }

            bool ICollection<T>.Remove(T item)
            {
                throw new NotImplementedException();
            }

            void IList<T>.RemoveAt(int index)
            {
                throw new NotImplementedException();
            }
        }
    }
}