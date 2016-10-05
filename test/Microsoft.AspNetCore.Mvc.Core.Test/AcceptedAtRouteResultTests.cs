// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class AcceptedAtRouteResultTests
    {
        public static TheoryData<object> ValuesData
        {
            get
            {
                return new TheoryData<object>
                {
                    null,
                    "Test string",
                    new object(),
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValuesData))]
        public void Constructor_InitializesStatusCodeAndValue(object value)
        {
            // Arrange & Act
            var result = new AcceptedAtRouteResult(
                routeName: null,
                routeValues: null,
                value: value);

            // Assert
            Assert.Equal(StatusCodes.Status202Accepted, result.StatusCode);
            Assert.Same(value, result.Value);
        }

        [Theory]
        [MemberData(nameof(ValuesData))]
        public async Task ExecuteResultAsync_SetsStatusCodeAndValue(object value)
        {
            // Arrange     
            var expectedUrl = "testAction";
            var httpContext = GetHttpContext();
            var actionContext = GetActionContext(httpContext);
            var urlHelper = GetMockUrlHelper(expectedUrl);
            var routeValues = new RouteValueDictionary(new Dictionary<string, string>()
            {
                { "test", "case" },
                { "sample", "route" }
            });
            var result = new AcceptedAtRouteResult(
                routeName: "sample",
                routeValues: routeValues,
                value: value);
            result.UrlHelper = urlHelper;

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(StatusCodes.Status202Accepted, httpContext.Response.StatusCode);
            Assert.Same(value, result.Value);
        }

        public static TheoryData<object> AcceptedAtRouteData
        {
            get
            {
                return new TheoryData<object>
                {
                    null,
                    new Dictionary<string, string>()
                    {
                        { "hello", "world" }
                    },
                    new RouteValueDictionary(
                        new Dictionary<string, string>()
                        {
                            { "test", "case" },
                            { "sample", "route" }
                        }),
                    };
            }
        }

        [Theory]
        [MemberData(nameof(AcceptedAtRouteData))]
        public async Task ExecuteResultAsync_SetsStatusCodeAndLocationHeader(object values)
        {
            // Arrange
            var expectedUrl = "testAction";
            var httpContext = GetHttpContext();
            var actionContext = GetActionContext(httpContext);
            var urlHelper = GetMockUrlHelper(expectedUrl);

            // Act
            var result = new AcceptedAtRouteResult(routeValues: values, value: null);
            result.UrlHelper = urlHelper;
            await result.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(StatusCodes.Status202Accepted, httpContext.Response.StatusCode);
            Assert.Equal(expectedUrl, httpContext.Response.Headers["Location"]);
        }

        [Fact]
        public async Task ThrowsOnNullUrl()
        {
            // Arrange
            var httpContext = GetHttpContext();
            var actionContext = GetActionContext(httpContext);
            var urlHelper = GetMockUrlHelper(returnValue: null);

            var result = new AcceptedAtRouteResult(
                routeName: null,
                routeValues: new Dictionary<string, object>(),
                value: null);

            result.UrlHelper = urlHelper;

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<InvalidOperationException>(() =>
            result.ExecuteResultAsync(actionContext),
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
            urlHelper.Setup(o => o.Link(It.IsAny<string>(), It.IsAny<object>())).Returns(returnValue);

            return urlHelper.Object;
        }
    }
}
