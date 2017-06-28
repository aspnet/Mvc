// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Core
{
    public class RequestSizeLimitAttributeTests
    {
        [Fact]
        public void RequestSizeLimitAttribute_SetsMaxRequestBodySize()
        {
            // Arrange
            var requestSizeLimitAttribute = new RequestSizeLimitAttribute(12345);
            var resourceExecutingContext = CreateResourceExecutingContext(new IFilterMetadata[] { requestSizeLimitAttribute });
            var httpMaxRequestBodySize = new TestHttpMaxRequestBodySizeFeature();
            resourceExecutingContext.HttpContext.Features.Set<IHttpMaxRequestBodySizeFeature>(httpMaxRequestBodySize);

            // Act
            requestSizeLimitAttribute.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.Equal(12345, httpMaxRequestBodySize.MaxRequestBodySize);
        }

        [Fact]
        public void RequestSizeLimitAttribute_SkipsWhenOverridden()
        {
            // Arrange
            var requestSizeLimitAttribute = new RequestSizeLimitAttribute(12345);
            var disableRequestSizeLimitAttribute = new DisableRequestSizeLimitAttribute();
            var resourceExecutingContext = CreateResourceExecutingContext(new IFilterMetadata[] { requestSizeLimitAttribute, disableRequestSizeLimitAttribute});
            var httpMaxRequestBodySize = new TestHttpMaxRequestBodySizeFeature();
            resourceExecutingContext.HttpContext.Features.Set<IHttpMaxRequestBodySizeFeature>(httpMaxRequestBodySize);

            // Act
            requestSizeLimitAttribute.OnResourceExecuting(resourceExecutingContext);

            // Assert
            Assert.Null(httpMaxRequestBodySize.MaxRequestBodySize);
        }

        private static ResourceExecutingContext CreateResourceExecutingContext(IFilterMetadata[] filters)
        {
            return new ResourceExecutingContext(
                CreateActionContext(),
                filters,
                new List<IValueProviderFactory>());
        }

        private static ActionContext CreateActionContext()
        {
            return new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        }

        private class TestHttpMaxRequestBodySizeFeature : IHttpMaxRequestBodySizeFeature
        {
            public bool IsReadOnly => false;

            public long? MaxRequestBodySize { get; set; }
        }
    }
}
