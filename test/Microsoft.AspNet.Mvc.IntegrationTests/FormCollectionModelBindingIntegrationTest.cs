// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features.Internal;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.Framework.Primitives;
using Xunit;

namespace Microsoft.AspNet.Mvc.IntegrationTests
{
    public class FormCollectionModelBindingIntegrationTest
    {
        private class Person
        {
            public Address Address { get; set; }
        }

        private class Address
        {
            public int Zip { get; set; }

            public IFormCollection FileCollection { get; set; }
        }

        [Fact]
        public async Task BindProperty_WithData_WithEmptyPrefix_GetsBound()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor
            {
                Name = "Parameter1",
                BindingInfo = new BindingInfo(),
                ParameterType = typeof(Person)
            };

            var data = "Some Data Is Better Than No Data.";
            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(
                request =>
                {
                    request.QueryString = QueryString.Create("Address.Zip", "12345");
                    UpdateRequest(request, data, "Address.File");
                });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert

            // ModelBindingResult
            Assert.True(modelBindingResult.IsModelSet);

            // Model
            var boundPerson = Assert.IsType<Person>(modelBindingResult.Model);
            Assert.NotNull(boundPerson.Address);
            var formCollection = Assert.IsAssignableFrom<IFormCollection>(boundPerson.Address.FileCollection);
            var file = Assert.Single(formCollection.Files);
            Assert.Equal("form-data; name=Address.File; filename=text.txt", file.ContentDisposition);
            var reader = new StreamReader(file.OpenReadStream());
            Assert.Equal(data, reader.ReadToEnd());

            // ModelState
            Assert.True(modelState.IsValid);
            var entry = Assert.Single(modelState);
            Assert.Equal("Address.Zip", entry.Key);
            Assert.Empty(entry.Value.Errors);
            Assert.Equal(ModelValidationState.Valid, entry.Value.ValidationState);
        }

        [Fact]
        public async Task BindParameter_WithData_GetsBound()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor
            {
                Name = "Parameter1",
                BindingInfo = new BindingInfo
                {
                    // Setting a custom parameter prevents it from falling back to an empty prefix.
                    BinderModelName = "CustomParameter",
                },
                ParameterType = typeof(IFormCollection)
            };

            var data = "Some Data Is Better Than No Data.";
            var operationContext = ModelBindingTestHelper.GetOperationBindingContext(
                request =>
                {
                    UpdateRequest(request, data, "CustomParameter");
                });

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert
            // ModelBindingResult
            Assert.True(modelBindingResult.IsModelSet);

            // Model
            var formCollection = Assert.IsAssignableFrom<IFormCollection>(modelBindingResult.Model);
            var file = Assert.Single(formCollection.Files);
            Assert.NotNull(file);
            Assert.Equal("form-data; name=CustomParameter; filename=text.txt", file.ContentDisposition);
            var reader = new StreamReader(file.OpenReadStream());
            Assert.Equal(data, reader.ReadToEnd());

            // ModelState
            Assert.True(modelState.IsValid);
            Assert.Empty(modelState);
        }

        [Fact]
        public async Task BindParameter_NoData_BindsWithEmptyCollection()
        {
            // Arrange
            var argumentBinder = ModelBindingTestHelper.GetArgumentBinder();
            var parameter = new ParameterDescriptor
            {
                Name = "Parameter1",
                BindingInfo = new BindingInfo
                {
                    BinderModelName = "CustomParameter",
                },
                ParameterType = typeof(IFormCollection)
            };

            // No data is passed.
            var operationContext = ModelBindingTestHelper.GetOperationBindingContext();

            var modelState = new ModelStateDictionary();

            // Act
            var modelBindingResult = await argumentBinder.BindModelAsync(parameter, modelState, operationContext);

            // Assert

            // ModelBindingResult
            var collection = Assert.IsAssignableFrom<IFormCollection>(modelBindingResult.Model);

            // ModelState
            Assert.True(modelState.IsValid);
            Assert.Empty(modelState);

            // FormCollection
            Assert.Empty(collection);
            Assert.Empty(collection.Files);
            Assert.Empty(collection.Keys);
        }

        private void UpdateRequest(HttpRequest request, string data, string name)
        {
            var fileCollection = new FormFileCollection();
            var formCollection = new FormCollection(new Dictionary<string, StringValues>(), fileCollection);
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(data));

            request.Form = formCollection;
            request.ContentType = "multipart/form-data; boundary=----WebKitFormBoundarymx2fSWqWSd0OxQqq";
            request.Headers["Content-Disposition"] = "form-data; name=" + name + "; filename=text.txt";
            fileCollection.Add(new FormFile(memoryStream, 0, data.Length)
            {
                Headers = request.Headers
            });
        }
    }
}