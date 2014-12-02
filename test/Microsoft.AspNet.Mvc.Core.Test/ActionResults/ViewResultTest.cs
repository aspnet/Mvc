// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.PipelineCore;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc
{
    public class ViewResultTest
    {
        [Fact]
        public async Task ExecuteResultAsync_ReturnsError_IfViewCouldNotBeFound()
        {
            // Arrange
            var expected = string.Join(Environment.NewLine,
                                       "The view 'MyView' was not found. The following locations were searched:",
                                       "Location1",
                                       "Location2.");
            var actionContext = new ActionContext(new DefaultHttpContext(),
                                                  new RouteData(),
                                                  new ActionDescriptor());
            var viewEngine = new Mock<IViewEngine>();
            viewEngine.Setup(v => v.FindView(It.IsAny<ActionContext>(), It.IsAny<string>()))
                      .Returns(ViewEngineResult.NotFound("MyView", new[] { "Location1", "Location2" }))
                       .Verifiable();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(p => p.GetService(typeof(ILoggerFactory)))
                    .Returns(new NullLoggerFactory());
            actionContext.HttpContext.RequestServices = serviceProvider.Object;

            var viewResult = new ViewResult
            {
                ViewEngine = viewEngine.Object,
                ViewName = "MyView",
            };

            // Act and Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                                () => viewResult.ExecuteResultAsync(actionContext));
            Assert.Equal(expected, ex.Message);
            viewEngine.Verify();
        }

        [Fact]
        public async Task ViewResult_UsesFindViewOnSpecifiedViewEngineToLocateViews()
        {
            // Arrange
            var viewName = "myview";
            var actionContext = new ActionContext(new DefaultHttpContext(),
                                                  new RouteData(),
                                                  new ActionDescriptor());

            var viewEngine = new Mock<IViewEngine>();
            var view = Mock.Of<IView>();

            viewEngine.Setup(e => e.FindView(actionContext, "myview"))
                      .Returns(ViewEngineResult.Found("myview", view))
                      .Verifiable();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(p => p.GetService(typeof(ILoggerFactory)))
                    .Returns(new NullLoggerFactory());

            actionContext.HttpContext.RequestServices = serviceProvider.Object;
            var viewResult = new ViewResult
            {
                ViewName = viewName,
                ViewEngine = viewEngine.Object,
            };

            // Act
            await viewResult.ExecuteResultAsync(actionContext);

            // Assert
            viewEngine.Verify();
        }

        [Fact]
        public async Task ExecuteResultAsync_UsesActionDescriptorName_IfViewNameIsNull()
        {
            // Arrange
            var viewName = "some-view-name";
            var actionContext = new ActionContext(new DefaultHttpContext(),
                                                  new RouteData(),
                                                  new ActionDescriptor { Name = viewName });

            var viewEngine = new Mock<ICompositeViewEngine>();
            viewEngine.Setup(e => e.FindView(actionContext, viewName))
                      .Returns(ViewEngineResult.Found(viewName, Mock.Of<IView>()))
                      .Verifiable();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(p => p.GetService(typeof(ILoggerFactory)))
                    .Returns(new NullLoggerFactory());
            actionContext.HttpContext.RequestServices = serviceProvider.Object;

            var viewResult = new ViewResult
            {
                ViewEngine = viewEngine.Object,
            };

            // Act
            await viewResult.ExecuteResultAsync(actionContext);

            // Assert
            viewEngine.Verify();
        }

        [Fact]
        public async Task ExecuteResultAsync_UsesCompositeViewEngineFromServices_IfViewEngineIsNotSpecified()
        {
            // Arrange
            var viewName = "some-view-name";
            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(ILoggerFactory)))
                    .Returns(new NullLoggerFactory());
            var context = new DefaultHttpContext();
            context.ApplicationServices = services.Object;
            var actionContext = new ActionContext(context,
                                                  new RouteData(),
                                                  new ActionDescriptor { Name = viewName });

            var viewEngine = new Mock<ICompositeViewEngine>();
            viewEngine.Setup(e => e.FindView(actionContext, viewName))
                      .Returns(ViewEngineResult.Found(viewName, Mock.Of<IView>()))
                      .Verifiable();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(p => p.GetService(typeof(ICompositeViewEngine)))
                           .Returns(viewEngine.Object);
            serviceProvider.Setup(p => p.GetService(typeof(ILoggerFactory)))
                    .Returns(new NullLoggerFactory());
            actionContext.HttpContext.RequestServices = serviceProvider.Object;

            var viewResult = new ViewResult
            {
                ViewName = viewName,
            };

            // Act
            await viewResult.ExecuteResultAsync(actionContext);

            // Assert
            viewEngine.Verify();
        }
    }
}