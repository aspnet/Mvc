﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.ModelBinding;
using Xunit;

namespace Microsoft.AspNet.Mvc.IntegrationTests
{
    public class ValidationIntegrationTests
    {
        private class Order1
        {
            [Required]
            public string CustomerName { get; set; }
        }

        [Fact]
        public async Task Validation_RequiredAttribute_OnSimpleTypeProperty_WithData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order1)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter.CustomerName=bill");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Order1>(modelBindingResult.Model);
            Assert.Equal("bill", model.CustomerName);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter.CustomerName").Value;
            Assert.Equal("bill", entry.Value.AttemptedValue);
            Assert.Equal("bill", entry.Value.RawValue);
            Assert.Empty(entry.Errors);
        }

        [Fact]
        public async Task Validation_RequiredAttribute_OnSimpleTypeProperty_NoData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order1)
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

            var model = Assert.IsType<Order1>(modelBindingResult.Model);
            Assert.Null(model.CustomerName);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(1, modelState.ErrorCount);
            Assert.False(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "CustomerName").Value;
            Assert.Null(entry.Value);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);

            var error = Assert.Single(entry.Errors);
            AssertRequiredError("CustomerName", error);
        }

        private class Order2
        {
            [Required]
            public Person2 Customer { get; set; }
        }

        private class Person2
        {
            public string Name { get; set; }
        }

        [Fact]
        public async Task Validation_RequiredAttribute_OnPOCOProperty_WithData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order2)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter.Customer.Name=bill");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Order2>(modelBindingResult.Model);
            Assert.NotNull(model.Customer);
            Assert.Equal("bill", model.Customer.Name);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter.Customer.Name").Value;
            Assert.Equal("bill", entry.Value.AttemptedValue);
            Assert.Equal("bill", entry.Value.RawValue);
            Assert.Empty(entry.Errors);
        }

        [Fact]
        public async Task Validation_RequiredAttribute_OnPOCOProperty_NoData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order2)
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

            var model = Assert.IsType<Order2>(modelBindingResult.Model);
            Assert.Null(model.Customer);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(1, modelState.ErrorCount);
            Assert.False(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "Customer").Value;
            Assert.Null(entry.Value);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);

            var error = Assert.Single(entry.Errors);
            AssertRequiredError("Customer", error);
        }

        private class Order3
        {
            public Person3 Customer { get; set; }
        }

        private class Person3
        {
            public int Age { get; set; }

            [Required]
            public string Name { get; set; }
        }

        [Fact]
        public async Task Validation_RequiredAttribute_OnNestedSimpleTypeProperty_WithData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order3)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter.Customer.Name=bill");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Order3>(modelBindingResult.Model);
            Assert.NotNull(model.Customer);
            Assert.Equal("bill", model.Customer.Name);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter.Customer.Name").Value;
            Assert.Equal("bill", entry.Value.AttemptedValue);
            Assert.Equal("bill", entry.Value.RawValue);
            Assert.Empty(entry.Errors);
        }

        [Fact]
        public async Task Validation_RequiredAttribute_OnNestedSimpleTypeProperty_NoDataForRequiredProperty()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order3)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                // Force creation of the Customer model.
                request.QueryString = new QueryString("?parameter.Customer.Age=17");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Order3>(modelBindingResult.Model);
            Assert.NotNull(model.Customer);
            Assert.Equal(17, model.Customer.Age);
            Assert.Null(model.Customer.Name);

            Assert.Equal(2, modelState.Count);
            Assert.Equal(1, modelState.ErrorCount);
            Assert.False(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter.Customer.Name").Value;
            Assert.Null(entry.Value);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);

            var error = Assert.Single(entry.Errors);
            AssertRequiredError("Name", error);
        }

        private class Order4
        {
            [Required]
            public List<Item4> Items { get; set; }
        }

        private class Item4
        {
            public int ItemId { get; set; }
        }

        [Fact]
        public async Task Validation_RequiredAttribute_OnCollectionProperty_WithData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order4)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?Items[0].ItemId=17");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Order4>(modelBindingResult.Model);
            Assert.NotNull(model.Items);
            Assert.Equal(17, Assert.Single(model.Items).ItemId);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "Items[0].ItemId").Value;
            Assert.Equal("17", entry.Value.AttemptedValue);
            Assert.Equal("17", entry.Value.RawValue);
            Assert.Empty(entry.Errors);
        }

        [Fact]
        public async Task Validation_RequiredAttribute_OnCollectionProperty_NoData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order4)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                // Force creation of the Customer model.
                request.QueryString = new QueryString("?");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Order4>(modelBindingResult.Model);
            Assert.Null(model.Items);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(1, modelState.ErrorCount);
            Assert.False(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "Items").Value;
            Assert.Null(entry.Value);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);

            var error = Assert.Single(entry.Errors);
            AssertRequiredError("Items", error);
        }

        private class Order5
        {
            [Required]
            public int? ProductId { get; set; }

            public string Name { get; set; }
        }

        [Fact]
        public async Task Validation_RequiredAttribute_OnPOCOPropertyOfBoundElement_WithData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(List<Order5>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter[0].ProductId=17");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<List<Order5>>(modelBindingResult.Model);
            Assert.Equal(17, Assert.Single(model).ProductId);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter[0].ProductId").Value;
            Assert.Equal("17", entry.Value.AttemptedValue);
            Assert.Equal("17", entry.Value.RawValue);
            Assert.Empty(entry.Errors);
        }

        [Fact]
        public async Task Validation_RequiredAttribute_OnPOCOPropertyOfBoundElement_NoDataForRequiredProperty()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(List<Order5>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                // Force creation of the Customer model.
                request.QueryString = new QueryString("?parameter[0].Name=bill");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<List<Order5>>(modelBindingResult.Model);
            var item = Assert.Single(model);
            Assert.Null(item.ProductId);
            Assert.Equal("bill", item.Name);

            Assert.Equal(2, modelState.Count);
            Assert.Equal(1, modelState.ErrorCount);
            Assert.False(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter[0].ProductId").Value;
            Assert.Null(entry.Value);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);

            var error = Assert.Single(entry.Errors);
            AssertRequiredError("ProductId", error);
        }

        private class Order6
        {
            [StringLength(5, ErrorMessage = "Too Long.")]
            public string Name { get; set; }
        }

        [Fact]
        public async Task Validation_StringLengthAttribute_OnPropertyOfPOCO_Valid()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order6)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter.Name=bill");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Order6>(modelBindingResult.Model);
            Assert.Equal("bill", model.Name);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter.Name").Value;
            Assert.Equal("bill", entry.Value.AttemptedValue);
            Assert.Equal("bill", entry.Value.RawValue);
            Assert.Empty(entry.Errors);
        }

        [Fact]
        public async Task Validation_StringLengthAttribute_OnPropertyOfPOCO_Invalid()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order6)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter.Name=billybob");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Order6>(modelBindingResult.Model);
            Assert.Equal("billybob", model.Name);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(1, modelState.ErrorCount);
            Assert.False(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter.Name").Value;
            Assert.Equal("billybob", entry.Value.AttemptedValue);
            Assert.Equal("billybob", entry.Value.RawValue);

            var error = Assert.Single(entry.Errors);
            Assert.Equal("Too Long.", error.ErrorMessage);
            Assert.Null(error.Exception);
        }

        private class Order7
        {
            public Person7 Customer { get; set; }
        }

        private class Person7
        {
            [StringLength(5, ErrorMessage = "Too Long.")]
            public string Name { get; set; }
        }

        [Fact]
        public async Task Validation_StringLengthAttribute_OnPropertyOfNestedPOCO_Valid()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order7)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter.Customer.Name=bill");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Order7>(modelBindingResult.Model);
            Assert.Equal("bill", model.Customer.Name);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter.Customer.Name").Value;
            Assert.Equal("bill", entry.Value.AttemptedValue);
            Assert.Equal("bill", entry.Value.RawValue);
            Assert.Empty(entry.Errors);
        }

        [Fact]
        public async Task Validation_StringLengthAttribute_OnPropertyOfNestedPOCO_Invalid()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order7)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter.Customer.Name=billybob");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Order7>(modelBindingResult.Model);
            Assert.Equal("billybob", model.Customer.Name);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(1, modelState.ErrorCount);
            Assert.False(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter.Customer.Name").Value;
            Assert.Equal("billybob", entry.Value.AttemptedValue);
            Assert.Equal("billybob", entry.Value.RawValue);

            var error = Assert.Single(entry.Errors);
            Assert.Equal("Too Long.", error.ErrorMessage);
            Assert.Null(error.Exception);
        }

        [Fact]
        public async Task Validation_StringLengthAttribute_OnPropertyOfNestedPOCO_NoData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order7)
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

            var model = Assert.IsType<Order7>(modelBindingResult.Model);
            Assert.Null(model.Customer);

            Assert.Equal(0, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);
        }

        private class Order8
        {
            [ValidatePerson8]
            public Person8 Customer { get; set; }
        }

        private class Person8
        {
            public string Name { get; set; }
        }

        private class ValidatePerson8Attribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (((Person8)value).Name == "bill")
                {
                    return null;
                }
                else
                {
                    return new ValidationResult("Invalid Person.");
                }
            }
        }

        [Fact]
        public async Task Validation_CustomAttribute_OnPOCOProperty_Valid()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order8)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter.Customer.Name=bill");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Order8>(modelBindingResult.Model);
            Assert.Equal("bill", model.Customer.Name);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter.Customer.Name").Value;
            Assert.Equal("bill", entry.Value.AttemptedValue);
            Assert.Equal("bill", entry.Value.RawValue);
            Assert.Empty(entry.Errors);
        }

        [Fact]
        public async Task Validation_CustomAttribute_OnPOCOProperty_Invalid()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order8)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter.Customer.Name=billybob");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Order8>(modelBindingResult.Model);
            Assert.Equal("billybob", model.Customer.Name);

            Assert.Equal(2, modelState.Count);
            Assert.Equal(1, modelState.ErrorCount);
            Assert.False(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter.Customer.Name").Value;
            Assert.Equal("billybob", entry.Value.AttemptedValue);
            Assert.Equal("billybob", entry.Value.RawValue);

            entry = Assert.Single(modelState, e => e.Key == "parameter.Customer").Value;
            Assert.Null(entry.Value);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);
            var error = Assert.Single(entry.Errors);
            Assert.Equal("Invalid Person.", error.ErrorMessage);
            Assert.Null(error.Exception);
        }

        private class Order9
        {
            [ValidateProducts9]
            public List<Product9> Products { get; set; }
        }

        private class Product9
        {
            public string Name { get; set; }
        }

        private class ValidateProducts9Attribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (((List<Product9>)value)[0].Name == "bill")
                {
                    return null;
                }
                else
                {
                    return new ValidationResult("Invalid Product.");
                }
            }
        }

        [Fact]
        public async Task Validation_CustomAttribute_OnCollectionElement_Valid()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order9)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter.Products[0].Name=bill");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Order9>(modelBindingResult.Model);
            Assert.Equal("bill", Assert.Single(model.Products).Name);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter.Products[0].Name").Value;
            Assert.Equal("bill", entry.Value.AttemptedValue);
            Assert.Equal("bill", entry.Value.RawValue);
            Assert.Empty(entry.Errors);
        }

        [Fact]
        public async Task Validation_CustomAttribute_OnCollectionElement_Invalid()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(Order9)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter.Products[0].Name=billybob");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<Order9>(modelBindingResult.Model);
            Assert.Equal("billybob", Assert.Single(model.Products).Name);

            Assert.Equal(2, modelState.Count);
            Assert.Equal(1, modelState.ErrorCount);
            Assert.False(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter.Products[0].Name").Value;
            Assert.Equal("billybob", entry.Value.AttemptedValue);
            Assert.Equal("billybob", entry.Value.RawValue);

            entry = Assert.Single(modelState, e => e.Key == "parameter.Products").Value;
            Assert.Null(entry.Value);
            Assert.Equal(ModelValidationState.Invalid, entry.ValidationState);

            var error = Assert.Single(entry.Errors);
            Assert.Equal("Invalid Product.", error.ErrorMessage);
            Assert.Null(error.Exception);
        }

        private class Order10
        {
            [StringLength(5, ErrorMessage = "Too Long.")]
            public string Name { get; set; }
        }

        [Fact]
        public async Task Validation_StringLengthAttribute_OnProperyOfCollectionElement_Valid()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(List<Order10>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter[0].Name=bill");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<List<Order10>>(modelBindingResult.Model);
            Assert.Equal("bill", Assert.Single(model).Name);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter[0].Name").Value;
            Assert.Equal("bill", entry.Value.AttemptedValue);
            Assert.Equal("bill", entry.Value.RawValue);
            Assert.Empty(entry.Errors);
        }

        [Fact]
        public async Task Validation_StringLengthAttribute_OnProperyOfCollectionElement_Invalid()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(List<Order10>)
            };

            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(request =>
            {
                request.QueryString = new QueryString("?parameter[0].Name=billybob");
            });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            Assert.NotNull(modelBindingResult);
            Assert.True(modelBindingResult.IsModelSet);

            var model = Assert.IsType<List<Order10>>(modelBindingResult.Model);
            Assert.Equal("billybob", Assert.Single(model).Name);

            Assert.Equal(1, modelState.Count);
            Assert.Equal(1, modelState.ErrorCount);
            Assert.False(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "parameter[0].Name").Value;
            Assert.Equal("billybob", entry.Value.AttemptedValue);
            Assert.Equal("billybob", entry.Value.RawValue);

            var error = Assert.Single(entry.Errors);
            Assert.Equal("Too Long.", error.ErrorMessage);
            Assert.Null(error.Exception);
        }

        [Fact]
        public async Task Validation_StringLengthAttribute_OnProperyOfCollectionElement_NoData()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor()
            {
                Name = "parameter",
                ParameterType = typeof(List<Order10>)
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

            var model = Assert.IsType<List<Order10>>(modelBindingResult.Model);
            Assert.Empty(model);

            Assert.Equal(0, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);
        }

        private class Order11
        {
            public IEnumerable<Address> ShippingAddresses { get; set; }

            public Address HomeAddress { get; set; }

            [FromBody]
            public Address OfficeAddress { get; set; }
        }

        private class Address
        {
            public int Street { get; set; }

            public string State { get; set; }

            [Range(10000, 99999)]
            public int Zip { get; set; }

            public Country Country { get; set; }
        }

        private class Country
        {
            public string Name { get; set; }
        }

        [Fact]
        public async Task TypeBasedExclusion_ForBodyAndNonBodyBoundModels()
        {
            // Arrange
            var parameter = new ParameterDescriptor
            {
                Name = "parameter",
                ParameterType = typeof(Order11)
            };

            MvcOptions testOptions = null;
            var input = "{\"Zip\":\"47\"}";
            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(
                request =>
                {
                    request.QueryString =
                        new QueryString("?HomeAddress.Country.Name=US&ShippingAddresses[0].Zip=45&HomeAddress.Zip=46");
                    request.Body = new MemoryStream(Encoding.UTF8.GetBytes(input));
                    request.ContentType = "application/json";
                },
                options =>
                {
                    options.ValidationExcludeFilters.Add(typeof(Address));
                    testOptions = options;
                });

            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder(testOptions);
            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            Assert.Equal(4, modelState.Count);
            Assert.Equal(0, modelState.ErrorCount);
            Assert.True(modelState.IsValid);

            var entry = Assert.Single(modelState, e => e.Key == "HomeAddress.Country.Name").Value;
            Assert.Equal("US", entry.Value.AttemptedValue);
            Assert.Equal("US", entry.Value.RawValue);
            Assert.Equal(ModelValidationState.Skipped, entry.ValidationState);

            entry = Assert.Single(modelState, e => e.Key == "ShippingAddresses[0].Zip").Value;
            Assert.Equal("45", entry.Value.AttemptedValue);
            Assert.Equal("45", entry.Value.RawValue);
            Assert.Equal(ModelValidationState.Skipped, entry.ValidationState);

            entry = Assert.Single(modelState, e => e.Key == "HomeAddress.Zip").Value;
            Assert.Equal("46", entry.Value.AttemptedValue);
            Assert.Equal("46", entry.Value.RawValue);
            Assert.Equal(ModelValidationState.Skipped, entry.ValidationState);

            entry = Assert.Single(modelState, e => e.Key == "OfficeAddress").Value;
            Assert.Null(entry.Value.AttemptedValue);
            var address = Assert.IsType<Address>(entry.Value.RawValue);
            Assert.Equal(47, address.Zip);

            // Address itself is not excluded from validation.
            Assert.Equal(ModelValidationState.Valid, entry.ValidationState);
        }

        private static void AssertRequiredError(string key, ModelError error)
        {
            // Mono issue - https://github.com/aspnet/External/issues/19
            Assert.Equal(PlatformNormalizer.NormalizeContent(
                string.Format("The {0} field is required.", key)), error.ErrorMessage);
            Assert.Null(error.Exception);
        }
    }
}
