// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.IntegrationTests
{
    public class TryValidateModelIntegrationTest
    {
        [Fact]
        public void ModelState_IsInvalid_ForInvalidData_OnDerivedModel()
        {
            // Arrange
            var testContext = ModelBindingTestHelper.GetTestContext();

            var modelState = testContext.ModelState;
            var model = new SoftwareViewModel
            {
                Category = "Technology",
                CompanyName = "Microsoft",
                Contact = "4258393231",
                Country = "UK", // Here the validate country is USA only
                DatePurchased = new DateTime(2010, 10, 10),
                Price = 110,
                Version = "2"
            };

            var controller = CreateController(testContext, testContext.MetadataProvider);

            // Act
            var result = controller.TryValidateModel(model, prefix: "software");

            // Assert
            Assert.False(result);
            Assert.False(modelState.IsValid);
            var modelStateErrors = GetModelStateErrors(modelState);
            Assert.Single(modelStateErrors);
            Assert.Equal("Product must be made in the USA if it is not named.", modelStateErrors["software"]);
        }

        [Fact]
        public void ModelState_IsValid_ForValidData_OnDerivedModel()
        {
            // Arrange
            var testContext = ModelBindingTestHelper.GetTestContext();
            var modelState = testContext.ModelState;
            var model = new SoftwareViewModel
            {
                Category = "Technology",
                CompanyName = "Microsoft",
                Contact = "4258393231",
                Country = "USA",
                DatePurchased = new DateTime(2010, 10, 10),
                Name = "MVC",
                Price = 110,
                Version = "2"
            };

            var controller = CreateController(testContext, testContext.MetadataProvider);

            // Act
            var result = controller.TryValidateModel(model);

            // Assert
            Assert.True(result);
            Assert.True(modelState.IsValid);
            var modelStateErrors = GetModelStateErrors(modelState);
            Assert.Empty(modelStateErrors);
        }

        [Fact]
        [ReplaceCulture]
        public void TryValidateModel_CollectionsModel_ReturnsErrorsForInvalidProperties()
        {
            // Arrange
            var testContext = ModelBindingTestHelper.GetTestContext();
            var modelState = testContext.ModelState;
            var model = new List<ProductViewModel>();
            model.Add(new ProductViewModel()
            {
                Price = 2,
                Contact = "acvrdzersaererererfdsfdsfdsfsdf",
                ProductDetails = new ProductDetails()
                {
                    Detail1 = "d1",
                    Detail2 = "d2",
                    Detail3 = "d3"
                }
            });
            model.Add(new ProductViewModel()
            {
                Price = 2,
                Contact = "acvrdzersaererererfdsfdsfdsfsdf",
                ProductDetails = new ProductDetails()
                {
                    Detail1 = "d1",
                    Detail2 = "d2",
                    Detail3 = "d3"
                }
            });

            var controller = CreateController(testContext, testContext.MetadataProvider);

            // We define the "CompanyName null" message locally, so we should manually check its value.
            var categoryRequired = ValidationAttributeUtil.GetRequiredErrorMessage("Category");
            var priceRange = ValidationAttributeUtil.GetRangeErrorMessage(20, 100, "Price");
            var contactUsMax = ValidationAttributeUtil.GetStringLengthErrorMessage(null, 20, "Contact Us");
            var contactusRegEx = ValidationAttributeUtil.GetRegExErrorMessage("^[0-9]*$", "Contact Us");

            // Act
            var result = controller.TryValidateModel(model);

            // Assert
            Assert.False(result);
            Assert.False(modelState.IsValid);
            var modelStateErrors = GetModelStateErrors(modelState);

            Assert.Equal("CompanyName cannot be null or empty.", modelStateErrors["[0].CompanyName"]);
            Assert.Equal(priceRange, modelStateErrors["[0].Price"]);
            Assert.Equal(categoryRequired, modelStateErrors["[0].Category"]);
            AssertErrorEquals(contactUsMax + contactusRegEx, modelStateErrors["[0].Contact"]);
            Assert.Equal("CompanyName cannot be null or empty.", modelStateErrors["[1].CompanyName"]);
            Assert.Equal(priceRange, modelStateErrors["[1].Price"]);
            Assert.Equal(categoryRequired, modelStateErrors["[1].Category"]);
            AssertErrorEquals(contactUsMax + contactusRegEx, modelStateErrors["[1].Contact"]);
        }

        private void AssertErrorEquals(string expected, string actual)
        {
            // OrderBy is used because the order of the results may very depending on the platform / client.
            Assert.Equal(
                expected.Split('.').OrderBy(item => item, StringComparer.Ordinal),
                actual.Split('.').OrderBy(item => item, StringComparer.Ordinal));
        }

        private TestController CreateController(
            ActionContext actionContext,
            IModelMetadataProvider metadataProvider)
        {
            var options = actionContext.HttpContext.RequestServices.GetRequiredService<IOptions<MvcOptions>>();

            var controller = new TestController();
            controller.ControllerContext = new ControllerContext(actionContext);
            controller.ObjectValidator = ModelBindingTestHelper.GetObjectValidator(metadataProvider, options);
            controller.MetadataProvider = metadataProvider;

            return controller;
        }

        private Dictionary<string, string> GetModelStateErrors(ModelStateDictionary modelState)
        {
            var result = new Dictionary<string, string>();
            foreach (var item in modelState)
            {
                var errorMessage = string.Empty;
                foreach (var error in item.Value.Errors)
                {
                    if (error != null)
                    {
                        errorMessage = errorMessage + error.ErrorMessage;
                    }
                }
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    result.Add(item.Key, errorMessage);
                }
            }

            return result;
        }

        private class TestController : Controller
        {
        }
    }
}
