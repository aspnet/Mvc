// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNET50
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test
{
    public class AsyncTimeoutAttributeTest
    {
        [Fact]
        public async Task RequestIsAborted_AfterTimeoutDurationElapses()
        {
            // Arrange
            var httpContext = new Mock<HttpContext>();
            var asyncTimeoutAttribute = new AsyncTimeoutAttribute(1 * 1000); // 1 second
            var resourceExecutingContext = GetResourceExecutingContext(httpContext.Object);
            
            // Act
            await asyncTimeoutAttribute.OnResourceExecutionAsync(
                resourceExecutingContext, 
                async () =>
                {
                    // Imagine here the rest of pipeline(ex: model-binding->action filters-action) being executed
                    await Task.Delay(10 * 1000); // 10 seconds
                    return null;
                });

            // Assert
            httpContext.Verify(hc => hc.Abort(), Times.Once);
        }

        [Fact]
        public async Task RequestIsNotAborted_BeforeTimeoutDurationElapses()
        {
            // Arrange
            var httpContext = new Mock<HttpContext>();
            var asyncTimeoutAttribute = new AsyncTimeoutAttribute(10 * 1000); // 10 seconds
            var resourceExecutingContext = GetResourceExecutingContext(httpContext.Object);
            
            // Act
            await asyncTimeoutAttribute.OnResourceExecutionAsync(
                resourceExecutingContext,
                async () =>
                {
                    // Imagine here the rest of pipeline(ex: model-binding->action filters-action) being executed
                    await Task.Delay(1 * 1000); // 1 second
                    return null;
                });

            // Assert
            httpContext.Verify(hc => hc.Abort(), Times.Never);
        }

        private ResourceExecutingContext GetResourceExecutingContext(HttpContext httpContext)
        {
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            return new ResourceExecutingContext(actionContext, new IFilter[] { });
        }
    }
}
#endif