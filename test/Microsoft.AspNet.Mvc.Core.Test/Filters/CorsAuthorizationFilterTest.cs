// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Cors.Core;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Core;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Newtonsoft.Json.Utilities;
using Xunit;

namespace Microsoft.AspNet.Mvc.Test
{
    public class CorsAuthorizationFilterTest
    {
        [Fact]
        public async Task PreFlightRequest_SuccessfulMatch_WritesHeaders()
        {
            // Arrange
            var filter = new CorsAuthorizationFilter(string.Empty);
            var mockEngine = GetPassingEngine(supportsCredentials:true);

            var authorizationContext = GetAuthorizationContext(
                mockEngine,
                new[] { new FilterDescriptor(filter, FilterScope.Action) },
                GetRequestHeaders(true),
                isPreflight: true);

            // Act
            await filter.OnAuthorizationAsync(authorizationContext);
            await authorizationContext.Result.ExecuteResultAsync(authorizationContext);

            // Assert
            var response = authorizationContext.HttpContext.Response;
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("http://example.com", response.Headers[CorsConstants.AccessControlAllowOrigin]);
            Assert.Equal("header1,header2", response.Headers[CorsConstants.AccessControlAllowHeaders]);

            // Notice: GET header gets filtered because it is a simple header.
            Assert.Equal("PUT", response.Headers[CorsConstants.AccessControlAllowMethods]);
            Assert.Equal("exposed1,exposed2", response.Headers[CorsConstants.AccessControlExposeHeaders]);
            Assert.Equal("123", response.Headers[CorsConstants.AccessControlMaxAge]);
            Assert.Equal("true", response.Headers[CorsConstants.AccessControlAllowCredentials]);
        }

        [Fact]
        public async Task PreFlight_FailedMatch_Writes200()
        {
            // Arrange
            var filter = new CorsAuthorizationFilter(string.Empty);
            var mockEngine = GetFailingEngine();

            var authorizationContext = GetAuthorizationContext(
                mockEngine,               
                new[] { new FilterDescriptor(filter, FilterScope.Action) },
                GetRequestHeaders(),
                isPreflight: true);

            // Act
            await filter.OnAuthorizationAsync(authorizationContext);
            await authorizationContext.Result.ExecuteResultAsync(authorizationContext);

            // Assert
            Assert.Equal(200, authorizationContext.HttpContext.Response.StatusCode);
            Assert.Empty(authorizationContext.HttpContext.Response.Headers);
        }

        [Fact]
        public async Task CorsRequest_SuccessfulMatch_WritesHeaders()
        {
            // Arrange
            var filter = new CorsAuthorizationFilter(string.Empty);
            var mockEngine = GetPassingEngine(supportsCredentials: true);

            var authorizationContext = GetAuthorizationContext(
                mockEngine,
                new[] { new FilterDescriptor(filter, FilterScope.Action) },
                GetRequestHeaders(true),
                isPreflight: true);

            // Act
            await filter.OnAuthorizationAsync(authorizationContext);
            await authorizationContext.Result.ExecuteResultAsync(authorizationContext);

            // Assert
            var response = authorizationContext.HttpContext.Response;
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("http://example.com", response.Headers[CorsConstants.AccessControlAllowOrigin]);
            Assert.Equal("exposed1,exposed2", response.Headers[CorsConstants.AccessControlExposeHeaders]);
        }

        [Fact]
        public async Task CorsRequest_FailedMatch_Writes200()
        {   
            // Arrange
            var filter = new CorsAuthorizationFilter(string.Empty);
            var mockEngine = GetFailingEngine();

            var authorizationContext = GetAuthorizationContext(
                mockEngine,
                new[] { new FilterDescriptor(filter, FilterScope.Action) },
                GetRequestHeaders(),
                isPreflight: false);

            // Act
            await filter.OnAuthorizationAsync(authorizationContext);

            // Assert
            Assert.Equal(200, authorizationContext.HttpContext.Response.StatusCode);
            Assert.Empty(authorizationContext.HttpContext.Response.Headers);
        }

        private AuthorizationContext GetAuthorizationContext(
            ICorsService corsService,
            FilterDescriptor[] filterDescriptors,
            RequestHeaders headers = null,
            bool isPreflight = false)
        {
            var policyProvider = new Mock<ICorsPolicyProvider>();
            policyProvider
                .Setup(o => o.GetPolicyAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new CorsPolicy()));
                       
            // ServiceProvider
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddInstance<ICorsService>(corsService);
            serviceCollection.AddInstance<ICorsPolicyProvider>(policyProvider.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // HttpContext
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;
            if (headers != null)
            {
                httpContext.Request.Headers.Add(CorsConstants.AccessControlRequestHeaders, headers.Headers.Split(','));
                httpContext.Request.Headers.Add(CorsConstants.AccessControlRequestMethod,  new[] { headers.Method });
                httpContext.Request.Headers.Add(CorsConstants.AccessControlExposeHeaders, headers.ExposedHeaders.Split(','));
                httpContext.Request.Headers.Add(CorsConstants.Origin, new[] { headers.Origin });
            }

            var method = isPreflight ? CorsConstants.PreflightHttpMethod : "GET";
            httpContext.Request.Method = method;

            // AuthorizationContext
            var actionContext = new ActionContext(
                httpContext: httpContext,
                routeData: new RouteData(),
                actionDescriptor: new ActionDescriptor() { FilterDescriptors = filterDescriptors });

            var authorizationContext = new AuthorizationContext(
                actionContext,
                filterDescriptors.Select(filter => filter.Filter).ToList()
            );

            return authorizationContext;
        }

        private ICorsService GetFailingEngine()
        {
            var mockEngine = new Mock<ICorsService>();
            var result = GetCorsResult("http://example.com");

            mockEngine
                .Setup(o => o.EvaluatePolicy(It.IsAny<HttpContext>(), It.IsAny<CorsPolicy>()))
                .Returns(result);
            return mockEngine.Object;
        }

        private ICorsService GetPassingEngine(bool supportsCredentials = false)
        {
            var mockEngine = new Mock<ICorsService>();
            var result = GetCorsResult(
                "http://example.com",
                new List<string> { "header1", "header2" },
                new List<string> { "PUT" },
                new List<string> { "exposed1", "exposed2" },
                123,
                supportsCredentials);

            mockEngine
                .Setup(o => o.EvaluatePolicy(It.IsAny<HttpContext>(), It.IsAny<CorsPolicy>()))
                .Returns(result);

            mockEngine
                .Setup(o => o.ApplyResult(It.IsAny<CorsResult>(), It.IsAny<HttpResponse>()))
                .Callback<CorsResult, HttpResponse>((result1, response1) =>
                {
                    var headers = response1.Headers;
                    headers.Set(
                        CorsConstants.AccessControlMaxAge,
                        result1.PreflightMaxAge.Value.TotalSeconds.ToString());
                    headers.Add(CorsConstants.AccessControlAllowOrigin, new[] { result1.AllowedOrigin });
                    if (result1.SupportsCredentials)
                    {
                        headers.Add(CorsConstants.AccessControlAllowCredentials, new[] { "true" });
                    }

                    headers.Add(CorsConstants.AccessControlAllowHeaders, result1.AllowedHeaders.ToArray());
                    headers.Add(CorsConstants.AccessControlAllowMethods, result1.AllowedMethods.ToArray());
                    headers.Add(CorsConstants.AccessControlExposeHeaders, result1.AllowedExposedHeaders.ToArray());
                });

            return mockEngine.Object;
        }

        private RequestHeaders GetRequestHeaders(bool supportsCredentials = false)
        {
            return new RequestHeaders
            {
                Origin = "http://example.com",
                Headers = "header1,header2",
                Method = "GET",
                ExposedHeaders = "exposed1,exposed2",
            };
        }

        private CorsResult GetCorsResult(
            string origin = null,
            IList<string> headers = null,
            IList<string> methods = null,
            IList<string> exposedHeaders = null,
            long? preFlightMaxAge = null,
            bool? supportsCredentials = null)
        {
            var result = new CorsResult();

            if (origin != null)
            {
                result.AllowedOrigin = origin;
            }

            if (headers != null)
            {
                AddRange(result.AllowedHeaders, headers);
            }

            if (methods != null)
            {
                AddRange(result.AllowedMethods, methods);
            }

            if (exposedHeaders != null)
            {
                AddRange(result.AllowedExposedHeaders, exposedHeaders);
            }

            if (preFlightMaxAge != null)
            {
                result.PreflightMaxAge = TimeSpan.FromSeconds(preFlightMaxAge.Value);
            }

            if (supportsCredentials != null)
            {
                result.SupportsCredentials = supportsCredentials.Value;
            }

            return result;
        }

        private void AddRange(IList<string> target, IList<string> source)
        {
            foreach (var item in source)
            {
                target.Add(item);
            }
        }

        private class RequestHeaders
        {
            public string Origin { get; set; }

            public string Headers { get; set; }

            public string ExposedHeaders { get; set; }

            public string Method { get; set; }
        }
    }
}
