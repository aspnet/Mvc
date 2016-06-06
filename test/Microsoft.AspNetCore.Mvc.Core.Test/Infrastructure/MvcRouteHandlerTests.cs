// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Infrastructure
{
    public class MvcRouteHandlerTests
    {
        [Fact]
        public async Task RouteAsync_FailOnNoAction_LogsCorrectValues()
        {
            // Arrange
            var sink = new TestSink();
            var loggerFactory = new TestLoggerFactory(sink, enabled: true);

            var mockActionSelector = new Mock<IActionSelector>();
            mockActionSelector
                .Setup(a => a.SelectCandidates(It.IsAny<RouteContext>()))
                .Returns(new ActionDescriptor[0]);

            var context = CreateRouteContext();

            var handler = CreateMvcRouteHandler(
                actionSelector: mockActionSelector.Object,
                loggerFactory: loggerFactory);

            var expectedMessage = "No actions matched the current request";

            // Act
            await handler.RouteAsync(context);

            // Assert
            Assert.Empty(sink.Scopes);
            Assert.Single(sink.Writes);
            Assert.Equal(expectedMessage, sink.Writes[0].State?.ToString());
        }

        private MvcRouteHandler CreateMvcRouteHandler(
            ActionDescriptor actionDescriptor = null,
            IActionSelector actionSelector = null,
            IActionInvokerFactory invokerFactory = null,
            ILoggerFactory loggerFactory = null,
            object diagnosticListener = null)
        {
            var actionContextAccessor = new ActionContextAccessor();

            if (actionDescriptor == null)
            {
                var mockAction = new Mock<ActionDescriptor>();
                actionDescriptor = mockAction.Object;
            }

            if (actionSelector == null)
            {
                var mockActionSelector = new Mock<IActionSelector>();
                mockActionSelector
                    .Setup(a => a.SelectCandidates(It.IsAny<RouteContext>()))
                    .Returns(new ActionDescriptor[] { actionDescriptor });

                mockActionSelector
                    .Setup(a => a.SelectBestCandidate(It.IsAny<RouteContext>(), It.IsAny<IReadOnlyList<ActionDescriptor>>()))
                    .Returns(actionDescriptor);
                actionSelector = mockActionSelector.Object;
            }

            if (loggerFactory == null)
            {
                loggerFactory = NullLoggerFactory.Instance;
            }

            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
            if (diagnosticListener != null)
            {
                diagnosticSource.SubscribeWithAdapter(diagnosticListener);
            }

            if (invokerFactory == null)
            {
                var mockInvoker = new Mock<IActionInvoker>();
                mockInvoker.Setup(i => i.InvokeAsync())
                    .Returns(Task.FromResult(true));

                var mockInvokerFactory = new Mock<IActionInvokerFactory>();
                mockInvokerFactory.Setup(f => f.CreateInvoker(It.IsAny<ActionContext>()))
                    .Returns(mockInvoker.Object);

                invokerFactory = mockInvokerFactory.Object;
            }

            return new MvcRouteHandler(
                invokerFactory,
                actionSelector,
                diagnosticSource,
                loggerFactory,
                actionContextAccessor);
        }

        private RouteContext CreateRouteContext()
        {
            var routingFeature = new RoutingFeature();

            var httpContext = new Mock<HttpContext>();
            httpContext
                .Setup(h => h.Features[typeof(IRoutingFeature)])
                .Returns(routingFeature);

            var routeContext = new RouteContext(httpContext.Object);
            routingFeature.RouteData = routeContext.RouteData;
            return routeContext;
        }

        private class RoutingFeature : IRoutingFeature
        {
            public RouteData RouteData { get; set; }
        }
    }
}