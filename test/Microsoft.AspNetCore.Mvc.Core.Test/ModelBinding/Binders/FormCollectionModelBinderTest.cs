// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class FormCollectionModelBinderTest
    {
        [Fact]
        public async Task FormCollectionModelBinder_ValidType_BindSuccessful()
        {
            // Arrange
            var formCollection = new FormCollection(new Dictionary<string, StringValues>
            {
                { "field1", "value1" },
                { "field2", "value2" }
            });
            var httpContext = GetMockHttpContext(formCollection);
            var bindingContext = GetBindingContext(typeof(IFormCollection), httpContext);
            var binder = new FormCollectionModelBinder();

            // Act
            var result = await binder.BindModelResultAsync(bindingContext);

            // Assert
            Assert.NotEqual(default(ModelBindingResult), result);
            Assert.True(result.IsModelSet);

            Assert.Empty(bindingContext.ValidationState);

            var form = Assert.IsAssignableFrom<IFormCollection>(result.Model);
            Assert.Equal(2, form.Count);
            Assert.Equal("value1", form["field1"]);
            Assert.Equal("value2", form["field2"]);
        }

        [Fact]
        public async Task FormCollectionModelBinder_NoForm_BindSuccessful_ReturnsEmptyFormCollection()
        {
            // Arrange
            var httpContext = GetMockHttpContext(null, hasForm: false);
            var bindingContext = GetBindingContext(typeof(IFormCollection), httpContext);
            var binder = new FormCollectionModelBinder();

            // Act
            var result = await binder.BindModelResultAsync(bindingContext);

            // Assert
            Assert.NotEqual(default(ModelBindingResult), result);
            var form = Assert.IsAssignableFrom<IFormCollection>(result.Model);
            Assert.Empty(form);
        }

        private static HttpContext GetMockHttpContext(IFormCollection formCollection, bool hasForm = true)
        {
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(h => h.Request.ReadFormAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(formCollection));
            httpContext.Setup(h => h.Request.HasFormContentType).Returns(hasForm);
            return httpContext.Object;
        }

        private static DefaultModelBindingContext GetBindingContext(Type modelType, HttpContext httpContext)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var bindingContext = new DefaultModelBindingContext
            {
                ActionContext = new ActionContext()
                {
                    HttpContext = httpContext,
                },
                ModelMetadata = metadataProvider.GetMetadataForType(modelType),
                ModelName = "file",
                ValidationState = new ValidationStateDictionary(),
            };

            return bindingContext;
        }
    }
}
