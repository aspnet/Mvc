// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Template;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;
using Microsoft.AspNet.PipelineCore;

namespace Microsoft.AspNet.Mvc.Routing
{
    public class AttributeRouteTests
    {
        [Theory]
        [InlineData("template/5", "template/{parameter:int}")]
        [InlineData("template/5", "template/{parameter}")]
        [InlineData("template/5", "template/{*parameter:int}")]
        [InlineData("template/5", "template/{*parameter}")]
        [InlineData("template/{parameter:int}", "template/{parameter}")]
        [InlineData("template/{parameter:int}", "template/{*parameter:int}")]
        [InlineData("template/{parameter:int}", "template/{*parameter}")]
        [InlineData("template/{parameter}", "template/{*parameter:int}")]
        [InlineData("template/{parameter}", "template/{*parameter}")]
        [InlineData("template/{*parameter:int}", "template/{*parameter}")]
        public async Task AttributeRoute_RouteAsync_RespectsPrecedence(
            string firstTemplate, 
            string secondTemplate)
        {
            // Arrange
            var next = new Mock<IRouter>().Object;

            var firstRouter = new Mock<IRouter>();
            firstRouter.Setup(r => r.RouteAsync(It.IsAny<RouteContext>()))
                .Returns(Task.FromResult(true))
                .Verifiable();

            var secondRouter = new Mock<IRouter>();

            var firstRoute = CreateMatchingEntry(firstRouter.Object, firstTemplate, order: 0);
            var secondRoute = CreateMatchingEntry(secondRouter.Object, secondTemplate, order: 0);
            var matchingRoutes = new[] { secondRoute, firstRoute };

            var linkGenerationEntries = Enumerable.Empty<AttributeRouteLinkGenerationEntry>();

            var route = new AttributeRoute(next, matchingRoutes, linkGenerationEntries);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = new PathString("/template/5");

            var context = new RouteContext(httpContext);

            // Act
            await route.RouteAsync(context);

            // Assert
            Assert.DoesNotThrow(() => firstRouter.Verify());
        }

        [Theory]
        [InlineData("template/5", "template/{parameter:int}")]
        [InlineData("template/5", "template/{parameter}")]
        [InlineData("template/5", "template/{*parameter:int}")]
        [InlineData("template/5", "template/{*parameter}")]
        [InlineData("template/{parameter:int}", "template/{parameter}")]
        [InlineData("template/{parameter:int}", "template/{*parameter:int}")]
        [InlineData("template/{parameter:int}", "template/{*parameter}")]
        [InlineData("template/{parameter}", "template/{*parameter:int}")]
        [InlineData("template/{parameter}", "template/{*parameter}")]
        [InlineData("template/{*parameter:int}", "template/{*parameter}")]
        public async Task AttributeRoute_RouteAsync_RespectsOrderOverPrecedence(
            string firstTemplate, 
            string secondTemplate)
        {
            // Arrange
            var next = new Mock<IRouter>().Object;

            var firstRouter = new Mock<IRouter>();

            var secondRouter = new Mock<IRouter>();
            secondRouter.Setup(r => r.RouteAsync(It.IsAny<RouteContext>()))
                .Returns(Task.FromResult(true))
                .Verifiable();

            var firstRoute = CreateMatchingEntry(firstRouter.Object, firstTemplate, order: 1);
            var secondRoute = CreateMatchingEntry(secondRouter.Object, secondTemplate, order: 0);
            var matchingRoutes = new[] { firstRoute, secondRoute };

            var linkGenerationEntries = Enumerable.Empty<AttributeRouteLinkGenerationEntry>();

            var route = new AttributeRoute(next, matchingRoutes, linkGenerationEntries);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = new PathString("/template/5");

            var context = new RouteContext(httpContext);

            // Act
            await route.RouteAsync(context);

            // Assert
            Assert.DoesNotThrow(() => secondRouter.Verify());
        }

        [Theory]
        [InlineData("template/5")]
        [InlineData("template/{parameter:int}")]
        [InlineData("template/{parameter}")]
        [InlineData("template/{*parameter:int}")]
        [InlineData("template/{*parameter}")]
        public async Task AttributeRoute_RouteAsync_RespectsOrder(string template)
        {
            // Arrange
            var next = new Mock<IRouter>().Object;

            var firstRouter = new Mock<IRouter>();

            var secondRouter = new Mock<IRouter>();
            secondRouter.Setup(r => r.RouteAsync(It.IsAny<RouteContext>()))
                .Returns(Task.FromResult(true))
                .Verifiable();

            var firstRoute = CreateMatchingEntry(firstRouter.Object, template, order: 1);
            var secondRoute = CreateMatchingEntry(secondRouter.Object, template, order: 0);
            var matchingRoutes = new[] { firstRoute, secondRoute };

            var linkGenerationEntries = Enumerable.Empty<AttributeRouteLinkGenerationEntry>();

            var route = new AttributeRoute(next, matchingRoutes, linkGenerationEntries);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = new PathString("/template/5");

            var context = new RouteContext(httpContext);

            // Act
            await route.RouteAsync(context);

            // Assert
            Assert.DoesNotThrow(() => secondRouter.Verify());
        }

        [Theory]
        [InlineData("template/{first:int}", "template/{second:int}")]
        [InlineData("template/{first}", "template/{second}")]
        [InlineData("template/{*first:int}", "template/{*second:int}")]
        [InlineData("template/{*first}", "template/{*second}")]
        public async Task AttributeRoute_RouteAsync_EnsuresStableOrdering(string first, string second)
        {
            // Arrange
            var next = new Mock<IRouter>().Object;

            var firstRouter = new Mock<IRouter>();
            firstRouter.Setup(r => r.RouteAsync(It.IsAny<RouteContext>()))
                .Returns(Task.FromResult(true))
                .Verifiable();

            var secondRouter = new Mock<IRouter>();

            var firstRoute = CreateMatchingEntry(firstRouter.Object, first, order: 0);
            var secondRoute = CreateMatchingEntry(secondRouter.Object, second, order: 0);
            var matchingRoutes = new[] { secondRoute, firstRoute };

            var linkGenerationEntries = Enumerable.Empty<AttributeRouteLinkGenerationEntry>();

            var route = new AttributeRoute(next, matchingRoutes, linkGenerationEntries);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = new PathString("/template/5");

            var context = new RouteContext(httpContext);

            // Act
            await route.RouteAsync(context);

            // Assert
            Assert.DoesNotThrow(() => firstRouter.Verify());
        }

        [Theory]
        [InlineData("template/5", "template/{parameter:int}")]
        [InlineData("template/5", "template/{parameter}")]
        [InlineData("template/5", "template/{*parameter:int}")]
        [InlineData("template/5", "template/{*parameter}")]
        [InlineData("template/{parameter:int}", "template/{parameter}")]
        [InlineData("template/{parameter:int}", "template/{*parameter:int}")]
        [InlineData("template/{parameter:int}", "template/{*parameter}")]
        [InlineData("template/{parameter}", "template/{*parameter:int}")]
        [InlineData("template/{parameter}", "template/{*parameter}")]
        [InlineData("template/{*parameter:int}", "template/{*parameter}")]
        public void AttributeRoute_GenerateLink_RespectsPrecedence(string firstTemplate, string secondTemplate)
        {
            // Arrange
            var expectedGroup = CreateRouteGroup(0, firstTemplate);

            string selectedGroup = null;

            var router = new Mock<IRouter>();
            router.Setup(n => n.GetVirtualPath(It.IsAny<VirtualPathContext>())).Callback<VirtualPathContext>(ctx =>
            {
                selectedGroup = (string)ctx.ProvidedValues[AttributeRouting.RouteGroupKey];
                ctx.IsBound = true;
            })
            .Returns((string)null);

            var matchingRoutes = Enumerable.Empty<AttributeRouteMatchingEntry>();

            var firstEntry = CreateGenerationEntry(firstTemplate, requiredValues: null);
            var secondEntry = CreateGenerationEntry(secondTemplate, requiredValues: null, order: 0);
            var linkGenerationEntries = new[] { secondEntry, firstEntry };

            var route = new AttributeRoute(router.Object, matchingRoutes, linkGenerationEntries);

            var context = CreateVirtualPathContext(values: null, ambientValues: new { parameter = 5 });

            // Act
            string result = route.GetVirtualPath(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("template/5", result);
            Assert.Equal(expectedGroup, selectedGroup);
        }

        [Theory]
        [InlineData("template/5", "template/{parameter:int}")]
        [InlineData("template/5", "template/{parameter}")]
        [InlineData("template/5", "template/{*parameter:int}")]
        [InlineData("template/5", "template/{*parameter}")]
        [InlineData("template/{parameter:int}", "template/{parameter}")]
        [InlineData("template/{parameter:int}", "template/{*parameter:int}")]
        [InlineData("template/{parameter:int}", "template/{*parameter}")]
        [InlineData("template/{parameter}", "template/{*parameter:int}")]
        [InlineData("template/{parameter}", "template/{*parameter}")]
        [InlineData("template/{*parameter:int}", "template/{*parameter}")]
        public void AttributeRoute_GenerateLink_RespectsOrderOverPrecedence(string firstTemplate, string secondTemplate)
        {
            // Arrange
            var selectedGroup = CreateRouteGroup(0, secondTemplate);

            string firstRouteGroupSelected = null;
            var next = new Mock<IRouter>();
            next.Setup(n => n.GetVirtualPath(It.IsAny<VirtualPathContext>())).Callback<VirtualPathContext>(ctx =>
            {
                firstRouteGroupSelected = (string)ctx.ProvidedValues[AttributeRouting.RouteGroupKey];
                ctx.IsBound = true;
            })
            .Returns((string)null);

            var matchingRoutes = Enumerable.Empty<AttributeRouteMatchingEntry>();

            var firstRoute = CreateGenerationEntry(firstTemplate, requiredValues: null, order: 1);
            var secondRoute = CreateGenerationEntry(secondTemplate, requiredValues: null, order: 0);
            var linkGenerationEntries = new[] { firstRoute, secondRoute };

            var route = new AttributeRoute(next.Object, matchingRoutes, linkGenerationEntries);

            var context = CreateVirtualPathContext(null, ambientValues: new { parameter = 5 });

            // Act
            string result = route.GetVirtualPath(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("template/5", result);
            Assert.Equal(selectedGroup, firstRouteGroupSelected);
        }

        [Theory]
        [InlineData("template/5", "template/5")]
        [InlineData("template/{first:int}", "template/{second:int}")]
        [InlineData("template/{first}", "template/{second}")]
        [InlineData("template/{*first:int}", "template/{*second:int}")]
        [InlineData("template/{*first}", "template/{*second}")]
        public void AttributeRoute_GenerateLink_RespectsOrder(string firstTemplate, string secondTemplate)
        {
            // Arrange
            var expectedGroup = CreateRouteGroup(0, secondTemplate);

            var next = new Mock<IRouter>();
            string selectedGroup = null;
            next.Setup(n => n.GetVirtualPath(It.IsAny<VirtualPathContext>())).Callback<VirtualPathContext>(ctx =>
            {
                selectedGroup = (string)ctx.ProvidedValues[AttributeRouting.RouteGroupKey];
                ctx.IsBound = true;
            })
            .Returns((string)null);

            var matchingRoutes = Enumerable.Empty<AttributeRouteMatchingEntry>();

            var firstRoute = CreateGenerationEntry(firstTemplate, requiredValues: null, order: 1);
            var secondRoute = CreateGenerationEntry(secondTemplate, requiredValues: null, order: 0);
            var linkGenerationEntries = new[] { firstRoute, secondRoute };

            var route = new AttributeRoute(next.Object, matchingRoutes, linkGenerationEntries);

            var context = CreateVirtualPathContext(values: null, ambientValues: new { first = 5, second = 5 });

            // Act
            string result = route.GetVirtualPath(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("template/5", result);
            Assert.Equal(expectedGroup, selectedGroup);
        }

        [Theory]
        [InlineData("first/5", "second/5")]
        [InlineData("first/{first:int}", "second/{second:int}")]
        [InlineData("first/{first}", "second/{second}")]
        [InlineData("first/{*first:int}", "second/{*second:int}")]
        [InlineData("first/{*first}", "second/{*second}")]
        public void AttributeRoute_GenerateLink_EnsuresStableOrder(string firstTemplate, string secondTemplate)
        {
            // Arrange
            var expectedGroup = CreateRouteGroup(0, firstTemplate);

            var next = new Mock<IRouter>();
            string selectedGroup = null;
            next.Setup(n => n.GetVirtualPath(It.IsAny<VirtualPathContext>())).Callback<VirtualPathContext>(ctx =>
            {
                selectedGroup = (string)ctx.ProvidedValues[AttributeRouting.RouteGroupKey];
                ctx.IsBound = true;
            })
            .Returns((string)null);

            var matchingRoutes = Enumerable.Empty<AttributeRouteMatchingEntry>();

            var firstRoute = CreateGenerationEntry(firstTemplate, requiredValues: null, order: 0);
            var secondRoute = CreateGenerationEntry(secondTemplate, requiredValues: null, order: 0);
            var linkGenerationEntries = new[] { secondRoute, firstRoute };

            var route = new AttributeRoute(next.Object, matchingRoutes, linkGenerationEntries);

            var context = CreateVirtualPathContext(values: null, ambientValues: new { first = 5, second = 5 });

            // Act
            string result = route.GetVirtualPath(context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("first/5", result);
            Assert.Equal(expectedGroup, selectedGroup);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_NoRequiredValues()
        {
            // Arrange
            var entry = CreateGenerationEntry("api/Store", new { });
            var route = CreateAttributeRoute(entry);

            var context = CreateVirtualPathContext(new { });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("api/Store", path);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_Match()
        {
            // Arrange
            var entry = CreateGenerationEntry("api/Store", new { action = "Index", controller = "Store" });
            var route = CreateAttributeRoute(entry);

            var context = CreateVirtualPathContext(new { action = "Index", controller = "Store" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("api/Store", path);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_NoMatch()
        {
            // Arrange
            var entry = CreateGenerationEntry("api/Store", new { action = "Details", controller = "Store" });
            var route = CreateAttributeRoute(entry);

            var context = CreateVirtualPathContext(new { action = "Index", controller = "Store" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Null(path);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_Match_WithAmbientValues()
        {
            // Arrange
            var entry = CreateGenerationEntry("api/Store", new { action = "Index", controller = "Store" });
            var route = CreateAttributeRoute(entry);

            var context = CreateVirtualPathContext(new { }, new { action = "Index", controller = "Store" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("api/Store", path);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_Match_WithParameters()
        {
            // Arrange
            var entry = CreateGenerationEntry("api/Store/{action}", new { action = "Index", controller = "Store" });
            var route = CreateAttributeRoute(entry);

            var context = CreateVirtualPathContext(new { action = "Index", controller = "Store" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("api/Store/Index", path);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_Match_WithMoreParameters()
        {
            // Arrange
            var entry = CreateGenerationEntry(
                "api/{area}/dosomething/{controller}/{action}",
                new { action = "Index", controller = "Store", area = "AwesomeCo" });

            var expectedValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "area", "AwesomeCo" },
                { "controller", "Store" },
                { "action", "Index" },
                { AttributeRouting.RouteGroupKey, entry.RouteGroup },
            };

            var next = new StubRouter();
            var route = CreateAttributeRoute(next, entry);

            var context = CreateVirtualPathContext(
                new { action = "Index", controller = "Store" },
                new { area = "AwesomeCo" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("api/AwesomeCo/dosomething/Store/Index", path);
            Assert.Equal(expectedValues, next.GenerationContext.ProvidedValues);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_Match_WithDefault()
        {
            // Arrange
            var entry = CreateGenerationEntry("api/Store/{action=Index}", new { action = "Index", controller = "Store" });
            var route = CreateAttributeRoute(entry);

            var context = CreateVirtualPathContext(new { action = "Index", controller = "Store" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("api/Store", path);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_Match_WithConstraint()
        {
            // Arrange
            var entry = CreateGenerationEntry("api/Store/{action}/{id:int}", new { action = "Index", controller = "Store" });

            var expectedValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "action", "Index" },
                { "id", 5 },
                { AttributeRouting.RouteGroupKey, entry.RouteGroup  },
            };

            var next = new StubRouter();
            var route = CreateAttributeRoute(next, entry);

            var context = CreateVirtualPathContext(new { action = "Index", controller = "Store", id = 5 });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("api/Store/Index/5", path);
            Assert.Equal(expectedValues, next.GenerationContext.ProvidedValues);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_NoMatch_WithConstraint()
        {
            // Arrange
            var entry = CreateGenerationEntry("api/Store/{action}/{id:int}", new { action = "Index", controller = "Store" });
            var route = CreateAttributeRoute(entry);

            var expectedValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", "5" },
                { AttributeRouting.RouteGroupKey, entry.RouteGroup  },
            };

            var next = new StubRouter();
            var context = CreateVirtualPathContext(new { action = "Index", controller = "Store", id = "heyyyy" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Null(path);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_Match_WithMixedAmbientValues()
        {
            // Arrange
            var entry = CreateGenerationEntry("api/Store", new { action = "Index", controller = "Store" });
            var route = CreateAttributeRoute(entry);

            var context = CreateVirtualPathContext(new { action = "Index" }, new { controller = "Store" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("api/Store", path);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_Match_WithQueryString()
        {
            // Arrange
            var entry = CreateGenerationEntry("api/Store", new { action = "Index", controller = "Store" });
            var route = CreateAttributeRoute(entry);

            var context = CreateVirtualPathContext(new { action = "Index", id = 5 }, new { controller = "Store" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("api/Store?id=5", path);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_ForwardsRouteGroup()
        {
            // Arrange
            var entry = CreateGenerationEntry("api/Store", new { action = "Index", controller = "Store" });

            var expectedValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { AttributeRouting.RouteGroupKey, entry.RouteGroup },
            };

            var next = new StubRouter();
            var route = CreateAttributeRoute(next, entry);

            var context = CreateVirtualPathContext(new { action = "Index", controller = "Store" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal(expectedValues, next.GenerationContext.ProvidedValues);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_RejectedByFirstRoute()
        {
            // Arrange
            var entry1 = CreateGenerationEntry("api/Store", new { action = "Index", controller = "Store" });
            var entry2 = CreateGenerationEntry("api2/{controller}", new { action = "Index", controller = "Blog" });

            var route = CreateAttributeRoute(entry1, entry2);

            var context = CreateVirtualPathContext(new { action = "Index", controller = "Blog" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("api2/Blog", path);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_RejectedByHandler()
        {
            // Arrange
            var entry1 = CreateGenerationEntry("api/Store", new { action = "Edit", controller = "Store" });
            var entry2 = CreateGenerationEntry("api2/{controller}", new { action = "Edit", controller = "Store" });

            var next = new StubRouter();

            var callCount = 0;
            next.GenerationDelegate = (VirtualPathContext c) =>
            {
                // Reject entry 1.
                callCount++;
                return !c.ProvidedValues.Contains(new KeyValuePair<string, object>(
                    AttributeRouting.RouteGroupKey,
                    entry1.RouteGroup));
            };

            var route = CreateAttributeRoute(next, entry1, entry2);

            var context = CreateVirtualPathContext(new { action = "Edit", controller = "Store" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("api2/Store", path);
            Assert.Equal(2, callCount);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_ToArea()
        {
            // Arrange
            var entry1 = CreateGenerationEntry("Help/Store", new { area = "Help", action = "Edit", controller = "Store" });
            entry1.Precedence = 1;

            var entry2 = CreateGenerationEntry("Store", new { area = (string)null, action = "Edit", controller = "Store" });
            entry2.Precedence = 2;

            var next = new StubRouter();

            var route = CreateAttributeRoute(next, entry1, entry2);

            var context = CreateVirtualPathContext(new { area = "Help", action = "Edit", controller = "Store" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("Help/Store", path);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_ToArea_PredecedenceReversed()
        {
            // Arrange
            var entry1 = CreateGenerationEntry("Help/Store", new { area = "Help", action = "Edit", controller = "Store" });
            entry1.Precedence = 2;

            var entry2 = CreateGenerationEntry("Store", new { area = (string)null, action = "Edit", controller = "Store" });
            entry2.Precedence = 1;

            var next = new StubRouter();

            var route = CreateAttributeRoute(next, entry1, entry2);

            var context = CreateVirtualPathContext(new { area = "Help", action = "Edit", controller = "Store" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("Help/Store", path);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_ToArea_WithAmbientValues()
        {
            // Arrange
            var entry1 = CreateGenerationEntry("Help/Store", new { area = "Help", action = "Edit", controller = "Store" });
            entry1.Precedence = 1;

            var entry2 = CreateGenerationEntry("Store", new { area = (string)null, action = "Edit", controller = "Store" });
            entry2.Precedence = 2;

            var next = new StubRouter();

            var route = CreateAttributeRoute(next, entry1, entry2);

            var context = CreateVirtualPathContext(
                values: new { action = "Edit", controller = "Store" },
                ambientValues: new { area = "Help" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("Help/Store", path);
        }

        [Fact]
        public void AttributeRoute_GenerateLink_OutOfArea_IgnoresAmbientValue()
        {
            // Arrange
            var entry1 = CreateGenerationEntry("Help/Store", new { area = "Help", action = "Edit", controller = "Store" });
            entry1.Precedence = 1;

            var entry2 = CreateGenerationEntry("Store", new { area = (string)null, action = "Edit", controller = "Store" });
            entry2.Precedence = 2;

            var next = new StubRouter();

            var route = CreateAttributeRoute(next, entry1, entry2);

            var context = CreateVirtualPathContext(
                values: new { action = "Edit", controller = "Store" },
                ambientValues: new { area = "Blog" });

            // Act
            var path = route.GetVirtualPath(context);

            // Assert
            Assert.Equal("Store", path);
        }

        private static VirtualPathContext CreateVirtualPathContext(object values, object ambientValues = null)
        {
            var httpContext = Mock.Of<HttpContext>();

            return new VirtualPathContext(
                httpContext,
                new RouteValueDictionary(ambientValues),
                new RouteValueDictionary(values));
        }

        private static AttributeRouteMatchingEntry CreateMatchingEntry(IRouter router, string template, int order)
        {
            var constraintResolver = CreateConstraintResolver();

            var routeTemplate = TemplateParser.Parse(template, constraintResolver);

            var entry = new AttributeRouteMatchingEntry();
            entry.Route = new TemplateRoute(router, template, constraintResolver);
            entry.Precedence = AttributeRoutePrecedence.Compute(routeTemplate);
            entry.Order = order;

            return entry;
        }

        private static AttributeRouteLinkGenerationEntry CreateGenerationEntry(string template, object requiredValues, int order = 0)
        {
            var constraintResolver = CreateConstraintResolver();

            var entry = new AttributeRouteLinkGenerationEntry();
            entry.TemplateText = template;
            entry.Template = TemplateParser.Parse(template, constraintResolver);

            var defaults = entry.Template.Parameters
                .Where(p => p.DefaultValue != null)
                .ToDictionary(p => p.Name, p => p.DefaultValue);

            var constraints = entry.Template.Parameters
                .Where(p => p.InlineConstraint != null)
                .ToDictionary(p => p.Name, p => p.InlineConstraint);

            entry.Constraints = constraints;
            entry.Defaults = defaults;
            entry.Binder = new TemplateBinder(entry.Template, defaults);
            entry.Order = order;
            entry.Precedence = AttributeRoutePrecedence.Compute(entry.Template);
            entry.RequiredLinkValues = new RouteValueDictionary(requiredValues);
            entry.RouteGroup = CreateRouteGroup(order, template);

            return entry;
        }

        private static string CreateRouteGroup(int order, string template)
        {
            return string.Format("{0}&{1}", order, template);
        }

        private static DefaultInlineConstraintResolver CreateConstraintResolver()
        {
            var services = Mock.Of<IServiceProvider>();

            var options = new RouteOptions();
            var optionsMock = new Mock<IOptionsAccessor<RouteOptions>>();
            optionsMock.SetupGet(o => o.Options).Returns(options);

            return new DefaultInlineConstraintResolver(services, optionsMock.Object);
        }

        private static AttributeRoute CreateAttributeRoute(AttributeRouteLinkGenerationEntry entry)
        {
            return CreateAttributeRoute(new StubRouter(), entry);
        }

        private static AttributeRoute CreateAttributeRoute(IRouter next, AttributeRouteLinkGenerationEntry entry)
        {
            return CreateAttributeRoute(next, new[] { entry });
        }

        private static AttributeRoute CreateAttributeRoute(params AttributeRouteLinkGenerationEntry[] entries)
        {
            return CreateAttributeRoute(new StubRouter(), entries);
        }

        private static AttributeRoute CreateAttributeRoute(IRouter next, params AttributeRouteLinkGenerationEntry[] entries)
        {
            return new AttributeRoute(
                next,
                Enumerable.Empty<AttributeRouteMatchingEntry>(),
                entries);
        }

        private class StubRouter : IRouter
        {
            public VirtualPathContext GenerationContext { get; set; }

            public Func<VirtualPathContext, bool> GenerationDelegate { get; set; }

            public RouteContext MatchingContext { get; set; }

            public Func<RouteContext, bool> MatchingDelegate { get; set; }

            public string GetVirtualPath(VirtualPathContext context)
            {
                GenerationContext = context;

                if (GenerationDelegate == null)
                {
                    context.IsBound = true;
                }
                else
                {
                    context.IsBound = GenerationDelegate(context);
                }

                return null;
            }

            public Task RouteAsync(RouteContext context)
            {
                if (MatchingDelegate == null)
                {
                    context.IsHandled = true;
                }
                else
                {
                    context.IsHandled = MatchingDelegate(context);
                }

                return Task.FromResult<object>(null);
            }
        }
    }
}