// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    public class UrlHelperTest : UrlHelperTestBase
    {
        public static TheoryData GeneratePathFromRoute_HandlesLeadingAndTrailingSlashesData =>
            new TheoryData<string, string, string>
            {
                {  null, "", "/" },
                {  null, "/", "/"  },
                {  null, "Hello", "/Hello" },
                {  null, "/Hello", "/Hello" },
                { "/", "", "/" },
                { "/", "hello", "/hello" },
                { "/", "/hello", "/hello" },
                { "/hello", "", "/hello" },
                { "/hello/", "", "/hello/" },
                { "/hello", "/", "/hello/" },
                { "/hello/", "world", "/hello/world" },
                { "/hello/", "/world", "/hello/world" },
                { "/hello/", "/world 123", "/hello/world 123" },
                { "/hello/", "/world%20123", "/hello/world%20123" },
            };

        [Theory]
        [MemberData(nameof(GeneratePathFromRoute_HandlesLeadingAndTrailingSlashesData))]
        public void AppendPathAndFragment_HandlesLeadingAndTrailingSlashes(
            string appBase,
            string virtualPath,
            string expected)
        {
            // Arrange
            var services = CreateServices();
            var httpContext = CreateHttpContext(services, appBase, host: null, protocol: null);
            var builder = new StringBuilder();

            // Act
            UrlHelperBase.AppendPathAndFragment(builder, httpContext.Request.PathBase, virtualPath, string.Empty);

            // Assert
            Assert.Equal(expected, builder.ToString());
        }

        [Theory]
        [MemberData(nameof(GeneratePathFromRoute_HandlesLeadingAndTrailingSlashesData))]
        public void AppendPathAndFragment_AppendsFragments(
            string appBase,
            string virtualPath,
            string expected)
        {
            // Arrange
            var fragmentValue = "fragment-value";
            expected += $"#{fragmentValue}";
            var services = CreateServices();
            var httpContext = CreateHttpContext(services, appBase, host: null, protocol: null);
            var builder = new StringBuilder();

            // Act
            UrlHelperBase.AppendPathAndFragment(builder, httpContext.Request.PathBase, virtualPath, fragmentValue);

            // Assert
            Assert.Equal(expected, builder.ToString());
        }

        [Theory]
        [InlineData(null, null, null, "/", null, "/")]
        [InlineData(null, null, null, "/Hello", null, "/Hello")]
        [InlineData(null, null, null, "Hello", null, "/Hello")]
        [InlineData("/", null, null, "", null, "/")]
        [InlineData("/hello/", null, null, "/world", null, "/hello/world")]
        [InlineData("/hello/", "https", "myhost", "/world", "fragment-value", "https://myhost/hello/world#fragment-value")]
        public void GenerateUrl_FastAndSlowPathsReturnsExpected(
            string appBase,
            string protocol,
            string host,
            string virtualPath,
            string fragment,
            string expected)
        {
            // Arrange
            var router = Mock.Of<IRouter>();
            var pathData = new VirtualPathData(router, virtualPath)
            {
                VirtualPath = virtualPath
            };
            var services = CreateServices();
            var httpContext = CreateHttpContext(services, appBase, host, protocol);
            var actionContext = CreateActionContext(httpContext);
            actionContext.RouteData.Routers.Add(router);
            var urlHelper = new TestUrlHelper(actionContext);

            // Act
            var url = urlHelper.GenerateUrl(protocol, host, pathData, fragment);

            // Assert
            Assert.Equal(expected, url);
        }

        protected override IServiceProvider CreateServices()
        {
            var services = GetCommonServices();
            return services.BuildServiceProvider();
        }

        protected override IUrlHelper CreateUrlHelper(string appRoot, string host, string protocol)
        {
            var services = CreateServices();
            var httpContext = CreateHttpContext(services, appRoot, host, protocol);
            var actionContext = CreateActionContext(httpContext);
            var defaultRoutes = GetDefaultRoutes(services);
            actionContext.RouteData.Routers.Add(defaultRoutes);
            return new UrlHelper(actionContext);
        }

        protected override IUrlHelper CreateUrlHelperWithDefaultRoutes(
            string appRoot,
            string host,
            string protocol,
            string routeName,
            string template)
        {
            var services = CreateServices();
            var httpContext = CreateHttpContext(services, appRoot, host, protocol);
            var actionContext = CreateActionContext(httpContext);
            var router = GetDefaultRoutes(services, routeName, template);
            actionContext.RouteData.Routers.Add(router);
            return CreateUrlHelper(actionContext);
        }

        protected override IUrlHelper CreateUrlHelper(ActionContext actionContext)
        {
            return new UrlHelper(actionContext);
        }

        protected override IUrlHelper CreateUrlHelperWithDefaultRoutes(string appRoot, string host, string protocol)
        {
            var services = CreateServices();
            var context = CreateHttpContext(services, appRoot, host, protocol);

            var router = GetDefaultRoutes(services);
            var actionContext = CreateActionContext(context);
            actionContext.RouteData.Routers.Add(router);

            return CreateUrlHelper(actionContext);
        }

        private static IRouter GetDefaultRoutes(IServiceProvider services)
        {
            return GetDefaultRoutes(services, "mockRoute", "/mockTemplate");
        }

        private static IRouter GetDefaultRoutes(
            IServiceProvider services,
            string mockRouteName,
            string mockTemplateValue)
        {
            var routeBuilder = CreateRouteBuilder(services);

            var target = new Mock<IRouter>(MockBehavior.Strict);
            target
                .Setup(router => router.GetVirtualPath(It.IsAny<VirtualPathContext>()))
                .Returns<VirtualPathContext>(context => null);
            routeBuilder.DefaultHandler = target.Object;

            routeBuilder.MapRoute(
                string.Empty,
                "{controller}/{action}/{id}",
                new RouteValueDictionary(new { id = "defaultid" }));

            routeBuilder.MapRoute(
                "namedroute",
                "named/{controller}/{action}/{id}",
                new RouteValueDictionary(new { id = "defaultid" }));

            var mockHttpRoute = new Mock<IRouter>();
            mockHttpRoute
                .Setup(mock => mock.GetVirtualPath(It.Is<VirtualPathContext>(c => string.Equals(c.RouteName, mockRouteName))))
                .Returns(new VirtualPathData(mockHttpRoute.Object, mockTemplateValue));

            routeBuilder.Routes.Add(mockHttpRoute.Object);
            return routeBuilder.Build();
        }

        private static IRouteBuilder CreateRouteBuilder(IServiceProvider services)
        {
            var app = new Mock<IApplicationBuilder>();
            app
                .SetupGet(a => a.ApplicationServices)
                .Returns(services);

            return new RouteBuilder(app.Object)
            {
                DefaultHandler = new PassThroughRouter(),
            };
        }

        private class PassThroughRouter : IRouter
        {
            public VirtualPathData GetVirtualPath(VirtualPathContext context)
            {
                return null;
            }

            public Task RouteAsync(RouteContext context)
            {
                context.Handler = (c) => Task.FromResult(0);
                return Task.FromResult(false);
            }
        }

        private class TestUrlHelper : UrlHelper
        {
            public TestUrlHelper(ActionContext actionContext) :
                base(actionContext)
            {

            }

            public new string GenerateUrl(string protocol, string host, VirtualPathData pathData, string fragment)
            {
                return base.GenerateUrl(
                    protocol,
                    host,
                    pathData,
                    fragment);
            }
        }
    }
}