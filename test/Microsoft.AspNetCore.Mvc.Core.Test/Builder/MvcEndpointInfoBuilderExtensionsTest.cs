// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Core.Test.Builder
{
    public class MvcEndpointInfoBuilderExtensionsTest
    {
        #region MapAreaEndpoint
        [Fact]
        public void MapAreaEndpoint_Simple()
        {
            // Arrange
            var builder = CreateEndpointBuilder();

            // Act
            builder.MapAreaEndpoint(name: null, areaName: "admin", template: "site/Admin/");

            // Assert
            var endpointInfo = Assert.Single(builder.EndpointInfos);

            Assert.Null(endpointInfo.Name);
            Assert.Equal("site/Admin/", endpointInfo.Template);
            Assert.Empty(endpointInfo.DataTokens);
            Assert.Collection(
                endpointInfo.Defaults.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal("area", kvp.Key);
                    Assert.Equal("admin", kvp.Value);
                });
        }

        [Fact]
        public void MapAreaEndpoint_Defaults()
        {
            // Arrange
            var builder = CreateEndpointBuilder();

            // Act
            builder.MapAreaEndpoint(
                name: "admin_area",
                areaName: "admin",
                template: "site/Admin/",
                defaults: new { action = "Home" });

            // Assert
            var endpointInfo = Assert.Single(builder.EndpointInfos);

            Assert.Equal("admin_area", endpointInfo.Name);
            Assert.Equal("site/Admin/", endpointInfo.Template);
            Assert.Empty(endpointInfo.DataTokens);
            Assert.Collection(
                endpointInfo.Defaults.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal("action", kvp.Key);
                    Assert.Equal("Home", kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal("area", kvp.Key);
                    Assert.Equal("admin", kvp.Value);
                });
        }

        [Fact]
        public void MapAreaEndpoint_DefaultsAndConstraints()
        {
            // Arrange
            var builder = CreateEndpointBuilder();

            // Act
            builder.MapAreaEndpoint(
                name: "admin_area",
                areaName: "admin",
                template: "site/Admin/",
                defaults: new { action = "Home" },
                constraints: new { id = new IntRouteConstraint() });

            // Assert
            var endpointInfo = Assert.Single(builder.EndpointInfos);

            Assert.Equal("admin_area", endpointInfo.Name);
            Assert.Equal("site/Admin/", endpointInfo.Template);
            Assert.Empty(endpointInfo.DataTokens);
            Assert.Collection(
                endpointInfo.Defaults.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal("action", kvp.Key);
                    Assert.Equal("Home", kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal("area", kvp.Key);
                    Assert.Equal("admin", kvp.Value);
                });
        }

        [Fact]
        public void MapAreaEndpoint_DefaultsConstraintsAndDataTokens()
        {
            // Arrange
            var builder = CreateEndpointBuilder();

            // Act
            builder.MapAreaEndpoint(
                name: "admin_area",
                areaName: "admin",
                template: "site/Admin/",
                defaults: new { action = "Home" },
                constraints: new { id = new IntRouteConstraint() },
                dataTokens: new { some_token = "hello" });

            // Assert
            var endpointInfo = Assert.Single(builder.EndpointInfos);

            Assert.Equal("admin_area", endpointInfo.Name);
            Assert.Equal("site/Admin/", endpointInfo.Template);
            Assert.Collection(
                endpointInfo.DataTokens.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal("some_token", kvp.Key);
                    Assert.Equal("hello", kvp.Value);
                });
            Assert.Collection(
                endpointInfo.Defaults.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal("action", kvp.Key);
                    Assert.Equal("Home", kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal("area", kvp.Key);
                    Assert.Equal("admin", kvp.Value);
                });
        }

        [Fact]
        public void MapAreaEndpoint_DoesNotReplaceValuesForAreaIfAlreadyPresentInConstraintsOrDefaults()
        {
            // Arrange
            var builder = CreateEndpointBuilder();

            // Act
            builder.MapAreaEndpoint(
                name: "admin_area",
                areaName: "admin",
                template: "site/Admin/",
                defaults: new { area = "Home" },
                constraints: new { area = new IntRouteConstraint() },
                dataTokens: new { some_token = "hello" });

            // Assert
            var endpointInfo = Assert.Single(builder.EndpointInfos);

            Assert.Equal("admin_area", endpointInfo.Name);
            Assert.Equal("site/Admin/", endpointInfo.Template);
            Assert.Collection(
                endpointInfo.DataTokens.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal("some_token", kvp.Key);
                    Assert.Equal("hello", kvp.Value);
                });
            Assert.Collection(
                endpointInfo.Defaults.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal("area", kvp.Key);
                    Assert.Equal("Home", kvp.Value);
                });
        }

        [Fact]
        public void MapAreaEndpoint_UsesPassedInAreaNameAsIs()
        {
            // Arrange
            var builder = CreateEndpointBuilder();
            var areaName = "user.admin";

            // Act
            builder.MapAreaEndpoint(name: null, areaName: areaName, template: "site/Admin/");

            // Assert
            var endpointInfo = Assert.Single(builder.EndpointInfos);

            Assert.Null(endpointInfo.Name);
            Assert.Equal("site/Admin/", endpointInfo.Template);
            Assert.Empty(endpointInfo.DataTokens);
            Assert.Collection(
                endpointInfo.Defaults.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal("area", kvp.Key);
                    Assert.Equal(kvp.Value, areaName);
                });
        }
        #endregion

        private MvcEndpointInfoBuilder CreateEndpointBuilder()
        {
            var builder = new MvcEndpointInfoBuilder(Mock.Of<IInlineConstraintResolver>());
            return builder;
        }
    }
}