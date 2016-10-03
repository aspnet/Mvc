// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class AcceptedAtActionResultTests
    {
        public static TheoryData<object> ValuesData
        {
            get
            {
                return new TheoryData<object>
                {
                    null,
                    "Test string",
                    new ResourceStatusBody
                    {
                        EstimatedTime =DateTimeOffset.UtcNow.AddMinutes(5),
                        Status = ResourceStatus.In_Progress,
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValuesData))]
        public void AcceptedAtActionResult_InitializesStatusCodeAndValue(object value)
        {
            // Arrange 
            var expectedUrl = "testAction";

            //Act         
            var result = new AcceptedAtActionResult(
                actionName: expectedUrl,
                controllerName: null,
                routeValues: null, value: value);

            // Assert
            Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
            Assert.Same(value, result.Value);
        }

        [Theory]
        [MemberData(nameof(ValuesData))]
        public async Task AcceptedAtActionResult_SetsStatusCodeAndValueAsync(object value)
        {
            // Arrange           
            var expectedUrl = "testAction";
            var httpContext = GetHttpContext();
            var actionContext = GetActionContext(httpContext);
            var urlHelper = GetMockUrlHelper(expectedUrl);

            // Act
            var result = new AcceptedAtActionResult(
                actionName: expectedUrl,
                controllerName: null,
                routeValues: null,
                value: value);

            result.UrlHelper = urlHelper;
            await result.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(StatusCodes.Status202Accepted, httpContext.Response.StatusCode);
            Assert.Same(value, result.Value);
        }

        [Fact]
        public async Task AcceptedAtActionResult_ReturnsStatusCode_SetsLocationHeader()
        {
            // Arrange
            var expectedUrl = "testAction";
            var httpContext = GetHttpContext();
            var actionContext = GetActionContext(httpContext);
            var urlHelper = GetMockUrlHelper(expectedUrl);

            // Act
            var result = new AcceptedAtActionResult(
                actionName: expectedUrl,
                controllerName: null,
                routeValues: null,
                value: null);

            result.UrlHelper = urlHelper;
            await result.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(StatusCodes.Status202Accepted, httpContext.Response.StatusCode);
            Assert.Equal(expectedUrl, httpContext.Response.Headers["Location"]);
        }

        [Fact]
        public async Task AcceptedAtActionResult_ThrowsOnNullUrl()
        {
            // Arrange
            var httpContext = GetHttpContext();
            var actionContext = GetActionContext(httpContext);
            var urlHelper = GetMockUrlHelper(returnValue: null);

            var result = new AcceptedAtActionResult(
                actionName: null,
                controllerName: null,
                routeValues: null,
                value: null);

            result.UrlHelper = urlHelper;

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<InvalidOperationException>(
                async () => await result.ExecuteResultAsync(actionContext),
            "No route matches the supplied values.");
        }

        private static ActionContext GetActionContext(HttpContext httpContext)
        {
            var routeData = new RouteData();
            routeData.Routers.Add(Mock.Of<IRouter>());

            return new ActionContext(httpContext,
                                    routeData,
                                    new ActionDescriptor());
        }
        private static HttpContext GetHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.PathBase = new PathString("");
            httpContext.Response.Body = new MemoryStream();
            httpContext.RequestServices = CreateServices();
            return httpContext;
        }

        private static IServiceProvider CreateServices()
        {
            var options = new TestOptionsManager<MvcOptions>();
            options.Value.OutputFormatters.Add(new StringOutputFormatter());
            options.Value.OutputFormatters.Add(new JsonOutputFormatter(
                new JsonSerializerSettings(),
                ArrayPool<char>.Shared));

            var services = new ServiceCollection();
            services.AddSingleton(new ObjectResultExecutor(
                options,
                new TestHttpResponseStreamWriterFactory(),
                NullLoggerFactory.Instance));

            return services.BuildServiceProvider();
        }

        private static IUrlHelper GetMockUrlHelper(string returnValue)
        {
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(o => o.Action(It.IsAny<UrlActionContext>())).Returns(returnValue);

            return urlHelper.Object;
        }

        private class ResourceStatusBody
        {
            public DateTimeOffset EstimatedTime { get; set; }

            public ResourceStatus Status { get; set; }
        }

        private enum ResourceStatus { Failed, In_Progress, Completed };
    }
}