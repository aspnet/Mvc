// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class RedirectResultTest
    {
        [Theory]
        [InlineData("", "/Home/About", "/Home/About")]
        [InlineData("/myapproot", "/test", "/test")]
        public void Execute_ReturnsContentPath_WhenItDoesNotStartWithTilde(string appRoot,
                                                                           string contentPath,
                                                                           string expectedPath)
        {
            // Arrange
            var httpResponse = new Mock<HttpResponse>();
            httpResponse.Setup(o => o.Redirect(expectedPath, false))
                        .Verifiable();

            var httpContext = GetHttpContext(appRoot, contentPath, expectedPath, httpResponse.Object);
            var actionContext = GetActionContext(httpContext);
            var result = new RedirectResult(contentPath);

            // Act
            result.ExecuteResult(actionContext);

            // Assert
            // Verifying if Redirect was called with the specific Url and parameter flag.
            httpResponse.Verify();
        }

        [Theory]
        [InlineData(null, "~/Home/About", "/Home/About")]
        [InlineData("/", "~/Home/About", "/Home/About")]
        [InlineData("/", "~/", "/")]
        [InlineData("", "~/Home/About", "/Home/About")]
        [InlineData("/myapproot", "~/", "/myapproot/")]
        [InlineData("", "~/Home/About", "/Home/About")]
        [InlineData("/myapproot", "~/", "/myapproot/")]
        public void Execute_ReturnsAppRelativePath_WhenItStartsWithTilde(string appRoot,
                                                                         string contentPath,
                                                                         string expectedPath)
        {
            // Arrange
            var httpResponse = new Mock<HttpResponse>();
            httpResponse.Setup(o => o.Redirect(expectedPath, false))
                        .Verifiable();

            var httpContext = GetHttpContext(appRoot, contentPath, expectedPath, httpResponse.Object);
            var actionContext = GetActionContext(httpContext);
            var result = new RedirectResult(contentPath);

            // Act
            result.ExecuteResult(actionContext);

            // Assert
            // Verifying if Redirect was called with the specific Url and parameter flag.
            httpResponse.Verify();
        }

        private static ActionContext GetActionContext(HttpContext httpContext)
        {
            return new ActionContext(httpContext,
                                    Mock.Of<IRouter>(),
                                    new Dictionary<string, object>(),
                                    new ActionDescriptor());
        }

        private static IServiceProvider GetServiceProvider(IUrlHelper urlHelper)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddInstance<IUrlHelper>(urlHelper);
            return serviceCollection.BuildServiceProvider();
        }

        private static HttpContext GetHttpContext(string appRoot, 
                                                     string contentPath,
                                                     string expectedPath,
                                                     HttpResponse response)
        {
            var httpContext = new Mock<HttpContext>();
            var actionContext = GetActionContext(httpContext.Object);
            var mockContentAccessor = new Mock<IContextAccessor<ActionContext>>();
            mockContentAccessor.SetupGet(o => o.Value).Returns(actionContext);
            var mockActionSelector = new Mock<IActionSelector>();
            var urlHelper = new UrlHelper(mockContentAccessor.Object, mockActionSelector.Object);
            var serviceProvider = GetServiceProvider(urlHelper);

            httpContext.Setup(o => o.Response)
                       .Returns(response);
            httpContext.SetupGet(o => o.RequestServices)
                       .Returns(serviceProvider);
            httpContext.Setup(o => o.Request.PathBase)
                       .Returns(new PathString(appRoot));

            return httpContext.Object;
        }
    }
}