// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET45
using System.Collections.Generic;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Routing.Tests
{
    public class RouteKeyExistsConstraintTests
    {
        private readonly IRouteConstraint _constraint = new RouteDataConstraintsContainsKeyConstraint();

        [Theory]
        [InlineData("area")]
        [InlineData("controller")]
        [InlineData("action")]
        [InlineData("randomKey")]
        public void RouteKey_DoesNotExist_MatchFails(string keyName)
        {
            // Arrange
            var values = new Dictionary<string, object>();
            var httpContext = GetHttpContext(new ActionDescriptor());
            var route = (new Mock<IRouter>()).Object;
            
            // Act
            var match = _constraint.Match(httpContext, route, keyName, values, RouteDirection.IncomingRequest);

            // Assert
            Assert.False(match);
        }

        [Theory]
        [InlineData("area")]
        [InlineData("controller")]
        [InlineData("action")]
        [InlineData("randomKey")]
        public void RouteKey_Exists_MatchSucceeds(string keyName)
        {
            // Arrange
            var actionDescriptor = CreateActionDescriptor("testArea",
                                                          "testController",
                                                          "testAction");
            actionDescriptor.RouteConstraints.Add(new RouteDataActionConstraint("randomKey", "testRandom"));
            var httpContext = GetHttpContext(actionDescriptor);
            var route = (new Mock<IRouter>()).Object;
            var values = new Dictionary<string, object>()
                         {
                            { "area", "testArea" },
                            { "controller", "testController" },
                            { "action", "testAction" },
                            { "randomKey", "testRandom" }
                         };

            // Act
            var match = _constraint.Match(httpContext, route, keyName, values, RouteDirection.IncomingRequest);

            // Assert
            Assert.True(match);
        }

        private static HttpContext GetHttpContext(ActionDescriptor actionDescriptor)
        {
            var actionProvider = new Mock<INestedProviderManager<ActionDescriptorProviderContext>>(
                                                                                    MockBehavior.Strict);

            actionProvider
                .Setup(p => p.Invoke(It.IsAny<ActionDescriptorProviderContext>()))
                .Callback<ActionDescriptorProviderContext>(c => c.Results.Add(actionDescriptor));

            var context = new Mock<HttpContext>();
            context.Setup(o => o.ApplicationServices
                                .GetService(typeof(INestedProviderManager<ActionDescriptorProviderContext>)))
                   .Returns(actionProvider.Object);
            return context.Object;
        }

        private static ActionDescriptor CreateActionDescriptor(string area, string controller, string action)
        {
            var actionDescriptor = new ActionDescriptor()
            {
                Name = string.Format("Area: {0}, Controller: {1}, Action: {2}", area, controller, action),
                RouteConstraints = new List<RouteDataActionConstraint>(),
            };

            actionDescriptor.RouteConstraints.Add(
                area == null ?
                new RouteDataActionConstraint("area", RouteKeyHandling.DenyKey) :
                new RouteDataActionConstraint("area", area));

            actionDescriptor.RouteConstraints.Add(
                controller == null ?
                new RouteDataActionConstraint("controller", RouteKeyHandling.DenyKey) :
                new RouteDataActionConstraint("controller", controller));

            actionDescriptor.RouteConstraints.Add(
                action == null ?
                new RouteDataActionConstraint("action", RouteKeyHandling.DenyKey) :
                new RouteDataActionConstraint("action", action));

            return actionDescriptor;
        }
    }
}
#endif