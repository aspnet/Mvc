﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Constraints;
using Microsoft.AspNet.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Builder
{
    public class MvcAreaRouteBuilderExtensionsTest
    {
        [Fact]
        public void MapAreaRoute_Simple()
        {
            // Arrange
            var builder = new RouteBuilder()
            {
                DefaultHandler = Mock.Of<IRouter>(),
                ServiceProvider = CreateServices(),
            };

            // Act
            builder.MapAreaRoute(name: null, areaName: "admin", template: "site/Admin/");

            // Assert
            var route = Assert.IsType<TemplateRoute>((Assert.Single(builder.Routes)));

            Assert.Null(route.Name);
            Assert.Equal("site/Admin/", route.RouteTemplate);
            Assert.Collection(
                route.Constraints.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal(kvp.Key, "area");
                    Assert.IsType<RegexRouteConstraint>(kvp.Value);
                });
            Assert.Empty(route.DataTokens);
            Assert.Collection(
                route.Defaults.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal(kvp.Key, "area");
                    Assert.Equal(kvp.Value, "admin");
                });
        }

        [Fact]
        public void MapAreaRoute_Defaults()
        {
            // Arrange
            var builder = new RouteBuilder()
            {
                DefaultHandler = Mock.Of<IRouter>(),
                ServiceProvider = CreateServices(),
            };

            // Act
            builder.MapAreaRoute(
                name: "admin_area",
                areaName: "admin",
                template: "site/Admin/",
                defaults: new { action = "Home" });

            // Assert
            var route = Assert.IsType<TemplateRoute>((Assert.Single(builder.Routes)));

            Assert.Equal("admin_area", route.Name);
            Assert.Equal("site/Admin/", route.RouteTemplate);
            Assert.Collection(
                route.Constraints.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal(kvp.Key, "area");
                    Assert.IsType<RegexRouteConstraint>(kvp.Value);
                });
            Assert.Empty(route.DataTokens);
            Assert.Collection(
                route.Defaults.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal(kvp.Key, "action");
                    Assert.Equal(kvp.Value, "Home");
                },
                kvp =>
                {
                    Assert.Equal(kvp.Key, "area");
                    Assert.Equal(kvp.Value, "admin");
                });
        }

        [Fact]
        public void MapAreaRoute_DefaultsAndConstraints()
        {
            // Arrange
            var builder = new RouteBuilder()
            {
                DefaultHandler = Mock.Of<IRouter>(),
                ServiceProvider = CreateServices(),
            };

            // Act
            builder.MapAreaRoute(
                name: "admin_area",
                areaName: "admin",
                template: "site/Admin/",
                defaults: new { action = "Home" },
                constraints: new { id = new IntRouteConstraint() });

            // Assert
            var route = Assert.IsType<TemplateRoute>((Assert.Single(builder.Routes)));

            Assert.Equal("admin_area", route.Name);
            Assert.Equal("site/Admin/", route.RouteTemplate);
            Assert.Collection(
                route.Constraints.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal(kvp.Key, "area");
                    Assert.IsType<RegexRouteConstraint>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(kvp.Key, "id");
                    Assert.IsType<IntRouteConstraint>(kvp.Value);
                });
            Assert.Empty(route.DataTokens);
            Assert.Collection(
                route.Defaults.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal(kvp.Key, "action");
                    Assert.Equal(kvp.Value, "Home");
                },
                kvp =>
                {
                    Assert.Equal(kvp.Key, "area");
                    Assert.Equal(kvp.Value, "admin");
                });
        }

        [Fact]
        public void MapAreaRoute_DefaultsConstraintsAndDataTokens()
        {
            // Arrange
            var builder = new RouteBuilder()
            {
                DefaultHandler = Mock.Of<IRouter>(),
                ServiceProvider = CreateServices(),
            };

            // Act
            builder.MapAreaRoute(
                name: "admin_area",
                areaName: "admin",
                template: "site/Admin/",
                defaults: new { action = "Home" },
                constraints: new { id = new IntRouteConstraint() },
                dataTokens: new { some_token = "hello" });

            // Assert
            var route = Assert.IsType<TemplateRoute>((Assert.Single(builder.Routes)));

            Assert.Equal("admin_area", route.Name);
            Assert.Equal("site/Admin/", route.RouteTemplate);
            Assert.Collection(
                route.Constraints.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal(kvp.Key, "area");
                    Assert.IsType<RegexRouteConstraint>(kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal(kvp.Key, "id");
                    Assert.IsType<IntRouteConstraint>(kvp.Value);
                });
            Assert.Collection(
                route.DataTokens.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal(kvp.Key, "some_token");
                    Assert.Equal(kvp.Value, "hello");
                });
            Assert.Collection(
                route.Defaults.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal(kvp.Key, "action");
                    Assert.Equal(kvp.Value, "Home");
                },
                kvp =>
                {
                    Assert.Equal(kvp.Key, "area");
                    Assert.Equal(kvp.Value, "admin");
                });
        }

        [Fact]
        public void MapAreaRoute_ReplacesValuesForArea()
        {
            // Arrange
            var builder = new RouteBuilder()
            {
                DefaultHandler = Mock.Of<IRouter>(),
                ServiceProvider = CreateServices(),
            };

            // Act
            builder.MapAreaRoute(
                name: "admin_area",
                areaName: "admin",
                template: "site/Admin/",
                defaults: new { area = "Home" },
                constraints: new { area = new IntRouteConstraint() },
                dataTokens: new { some_token = "hello" });

            // Assert
            var route = Assert.IsType<TemplateRoute>((Assert.Single(builder.Routes)));

            Assert.Equal("admin_area", route.Name);
            Assert.Equal("site/Admin/", route.RouteTemplate);
            Assert.Collection(
                route.Constraints.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal(kvp.Key, "area");
                    Assert.IsType<RegexRouteConstraint>(kvp.Value);
                });
            Assert.Collection(
                route.DataTokens.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal(kvp.Key, "some_token");
                    Assert.Equal(kvp.Value, "hello");
                });
            Assert.Collection(
                route.Defaults.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal(kvp.Key, "area");
                    Assert.Equal(kvp.Value, "admin");
                });
        }

        private IServiceProvider CreateServices()
        {
            var services = new ServiceCollection();
            services.AddRouting();
            return services.BuildServiceProvider();
        }
    }
}
