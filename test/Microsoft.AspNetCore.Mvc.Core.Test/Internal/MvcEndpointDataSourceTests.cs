// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matchers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Core.Test.Internal
{
    public class MvcEndpointDataSourceTests
    {
        private MvcEndpointDataSource CreateMvcEndpointDataSource(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider = null,
            MvcEndpointInvokerFactory mvcEndpointInvokerFactory = null,
            IEnumerable<IActionDescriptorChangeProvider> actionDescriptorChangeProviders = null)
        {
            if (actionDescriptorCollectionProvider == null)
            {
                var mockDescriptorProvider = new Mock<IActionDescriptorCollectionProvider>();
                mockDescriptorProvider.Setup(m => m.ActionDescriptors).Returns(new ActionDescriptorCollection(new List<ActionDescriptor>(), 0));

                actionDescriptorCollectionProvider = mockDescriptorProvider.Object;
            }

            var dataSource = new MvcEndpointDataSource(
                actionDescriptorCollectionProvider,
                mvcEndpointInvokerFactory ?? new MvcEndpointInvokerFactory(new ActionInvokerFactory(Array.Empty<IActionInvokerProvider>())),
                actionDescriptorChangeProviders ?? Array.Empty<IActionDescriptorChangeProvider>(),
                Mock.Of<IServiceProvider>());

            return dataSource;
        }

        [Fact]
        public void Endpoints_AccessParameters_InitializedFromProvider()
        {
            // Arrange
            var routeValue = "Value";
            var routeValues = new Dictionary<string, string>
            {
                ["Name"] = routeValue
            };
            var displayName = "DisplayName!";
            var order = 1;
            var template = "/Template!";
            var filterDescriptor = new FilterDescriptor(new ControllerActionFilter(), 1);

            var mockDescriptorProvider = new Mock<IActionDescriptorCollectionProvider>();
            mockDescriptorProvider.Setup(m => m.ActionDescriptors).Returns(new ActionDescriptorCollection(new List<ActionDescriptor>
            {
                new ActionDescriptor
                {
                    RouteValues = routeValues,
                    DisplayName = displayName,
                    AttributeRouteInfo = new AttributeRouteInfo
                    {
                        Order = order,
                        Template = template
                    },
                    FilterDescriptors = new List<FilterDescriptor>
                    {
                        filterDescriptor
                    }
                }
            }, 0));

            var dataSource = CreateMvcEndpointDataSource(mockDescriptorProvider.Object);

            // Act
            dataSource.InitializeEndpoints();

            // Assert
            var endpoint = Assert.Single(dataSource.Endpoints);
            var matcherEndpoint = Assert.IsType<MatcherEndpoint>(endpoint);

            var endpointValue = matcherEndpoint.Values["Name"];
            Assert.Equal(routeValue, endpointValue);

            Assert.Equal(displayName, matcherEndpoint.DisplayName);
            Assert.Equal(order, matcherEndpoint.Order);
            Assert.Equal(template, matcherEndpoint.Template);
        }

        [Fact]
        public void Endpoints_InvokeReturnedEndpoint_ActionInvokerProviderCalled()
        {
            // Arrange
            var featureCollection = new FeatureCollection();
            featureCollection.Set<IEndpointFeature>(new EndpointFeature
            {
                Values = new RouteValueDictionary()
            });

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(m => m.Features).Returns(featureCollection);

            var descriptorProviderMock = new Mock<IActionDescriptorCollectionProvider>();
            descriptorProviderMock.Setup(m => m.ActionDescriptors).Returns(new ActionDescriptorCollection(new List<ActionDescriptor>
            {
                new ActionDescriptor
                {
                    AttributeRouteInfo = new AttributeRouteInfo
                    {
                        Template = string.Empty
                    },
                    FilterDescriptors = new List<FilterDescriptor>()
                }
            }, 0));

            var actionInvokerCalled = false;
            var actionInvokerMock = new Mock<IActionInvoker>();
            actionInvokerMock.Setup(m => m.InvokeAsync()).Returns(() =>
            {
                actionInvokerCalled = true;
                return Task.CompletedTask;
            });

            var actionInvokerProviderMock = new Mock<IActionInvokerFactory>();
            actionInvokerProviderMock.Setup(m => m.CreateInvoker(It.IsAny<ActionContext>())).Returns(actionInvokerMock.Object);

            var dataSource = CreateMvcEndpointDataSource(
                descriptorProviderMock.Object,
                new MvcEndpointInvokerFactory(actionInvokerProviderMock.Object));

            // Act
            dataSource.InitializeEndpoints();

            // Assert
            var endpoint = Assert.Single(dataSource.Endpoints);
            var matcherEndpoint = Assert.IsType<MatcherEndpoint>(endpoint);

            var invokerDelegate = matcherEndpoint.Invoker((next) => Task.CompletedTask);

            invokerDelegate(httpContextMock.Object);

            Assert.True(actionInvokerCalled);
        }

        [Fact]
        public void ChangeToken_MultipleChangeTokenProviders_ComposedResult()
        {
            // Arrange
            var featureCollection = new FeatureCollection();
            featureCollection.Set<IEndpointFeature>(new EndpointFeature
            {
                Values = new RouteValueDictionary()
            });

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(m => m.Features).Returns(featureCollection);

            var descriptorProviderMock = new Mock<IActionDescriptorCollectionProvider>();
            descriptorProviderMock.Setup(m => m.ActionDescriptors).Returns(new ActionDescriptorCollection(new List<ActionDescriptor>(), 0));

            var actionInvokerMock = new Mock<IActionInvoker>();

            var actionInvokerProviderMock = new Mock<IActionInvokerFactory>();
            actionInvokerProviderMock.Setup(m => m.CreateInvoker(It.IsAny<ActionContext>())).Returns(actionInvokerMock.Object);

            var changeTokenMock = new Mock<IChangeToken>();

            var changeProvider1Mock = new Mock<IActionDescriptorChangeProvider>();
            changeProvider1Mock.Setup(m => m.GetChangeToken()).Returns(changeTokenMock.Object);
            var changeProvider2Mock = new Mock<IActionDescriptorChangeProvider>();
            changeProvider2Mock.Setup(m => m.GetChangeToken()).Returns(changeTokenMock.Object);

            var dataSource = CreateMvcEndpointDataSource(
                descriptorProviderMock.Object,
                new MvcEndpointInvokerFactory(actionInvokerProviderMock.Object),
                new[] { changeProvider1Mock.Object, changeProvider2Mock.Object });

            // Act
            var changeToken = dataSource.ChangeToken;

            // Assert
            var compositeChangeToken = Assert.IsType<CompositeChangeToken>(changeToken);
            Assert.Equal(2, compositeChangeToken.ChangeTokens.Count);
        }

        private MvcEndpointInfo CreateEndpointInfo(
            string name,
            string template,
            RouteValueDictionary defaults = null,
            IDictionary<string, object> constraints = null,
            RouteValueDictionary dataTokens = null)
        {
            var constraintResolver = new DefaultInlineConstraintResolver(Options.Create<RouteOptions>(new RouteOptions()));
            return new MvcEndpointInfo(name, template, defaults, constraints, dataTokens, constraintResolver);
        }

        private ActionDescriptor CreateActionDescriptor(string controller, string action, string area = null)
        {
            return new ActionDescriptor
            {
                RouteValues =
                    {
                        ["controller"] = controller,
                        ["action"] = action,
                        ["area"] = area
                    },
                DisplayName = string.Empty,
            };
        }

        [Fact]
        public void DefaultMvcRoute()
        {
            // Arrange
            var mockDescriptorProvider = new Mock<IActionDescriptorCollectionProvider>();
            mockDescriptorProvider.Setup(m => m.ActionDescriptors).Returns(new ActionDescriptorCollection(new List<ActionDescriptor>
            {
                CreateActionDescriptor("TestController", "TestAction")
            }, 0));

            var dataSource = CreateMvcEndpointDataSource(mockDescriptorProvider.Object);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(string.Empty, "{controller}/{action}/{id?}"));

            // Act
            dataSource.InitializeEndpoints();

            // Assert
            var endpoint = Assert.Single(dataSource.Endpoints);
            var matcherEndpoint = Assert.IsType<MatcherEndpoint>(endpoint);

            Assert.Equal("TestController/TestAction/{id?}", matcherEndpoint.Template);
        }

        [Fact]
        public void WithActionInlineDefault()
        {
            // Arrange
            var mockDescriptorProvider = new Mock<IActionDescriptorCollectionProvider>();
            mockDescriptorProvider.Setup(m => m.ActionDescriptors).Returns(new ActionDescriptorCollection(new List<ActionDescriptor>
            {
                CreateActionDescriptor("TestController", "TestAction")
            }, 0));

            var dataSource = CreateMvcEndpointDataSource(mockDescriptorProvider.Object);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(string.Empty, "{controller}/{action=TestAction}"));

            // Act
            dataSource.InitializeEndpoints();

            // Assert
            Assert.Collection(dataSource.Endpoints,
                (e) => Assert.Equal("TestController", Assert.IsType<MatcherEndpoint>(e).Template),
                (e) => Assert.Equal("TestController/TestAction", Assert.IsType<MatcherEndpoint>(e).Template));
        }

        [Fact]
        public void WithActionDefault()
        {
            // Arrange
            var mockDescriptorProvider = new Mock<IActionDescriptorCollectionProvider>();
            mockDescriptorProvider.Setup(m => m.ActionDescriptors).Returns(new ActionDescriptorCollection(new List<ActionDescriptor>
            {
                CreateActionDescriptor("TestController", "TestAction")
            }, 0));

            var dataSource = CreateMvcEndpointDataSource(mockDescriptorProvider.Object);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(string.Empty, "{controller}/{action}", new RouteValueDictionary(new { action = "TestAction" })));

            // Act
            dataSource.InitializeEndpoints();

            // Assert
            Assert.Collection(dataSource.Endpoints,
                (e) => Assert.Equal("TestController", Assert.IsType<MatcherEndpoint>(e).Template),
                (e) => Assert.Equal("TestController/TestAction", Assert.IsType<MatcherEndpoint>(e).Template));
        }

        [Fact]
        public void WithActionConstraint()
        {
            // Arrange
            var mockDescriptorProvider = new Mock<IActionDescriptorCollectionProvider>();
            mockDescriptorProvider.Setup(m => m.ActionDescriptors).Returns(new ActionDescriptorCollection(new List<ActionDescriptor>
            {
                CreateActionDescriptor("TestController", "TestAction"),
                CreateActionDescriptor("TestController", "TestAction1"),
                CreateActionDescriptor("TestController", "TestAction2")
            }, 0));

            var dataSource = CreateMvcEndpointDataSource(mockDescriptorProvider.Object);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(
                string.Empty,
                "{controller}/{action}",
                constraints: new RouteValueDictionary(new { action = "(TestAction1|TestAction2)" })));

            // Act
            dataSource.InitializeEndpoints();

            // Assert
            Assert.Collection(dataSource.Endpoints,
                (e) => Assert.Equal("TestController/TestAction1", Assert.IsType<MatcherEndpoint>(e).Template),
                (e) => Assert.Equal("TestController/TestAction2", Assert.IsType<MatcherEndpoint>(e).Template));
        }

        [Fact]
        public void WithActionInlineConstraint()
        {
            // Arrange
            var mockDescriptorProvider = new Mock<IActionDescriptorCollectionProvider>();
            mockDescriptorProvider.Setup(m => m.ActionDescriptors).Returns(new ActionDescriptorCollection(new List<ActionDescriptor>
            {
                CreateActionDescriptor("TestController", "TestAction"),
                CreateActionDescriptor("TestController", "TestAction1"),
                CreateActionDescriptor("TestController", "TestAction2")
            }, 0));

            var dataSource = CreateMvcEndpointDataSource(mockDescriptorProvider.Object);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(
                string.Empty,
                "{controller}/{action:regex((TestAction1|TestAction2))}"));

            // Act
            dataSource.InitializeEndpoints();

            // Assert
            Assert.Collection(dataSource.Endpoints,
                (e) => Assert.Equal("TestController/TestAction1", Assert.IsType<MatcherEndpoint>(e).Template),
                (e) => Assert.Equal("TestController/TestAction2", Assert.IsType<MatcherEndpoint>(e).Template));
        }
    }
}
