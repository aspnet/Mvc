﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class MvcEndpointDataSourceTests
    {
        [Fact]
        public void Endpoints_AccessParameters_InitializedFromProvider()
        {
            // Arrange
            var routeValue = "Value";
            var requiredValues = new Dictionary<string, string>
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
                    RouteValues = requiredValues,
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
            var endpoints = dataSource.Endpoints;

            // Assert
            var endpoint = Assert.Single(endpoints);
            var matcherEndpoint = Assert.IsType<RouteEndpoint>(endpoint);

            var routeValuesAddressMetadata = matcherEndpoint.Metadata.GetMetadata<RouteValuesAddressMetadata>();
            Assert.NotNull(routeValuesAddressMetadata);
            var endpointValue = routeValuesAddressMetadata.RequiredValues["Name"];
            Assert.Equal(routeValue, endpointValue);

            Assert.Equal(displayName, matcherEndpoint.DisplayName);
            Assert.Equal(order, matcherEndpoint.Order);
            Assert.Equal(template, matcherEndpoint.RoutePattern.RawText);
        }

        [Fact]
        public async Task Endpoints_InvokeReturnedEndpoint_ActionInvokerProviderCalled()
        {
            // Arrange
            var endpointFeature = new EndpointFeature
            {
                RouteValues = new RouteValueDictionary()
            };

            var featureCollection = new FeatureCollection();
            featureCollection.Set<IEndpointFeature>(endpointFeature);
            featureCollection.Set<IRouteValuesFeature>(endpointFeature);

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
            var endpoints = dataSource.Endpoints;

            // Assert
            var endpoint = Assert.Single(endpoints);
            var matcherEndpoint = Assert.IsType<RouteEndpoint>(endpoint);

            await matcherEndpoint.RequestDelegate(httpContextMock.Object);

            Assert.True(actionInvokerCalled);
        }

        [Fact(Skip = "https://github.com/aspnet/Routing/issues/722")]
        public void GetChangeToken_MultipleChangeTokenProviders_ComposedResult()
        {
            // Arrange
            var featureCollection = new FeatureCollection();
            featureCollection.Set<IEndpointFeature>(new EndpointFeature
            {
                RouteValues = new RouteValueDictionary()
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
            var changeToken = dataSource.GetChangeToken();

            // Assert
            var compositeChangeToken = Assert.IsType<CompositeChangeToken>(changeToken);
            Assert.Equal(2, compositeChangeToken.ChangeTokens.Count);
        }

        [Theory]
        [InlineData("{controller}/{action}/{id?}", new[] { "TestController/TestAction/{id?}" })]
        [InlineData("{controller}/{id?}", new string[] { })]
        [InlineData("{action}/{id?}", new string[] { })]
        [InlineData("{Controller}/{Action}/{id?}", new[] { "TestController/TestAction/{id?}" })]
        [InlineData("{CONTROLLER}/{ACTION}/{id?}", new[] { "TestController/TestAction/{id?}" })]
        [InlineData("{controller}/{action=TestAction}", new[] { "TestController", "TestController/TestAction" })]
        [InlineData("{controller}/{action=TestAction}/{id?}", new[] { "TestController", "TestController/TestAction/{id?}" })]
        [InlineData("{controller=TestController}/{action=TestAction}/{id?}", new[] { "", "TestController", "TestController/TestAction/{id?}" })]
        [InlineData("{controller}/{action}/{*catchAll}", new[] { "TestController/TestAction/{*catchAll}" })]
        [InlineData("{controller}/{action=TestAction}/{*catchAll}", new[] { "TestController", "TestController/TestAction/{*catchAll}" })]
        [InlineData("{controller}/{action=TestAction}/{id?}/{*catchAll}", new[] { "TestController", "TestController/TestAction/{id?}/{*catchAll}" })]
        [InlineData("{controller}/{action}.{ext?}", new[] { "TestController/TestAction.{ext?}" })]
        [InlineData("{controller}/{action=TestAction}.{ext?}", new[] { "TestController", "TestController/TestAction.{ext?}" })]
        public void Endpoints_SingleAction(string endpointInfoRoute, string[] finalEndpointPatterns)
        {
            // Arrange
            var actionDescriptorCollection = GetActionDescriptorCollection(
                new { controller = "TestController", action = "TestAction" });
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(string.Empty, endpointInfoRoute));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            var inspectors = finalEndpointPatterns
                .Select(t => new Action<Endpoint>(e => Assert.Equal(t, Assert.IsType<RouteEndpoint>(e).RoutePattern.RawText)))
                .ToArray();

            // Assert
            Assert.Collection(endpoints, inspectors);
        }

        [Theory]
        [InlineData("{area}/{controller}/{action}/{id?}", new[] { "TestArea/TestController/TestAction/{id?}" })]
        [InlineData("{controller}/{action}/{id?}", new string[] { })]
        [InlineData("{area=TestArea}/{controller}/{action}/{id?}", new[] { "TestArea/TestController/TestAction/{id?}" })]
        [InlineData("{area=TestArea}/{controller}/{action=TestAction}/{id?}", new[] { "TestArea/TestController", "TestArea/TestController/TestAction/{id?}" })]
        [InlineData("{area=TestArea}/{controller=TestController}/{action=TestAction}/{id?}", new[] { "", "TestArea", "TestArea/TestController", "TestArea/TestController/TestAction/{id?}" })]
        [InlineData("{area:exists}/{controller}/{action}/{id?}", new[] { "TestArea/TestController/TestAction/{id?}" })]
        public void Endpoints_AreaSingleAction(string endpointInfoRoute, string[] finalEndpointTemplates)
        {
            // Arrange
            var actionDescriptorCollection = GetActionDescriptorCollection(
                new { controller = "TestController", action = "TestAction", area = "TestArea" });
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(string.Empty, endpointInfoRoute));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            var inspectors = finalEndpointTemplates
                .Select(t => new Action<Endpoint>(e => Assert.Equal(t, Assert.IsType<RouteEndpoint>(e).RoutePattern.RawText)))
                .ToArray();

            // Assert
            Assert.Collection(endpoints, inspectors);
        }

        [Fact]
        public void Endpoints_SingleAction_WithActionDefault()
        {
            // Arrange
            var actionDescriptorCollection = GetActionDescriptorCollection(
                new { controller = "TestController", action = "TestAction" });
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(
                string.Empty,
                "{controller}/{action}",
                new RouteValueDictionary(new { action = "TestAction" })));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            Assert.Collection(endpoints,
                (e) => Assert.Equal("TestController", Assert.IsType<RouteEndpoint>(e).RoutePattern.RawText),
                (e) => Assert.Equal("TestController/TestAction", Assert.IsType<RouteEndpoint>(e).RoutePattern.RawText));
        }

        [Fact]
        public void Endpoints_CalledMultipleTimes_ReturnsSameInstance()
        {
            // Arrange
            var actionDescriptorCollectionProviderMock = new Mock<IActionDescriptorCollectionProvider>();
            actionDescriptorCollectionProviderMock
                .Setup(m => m.ActionDescriptors)
                .Returns(new ActionDescriptorCollection(new[]
                {
                    CreateActionDescriptor(new { controller = "TestController", action = "TestAction" })
                }, version: 0));

            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollectionProviderMock.Object);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(
                string.Empty,
                "{controller}/{action}",
                new RouteValueDictionary(new { action = "TestAction" })));

            // Act
            var endpoints1 = dataSource.Endpoints;
            var endpoints2 = dataSource.Endpoints;

            // Assert
            Assert.Collection(endpoints1,
                (e) => Assert.Equal("TestController", Assert.IsType<RouteEndpoint>(e).RoutePattern.RawText),
                (e) => Assert.Equal("TestController/TestAction", Assert.IsType<RouteEndpoint>(e).RoutePattern.RawText));
            Assert.Same(endpoints1, endpoints2);

            actionDescriptorCollectionProviderMock.VerifyGet(m => m.ActionDescriptors, Times.Once);
        }

        [Fact(Skip = "https://github.com/aspnet/Routing/issues/722")]
        public void Endpoints_ChangeTokenTriggered_EndpointsRecreated()
        {
            // Arrange
            var actionDescriptorCollectionProviderMock = new Mock<IActionDescriptorCollectionProvider>();
            actionDescriptorCollectionProviderMock
                .Setup(m => m.ActionDescriptors)
                .Returns(new ActionDescriptorCollection(new[]
                {
                    CreateActionDescriptor(new { controller = "TestController", action = "TestAction" })
                }, version: 0));

            CancellationTokenSource cts = null;

            var changeProviderMock = new Mock<IActionDescriptorChangeProvider>();
            changeProviderMock.Setup(m => m.GetChangeToken()).Returns(() =>
            {
                cts = new CancellationTokenSource();
                var changeToken = new CancellationChangeToken(cts.Token);

                return changeToken;
            });

            var dataSource = CreateMvcEndpointDataSource(
                actionDescriptorCollectionProviderMock.Object,
                actionDescriptorChangeProviders: new[] { changeProviderMock.Object });
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(
                string.Empty,
                "{controller}/{action}",
                new RouteValueDictionary(new { action = "TestAction" })));

            // Act
            var endpoints = dataSource.Endpoints;

            Assert.Collection(endpoints,
                (e) => Assert.Equal("TestController", Assert.IsType<RouteEndpoint>(e).RoutePattern.RawText),
                (e) => Assert.Equal("TestController/TestAction", Assert.IsType<RouteEndpoint>(e).RoutePattern.RawText));

            actionDescriptorCollectionProviderMock
                .Setup(m => m.ActionDescriptors)
                .Returns(new ActionDescriptorCollection(new[]
                {
                    CreateActionDescriptor(new { controller = "NewTestController", action = "NewTestAction" })
                }, version: 1));

            cts.Cancel();

            // Assert
            var newEndpoints = dataSource.Endpoints;

            Assert.NotSame(endpoints, newEndpoints);
            Assert.Collection(newEndpoints,
                (e) => Assert.Equal("NewTestController/NewTestAction", Assert.IsType<RouteEndpoint>(e).RoutePattern.RawText));
        }

        [Fact]
        public void Endpoints_MultipleActions_WithActionConstraint()
        {
            // Arrange
            var actionDescriptorCollection = GetActionDescriptorCollection(
                new { controller = "TestController", action = "TestAction" },
                new { controller = "TestController", action = "TestAction1" },
                new { controller = "TestController", action = "TestAction2" });
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(
                string.Empty,
                "{controller}/{action}",
                constraints: new RouteValueDictionary(new { action = "(TestAction1|TestAction2)" })));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            Assert.Collection(endpoints,
                (e) => Assert.Equal("TestController/TestAction1", Assert.IsType<RouteEndpoint>(e).RoutePattern.RawText),
                (e) => Assert.Equal("TestController/TestAction2", Assert.IsType<RouteEndpoint>(e).RoutePattern.RawText));
        }

        [Theory]
        [InlineData("{controller}/{action}", new[] { "TestController1/TestAction1", "TestController1/TestAction2", "TestController1/TestAction3", "TestController2/TestAction1" })]
        [InlineData("{controller}/{action:regex((TestAction1|TestAction2))}", new[] { "TestController1/TestAction1", "TestController1/TestAction2", "TestController2/TestAction1" })]
        public void Endpoints_MultipleActions(string endpointInfoRoute, string[] finalEndpointTemplates)
        {
            // Arrange
            var actionDescriptorCollection = GetActionDescriptorCollection(
                new { controller = "TestController1", action = "TestAction1" },
                new { controller = "TestController1", action = "TestAction2" },
                new { controller = "TestController1", action = "TestAction3" },
                new { controller = "TestController2", action = "TestAction1" });
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(
                string.Empty,
                endpointInfoRoute));

            // Act
            var endpoints = dataSource.Endpoints;

            var inspectors = finalEndpointTemplates
                .Select(t => new Action<Endpoint>(e => Assert.Equal(t, Assert.IsType<RouteEndpoint>(e).RoutePattern.RawText)))
                .ToArray();

            // Assert
            Assert.Collection(endpoints, inspectors);
        }

        [Fact]
        public void Endpoints_ConventionalRoute_WithEmptyRouteName_CreatesMetadataWithEmptyRouteName()
        {
            // Arrange
            var actionDescriptorCollection = GetActionDescriptorCollection(
                new { controller = "Home", action = "Index" });
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(
                CreateEndpointInfo(string.Empty, "named/{controller}/{action}/{id?}"));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            var endpoint = Assert.Single(endpoints);
            var matcherEndpoint = Assert.IsType<RouteEndpoint>(endpoint);
            var routeValuesAddressNameMetadata = matcherEndpoint.Metadata.GetMetadata<IRouteValuesAddressMetadata>();
            Assert.NotNull(routeValuesAddressNameMetadata);
            Assert.Equal(string.Empty, routeValuesAddressNameMetadata.Name);
        }

        [Fact]
        public void Endpoints_CanCreateMultipleEndpoints_WithSameRouteName()
        {
            // Arrange
            var actionDescriptorCollection = GetActionDescriptorCollection(
                new { controller = "Home", action = "Index" },
                new { controller = "Products", action = "Details" });
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(
                CreateEndpointInfo("namedRoute", "named/{controller}/{action}/{id?}"));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            Assert.Collection(
                endpoints,
                (ep) =>
                {
                    var matcherEndpoint = Assert.IsType<RouteEndpoint>(ep);
                    var routeValuesAddressMetadata = matcherEndpoint.Metadata.GetMetadata<IRouteValuesAddressMetadata>();
                    Assert.NotNull(routeValuesAddressMetadata);
                    Assert.Equal("namedRoute", routeValuesAddressMetadata.Name);
                    Assert.Equal("named/Home/Index/{id?}", matcherEndpoint.RoutePattern.RawText);
                },
                (ep) =>
                {
                    var matcherEndpoint = Assert.IsType<RouteEndpoint>(ep);
                    var routeValuesAddressMetadata = matcherEndpoint.Metadata.GetMetadata<IRouteValuesAddressMetadata>();
                    Assert.NotNull(routeValuesAddressMetadata);
                    Assert.Equal("namedRoute", routeValuesAddressMetadata.Name);
                    Assert.Equal("named/Products/Details/{id?}", matcherEndpoint.RoutePattern.RawText);
                });
        }

        [Fact]
        public void Endpoints_ConventionalRoutes_StaticallyDefinedOrder_IsMaintained()
        {
            // Arrange
            var actionDescriptorCollection = GetActionDescriptorCollection(
                new { controller = "Home", action = "Index" },
                new { controller = "Products", action = "Details" });
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(
                name: string.Empty,
                template: "{controller}/{action}/{id?}"));
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(
                name: "namedRoute",
                "named/{controller}/{action}/{id?}"));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            Assert.Collection(
                endpoints,
                (ep) =>
                {
                    var matcherEndpoint = Assert.IsType<RouteEndpoint>(ep);
                    Assert.Equal("Home/Index/{id?}", matcherEndpoint.RoutePattern.RawText);
                    Assert.Equal(1, matcherEndpoint.Order);
                },
                (ep) =>
                {
                    var matcherEndpoint = Assert.IsType<RouteEndpoint>(ep);
                    Assert.Equal("named/Home/Index/{id?}", matcherEndpoint.RoutePattern.RawText);
                    Assert.Equal(2, matcherEndpoint.Order);
                },
                (ep) =>
                {
                    var matcherEndpoint = Assert.IsType<RouteEndpoint>(ep);
                    Assert.Equal("Products/Details/{id?}", matcherEndpoint.RoutePattern.RawText);
                    Assert.Equal(1, matcherEndpoint.Order);
                },
                (ep) =>
                {
                    var matcherEndpoint = Assert.IsType<RouteEndpoint>(ep);
                    Assert.Equal("named/Products/Details/{id?}", matcherEndpoint.RoutePattern.RawText);
                    Assert.Equal(2, matcherEndpoint.Order);
                });
        }

        [Fact]
        public void RequiredValue_WithNoCorresponding_TemplateParameter_DoesNotProduceEndpoint()
        {
            // Arrange
            var requiredValues = new RouteValueDictionary(new { area = "admin", controller = "home", action = "index" });
            var actionDescriptorCollection = GetActionDescriptorCollection(requiredValues);
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(string.Empty, "{controller}/{action}"));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            Assert.Empty(endpoints);
        }

        // Since area, controller, action and page are special, check to see if the followin test succeeds for a 
        // custom required value too.
        [Fact(Skip = "Needs review")]
        public void NonReservedRequiredValue_WithNoCorresponding_TemplateParameter_DoesNotProduceEndpoint()
        {
            // Arrange
            var requiredValues = new RouteValueDictionary(new { controller = "home", action = "index", foo = "bar" });
            var actionDescriptorCollection = GetActionDescriptorCollection(requiredValues);
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(string.Empty, "{controller}/{action}"));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            Assert.Empty(endpoints);
        }

        [Fact]
        public void TemplateParameter_WithNoDefaultOrRequiredValue_DoesNotProduceEndpoint()
        {
            // Arrange
            var requiredValues = new RouteValueDictionary(new { controller = "home", action = "index" });
            var actionDescriptorCollection = GetActionDescriptorCollection(requiredValues);
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(string.Empty, "{area}/{controller}/{action}"));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            Assert.Empty(endpoints);
        }

        [Fact]
        public void TemplateParameter_WithDefaultValue_AndNullRequiredValue_DoesNotProduceEndpoint()
        {
            // Arrange
            var requiredValues = new RouteValueDictionary(new { area = (string)null, controller = "home", action = "index" });
            var actionDescriptorCollection = GetActionDescriptorCollection(requiredValues);
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(string.Empty, "{area=admin}/{controller}/{action}"));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            Assert.Empty(endpoints);
        }

        [Fact]
        public void TemplateParameter_WithNullRequiredValue_DoesNotProduceEndpoint()
        {
            // Arrange
            var requiredValues = new RouteValueDictionary(new { area = (string)null, controller = "home", action = "index" });
            var actionDescriptorCollection = GetActionDescriptorCollection(requiredValues);
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(string.Empty, "{area}/{controller}/{action}"));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            Assert.Empty(endpoints);
        }

        [Fact]
        public void NoDefaultValues_RequiredValues_UsedToCreateDefaultValues()
        {
            // Arrange
            var expectedDefaults = new RouteValueDictionary(new { controller = "Foo", action = "Bar" });
            var actionDescriptorCollection = GetActionDescriptorCollection(requiredValues: expectedDefaults);
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(CreateEndpointInfo(string.Empty, "{controller}/{action}"));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            var endpoint = Assert.Single(endpoints);
            var matcherEndpoint = Assert.IsType<RouteEndpoint>(endpoint);
            Assert.Equal("Foo/Bar", matcherEndpoint.RoutePattern.RawText);
            AssertIsSubset(expectedDefaults, matcherEndpoint.RoutePattern.Defaults);
        }

        [Fact]
        public void RequiredValues_NotPresent_InDefaultValues_IsAddedToDefaultValues()
        {
            // Arrange
            var requiredValues = new RouteValueDictionary(
                new { controller = "Foo", action = "Bar", subarea = "test" });
            var expectedDefaults = requiredValues;
            var actionDescriptorCollection = GetActionDescriptorCollection(requiredValues: requiredValues);
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(
                CreateEndpointInfo(string.Empty, "{controller=Home}/{action=Index}"));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            var endpoint = Assert.Single(endpoints);
            var matcherEndpoint = Assert.IsType<RouteEndpoint>(endpoint);
            Assert.Equal("Foo/Bar", matcherEndpoint.RoutePattern.RawText);
            AssertIsSubset(expectedDefaults, matcherEndpoint.RoutePattern.Defaults);
        }

        [Fact]
        public void RequiredValues_IsSubsetOf_DefaultValues()
        {
            // Arrange
            var requiredValues = new RouteValueDictionary(
                new { controller = "Foo", action = "Bar", subarea = "test" });
            var expectedDefaults = new RouteValueDictionary(
                new { controller = "Foo", action = "Bar", subarea = "test", subscription = "general" });
            var actionDescriptorCollection = GetActionDescriptorCollection(requiredValues: requiredValues);
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(
                CreateEndpointInfo(string.Empty, "{controller=Home}/{action=Index}/{subscription=general}"));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            var endpoint = Assert.Single(endpoints);
            var matcherEndpoint = Assert.IsType<RouteEndpoint>(endpoint);
            Assert.Equal("Foo/Bar/{subscription=general}", matcherEndpoint.RoutePattern.RawText);
            AssertIsSubset(expectedDefaults, matcherEndpoint.RoutePattern.Defaults);
        }

        [Fact]
        public void RequiredValues_HavingNull_AndNotPresentInDefaultValues_IsAddedToDefaultValues()
        {
            // Arrange
            var requiredValues = new RouteValueDictionary(
                new { area = (string)null, controller = "Foo", action = "Bar", page = (string)null });
            var expectedDefaults = requiredValues;
            var actionDescriptorCollection = GetActionDescriptorCollection(requiredValues: requiredValues);
            var dataSource = CreateMvcEndpointDataSource(actionDescriptorCollection);
            dataSource.ConventionalEndpointInfos.Add(
                CreateEndpointInfo(string.Empty, "{controller=Home}/{action=Index}"));

            // Act
            var endpoints = dataSource.Endpoints;

            // Assert
            var endpoint = Assert.Single(endpoints);
            var matcherEndpoint = Assert.IsType<RouteEndpoint>(endpoint);
            Assert.Equal("Foo/Bar", matcherEndpoint.RoutePattern.RawText);
            AssertIsSubset(expectedDefaults, matcherEndpoint.RoutePattern.Defaults);
        }

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

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(m => m.GetService(typeof(IActionDescriptorCollectionProvider))).Returns(actionDescriptorCollectionProvider);

            var dataSource = new MvcEndpointDataSource(
                actionDescriptorCollectionProvider,
                mvcEndpointInvokerFactory ?? new MvcEndpointInvokerFactory(new ActionInvokerFactory(Array.Empty<IActionInvokerProvider>())),
                actionDescriptorChangeProviders ?? Array.Empty<IActionDescriptorChangeProvider>(),
                serviceProviderMock.Object);

            return dataSource;
        }

        private MvcEndpointInfo CreateEndpointInfo(
            string name,
            string template,
            RouteValueDictionary defaults = null,
            IDictionary<string, object> constraints = null,
            RouteValueDictionary dataTokens = null)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddRouting();

            var routeOptionsSetup = new MvcCoreRouteOptionsSetup();
            serviceCollection.Configure<RouteOptions>(routeOptionsSetup.Configure);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var parameterPolicyFactory = serviceProvider.GetRequiredService<ParameterPolicyFactory>();
            return new MvcEndpointInfo(name, template, defaults, constraints, dataTokens, parameterPolicyFactory);
        }

        private IActionDescriptorCollectionProvider GetActionDescriptorCollection(params object[] requiredValues)
        {
            var actionDescriptors = new List<ActionDescriptor>();
            foreach (var requiredValue in requiredValues)
            {
                actionDescriptors.Add(CreateActionDescriptor(requiredValue));
            }

            var actionDescriptorCollectionProviderMock = new Mock<IActionDescriptorCollectionProvider>();
            actionDescriptorCollectionProviderMock
                .Setup(m => m.ActionDescriptors)
                .Returns(new ActionDescriptorCollection(actionDescriptors, version: 0));
            return actionDescriptorCollectionProviderMock.Object;
        }

        private ActionDescriptor CreateActionDescriptor(string controller, string action, string area = null)
        {
            return CreateActionDescriptor(new { controller = controller, action = action, area = area });
        }

        private ActionDescriptor CreateActionDescriptor(object requiredValues)
        {
            var actionDescriptor = new ActionDescriptor();
            var routeValues = new RouteValueDictionary(requiredValues);
            foreach (var kvp in routeValues)
            {
                actionDescriptor.RouteValues[kvp.Key] = kvp.Value?.ToString();
            }
            return actionDescriptor;
        }

        private void AssertIsSubset(
            IReadOnlyDictionary<string, object> subset,
            IReadOnlyDictionary<string, object> fullSet)
        {
            foreach (var subsetPair in subset)
            {
                var isPresent = fullSet.TryGetValue(subsetPair.Key, out var fullSetPairValue);
                Assert.True(isPresent);
                Assert.Equal(subsetPair.Value, fullSetPairValue);
            }
        }
    }
}