// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.ApplicationModels;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.NestedProviders;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Core.Test
{
    public class AssemblyLoggingTest
    {
        [Fact]
        public void SimpleController_LogsCorrectStructures()
        {
            // Arrange
            var sink = new TestSink();
            var loggerFactory = new TestLoggerFactory(sink);

            // Act
            CreateActionDescriptors(loggerFactory, typeof(SimpleController).GetTypeInfo());

            // Assert
            Assert.Equal(3, sink.Writes.Count);

            var assemblyValues = sink.Writes[0].State as AssemblyValues;
            Assert.NotNull(assemblyValues);
            Assert.True(assemblyValues.AssemblyName.Contains("Microsoft.AspNet.Mvc.Core.Test"));

            var controllerModelValues = sink.Writes[1].State as ControllerModelValues;
            Assert.NotNull(controllerModelValues);
            Assert.Equal("Simple", controllerModelValues.ControllerName);
            Assert.Equal(typeof(SimpleController), controllerModelValues.ControllerType);
            Assert.Single(controllerModelValues.Actions);
            Assert.Empty(controllerModelValues.AttributeRoutes);
            Assert.Empty(controllerModelValues.RouteConstraints);
            Assert.Empty(controllerModelValues.Attributes);
            Assert.Empty(controllerModelValues.Filters);

            var actionDescriptorValues = sink.Writes[2].State as ActionDescriptorValues;
            Assert.NotNull(actionDescriptorValues);
            Assert.Equal("EmptyAction", actionDescriptorValues.Name);
            Assert.Equal("Simple", actionDescriptorValues.ControllerName);
            Assert.Equal(typeof(SimpleController), actionDescriptorValues.ControllerTypeInfo);
            Assert.Null(actionDescriptorValues.AttributeRouteInfo.Name);
            Assert.Null(actionDescriptorValues.ActionConstraints);
            Assert.Empty(actionDescriptorValues.FilterDescriptors);
            Assert.Empty(actionDescriptorValues.Parameters);
        }

        [Fact]
        public void BasicController_LogsCorrectStructures()
        {
            // Arrange
            var sink = new TestSink();
            var loggerFactory = new TestLoggerFactory(sink);

            // Act
            CreateActionDescriptors(loggerFactory, typeof(BasicController).GetTypeInfo());

            // Assert
            Assert.Equal(4, sink.Writes.Count);

            var assemblyValues = sink.Writes[0].State as AssemblyValues;
            Assert.NotNull(assemblyValues);
            Assert.True(assemblyValues.AssemblyName.Contains("Microsoft.AspNet.Mvc.Core.Test"));

            var controllerModelValues = sink.Writes[1].State as ControllerModelValues;
            Assert.NotNull(controllerModelValues);
            Assert.Equal("Basic", controllerModelValues.ControllerName);
            Assert.Equal(typeof(BasicController), controllerModelValues.ControllerType);
            Assert.Equal(2, controllerModelValues.Actions.Count);
            Assert.Equal("GET", controllerModelValues.Actions[0].HttpMethods.FirstOrDefault());
            Assert.Equal("POST", controllerModelValues.Actions[1].HttpMethods.FirstOrDefault());
            Assert.Empty(controllerModelValues.AttributeRoutes);
            Assert.Empty(controllerModelValues.RouteConstraints);
            Assert.NotEmpty(controllerModelValues.Attributes);
            Assert.Single(controllerModelValues.Filters);

            var actionDescriptorValues = sink.Writes[2].State as ActionDescriptorValues;
            Assert.NotNull(actionDescriptorValues);
            Assert.Equal("Basic", actionDescriptorValues.Name);
            Assert.Equal("Basic", actionDescriptorValues.ControllerName);
            Assert.Equal(typeof(BasicController), actionDescriptorValues.ControllerTypeInfo);
            Assert.Null(actionDescriptorValues.AttributeRouteInfo.Name);
            Assert.NotEmpty(actionDescriptorValues.ActionConstraints);
            Assert.Equal(2, actionDescriptorValues.FilterDescriptors.Count);
            Assert.Empty(actionDescriptorValues.Parameters);

            actionDescriptorValues = sink.Writes[3].State as ActionDescriptorValues;
            Assert.NotNull(actionDescriptorValues);
            Assert.Equal("Basic", actionDescriptorValues.Name);
            Assert.Equal("Basic", actionDescriptorValues.ControllerName);
            Assert.Equal(typeof(BasicController), actionDescriptorValues.ControllerTypeInfo);
            Assert.Null(actionDescriptorValues.AttributeRouteInfo.Name);
            Assert.NotEmpty(actionDescriptorValues.ActionConstraints);
            Assert.Single(actionDescriptorValues.FilterDescriptors);
            Assert.Single(actionDescriptorValues.RouteConstraints);
            Assert.Single(actionDescriptorValues.Parameters);
        }

        private void CreateActionDescriptors(ILoggerFactory loggerFactory, params TypeInfo[] controllerTypeInfo)
        {
            var actionDescriptorProvider = GetProvider(loggerFactory, controllerTypeInfo);
            var descriptorProvider =
                new NestedProviderManager<ActionDescriptorProviderContext>(new[] { actionDescriptorProvider });

            var serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(INestedProviderManager<ActionDescriptorProviderContext>),
                                            descriptorProvider);

            var actionCollectionDescriptorProvider = new DefaultActionDescriptorsCollectionProvider(serviceContainer, loggerFactory);
            var descriptors = actionCollectionDescriptorProvider.ActionDescriptors;
        }

        private ControllerActionDescriptorProvider GetProvider(
            ILoggerFactory loggerFactory, params TypeInfo[] controllerTypeInfo)
        {
            var modelBuilder = new StaticControllerModelBuilder(controllerTypeInfo);

            var assemblyProvider = new Mock<IAssemblyProvider>();
            assemblyProvider
                .SetupGet(ap => ap.CandidateAssemblies)
                .Returns(new Assembly[] { controllerTypeInfo.First().Assembly });

            var provider = new ControllerActionDescriptorProvider(
                assemblyProvider.Object,
                modelBuilder,
                new TestGlobalFilterProvider(),
                new MockMvcOptionsAccessor(),
                loggerFactory);

            return provider;
        }

        private class SimpleController
        {
            public void EmptyAction() { }
        }

        [Authorize]
        private class BasicController
        {
            [HttpGet]
            [AllowAnonymous]
            public void Basic() { }

            [HttpPost]
            [Route("/Basic")]
            public void Basic(int id) { }
        }
    }
}