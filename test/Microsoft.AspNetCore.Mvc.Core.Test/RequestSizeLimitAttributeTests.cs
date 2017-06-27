// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Core
{
    public class RequestSizeLimitAttributeTests
    {
        private const int RequestBodySizeLimit = 12345;

        [Theory]
        [InlineData(0)]
        [InlineData(1200)]
        [InlineData(12345)]
        public void ValidRequestLength_DoesNotThrowException_DisabledIsFalse(long requestLength)
        {
            // Arrange
            var requestBodySizeFilterAttribute = new RequestSizeLimitAttribute(RequestBodySizeLimit, false);
            var resourceExecutingContext = CreateResourceExecutingContext(new IFilterMetadata[] { requestBodySizeFilterAttribute });
            resourceExecutingContext.HttpContext.Request.Body = new MemoryStream();
            resourceExecutingContext.HttpContext.Request.Body.SetLength(requestLength);

            // Act
            requestBodySizeFilterAttribute.OnResourceExecuting(resourceExecutingContext);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1200)]
        [InlineData(12345)]
        public void ValidRequestLength_DoesNotThrowException_DisabledIsTrue(long requestLength)
        {
            // Arrange
            var requestBodySizeFilterAttribute = new RequestSizeLimitAttribute(RequestBodySizeLimit, true);
            var resourceExecutingContext = CreateResourceExecutingContext(new IFilterMetadata[] { requestBodySizeFilterAttribute });
            resourceExecutingContext.HttpContext.Request.Body = new MemoryStream();
            resourceExecutingContext.HttpContext.Request.Body.SetLength(requestLength);

            // Act
            requestBodySizeFilterAttribute.OnResourceExecuting(resourceExecutingContext);
        }

        [Fact]
        public void InvalidRequestLength_ThrowsInvalidOperationException()
        {
            // Arrange
            var requestBodySizeFilterAttribute = new RequestSizeLimitAttribute(RequestBodySizeLimit, false);
            var resourceExecutingContext = CreateResourceExecutingContext(new IFilterMetadata[] { requestBodySizeFilterAttribute });
            resourceExecutingContext.HttpContext.Request.Body = new MemoryStream();
            resourceExecutingContext.HttpContext.Request.Body.SetLength(RequestBodySizeLimit + 1);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => requestBodySizeFilterAttribute.OnResourceExecuting(resourceExecutingContext));
            Assert.Equal($"The size of the request body '{RequestBodySizeLimit+1}' is greater than the request size limit of '{RequestBodySizeLimit}', " +
                $"specified using the RequestSizeLimitAttribute.", ex.Message);
        }

        [Fact]
        public void InvalidRequestLength_DoesNotThrowWhenDisabledIsTrue()
        {
            // Arrange
            var requestBodySizeFilterAttribute = new RequestSizeLimitAttribute(RequestBodySizeLimit, true);
            var resourceExecutingContext = CreateResourceExecutingContext(new IFilterMetadata[] { requestBodySizeFilterAttribute });
            resourceExecutingContext.HttpContext.Request.Body = new MemoryStream();
            resourceExecutingContext.HttpContext.Request.Body.SetLength(RequestBodySizeLimit + 1);

            // Act
            requestBodySizeFilterAttribute.OnResourceExecuting(resourceExecutingContext);
        }

        [Fact]
        public void RequestSizeLimitLessThanZero_DoesNotThrow_SetsDisabledToTrue()
        {
            // Arrange
            var requestBodySizeFilterAttribute = new RequestSizeLimitAttribute(-1, false);
            var resourceExecutingContext = CreateResourceExecutingContext(new IFilterMetadata[] { requestBodySizeFilterAttribute });
            resourceExecutingContext.HttpContext.Request.Body = new MemoryStream();
            resourceExecutingContext.HttpContext.Request.Body.SetLength(0);

            // Act
            requestBodySizeFilterAttribute.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.True(requestBodySizeFilterAttribute.Disabled);
        }

        public ResourceExecutingContext CreateResourceExecutingContext(IFilterMetadata[] filters)
        {
            var context = new ResourceExecutingContext(
                CreateActionContext(),
                filters,
                new List<IValueProviderFactory>());
            return context;
        }

        private static ActionContext CreateActionContext()
        {
            return new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        }
    }
}
