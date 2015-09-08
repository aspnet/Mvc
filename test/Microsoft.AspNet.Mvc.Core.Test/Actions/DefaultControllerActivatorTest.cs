// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Actions
{
    public class DefaultControllerActivatorTest
    {
        [Theory]
        [InlineData(typeof(TypeDerivingFromController))]
        [InlineData(typeof(PocoType))]
        public void Create_CreatesInstancesOfTypes(Type type)
        {
            // Arrange
            var activator = new DefaultControllerActivator(new DefaultTypeActivatorCache());
            var serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider.Object
            };
            var actionContext = new ActionContext(httpContext,
                                                  new RouteData(),
                                                  new ActionDescriptor());
            // Act
            var instance = activator.Create(actionContext, type);

            // Assert
            Assert.IsType(type, instance);
        }

        [Fact]
        public void Create_TypeActivatesTypesWithServices()
        {
            // Arrange
            var activator = new DefaultControllerActivator(new DefaultTypeActivatorCache());
            var serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var testService = new TestService();
            serviceProvider.Setup(s => s.GetService(typeof(TestService)))
                           .Returns(testService)
                           .Verifiable();
                           
            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider.Object
            };
            var actionContext = new ActionContext(httpContext,
                                                  new RouteData(),
                                                  new ActionDescriptor());
            // Act
            var instance = activator.Create(actionContext, typeof(TypeDerivingFromControllerWithServices));

            // Assert
            var controller = Assert.IsType<TypeDerivingFromControllerWithServices>(instance);
            Assert.Same(testService, controller.TestService);
            serviceProvider.Verify();
        }

        public class Controller
        {
        }

        private class TypeDerivingFromController : Controller
        {
        }

        private class TypeDerivingFromControllerWithServices : Controller
        {
            public TypeDerivingFromControllerWithServices(TestService service)
            {
                TestService = service;
            }

            public TestService TestService { get; }
        }

        private class PocoType
        {
        }

        private class TestService
        {
        }
    }
}
