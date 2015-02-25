// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if ASPNET50
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test
{
    public class AsyncTimeoutAttributeTest
    {
        [Fact]
        public async Task SetsTimeoutCancellationTokenFeature_OnExecution()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var asyncTimeoutAttribute = new AsyncTimeoutAttribute(1 * 1000); // 1 second
            var resourceExecutingContext = GetResourceExecutingContext(httpContext);

            // Act
            await asyncTimeoutAttribute.OnResourceExecutionAsync(
                resourceExecutingContext,
                () => Task.FromResult<ResourceExecutedContext>(null));

            // Assert
            var timeoutFeature = resourceExecutingContext.HttpContext.GetFeature<ITimeoutCancellationTokenFeature>();
            Assert.NotNull(timeoutFeature);
            Assert.NotNull(timeoutFeature.TimeoutCancellationToken);
        }

        private ResourceExecutingContext GetResourceExecutingContext(HttpContext httpContext)
        {
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            return new ResourceExecutingContext(actionContext, new IFilter[] { });
        }
    }
}
#endif