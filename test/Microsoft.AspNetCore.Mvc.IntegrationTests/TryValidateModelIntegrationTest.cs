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
        [ReplaceCulture("fr-FR", "fr-FR")]
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

            // Act
            var result = controller.TryValidateModel(model);

            // Assert
            Assert.False(result);
            Assert.False(modelState.IsValid);
            var modelStateErrors = GetModelStateErrors(modelState);
            Assert.Contains("CompanyName", modelStateErrors["[0].CompanyName"]);
            AssertBetweenMessage(modelStateErrors["[0].Price"], "Price", 20, 100);
            Assert.Contains("Category", modelStateErrors["[0].Category"]);
            AssertErrorContains(
                new string[] {
                    "Contact Us",
                    "20",
                    "'^[0-9]*$'."
                }, modelStateErrors["[0].Contact"]);
            Assert.Contains("CompanyName", modelStateErrors["[1].CompanyName"]);
            AssertBetweenMessage(modelStateErrors["[1].Price"], "Price", 20, 100);
            Assert.Contains("Category", modelStateErrors["[1].Category"]);
            var expectedRegex = (TestPlatformHelper.IsMono ? "^[0-9]*$." : "'^[0-9]*$'.");
            Assert.Contains("Contact Us", modelStateErrors["[1].Contact"]);
            Assert.Contains("20", modelStateErrors["[1].Contact"]);
            Assert.Contains(expectedRegex, modelStateErrors["[1].Contact"]);
        }

        private void AssertBetweenMessage(string errorMessage, string field, int min, int max)
        {
            Assert.Contains(field, errorMessage);
            Assert.Contains(min.ToString(), errorMessage);
            Assert.Contains(max.ToString(), errorMessage);
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

        private void AssertErrorContains(IEnumerable<string> contains, string error)
        {
            foreach (var contain in contains)
            {
                Assert.Contains(contain, error);
            }
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
